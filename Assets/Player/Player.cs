using Gamekit2D;
using HitboxSystem;
using ResourceManager;
using System;
using UnityEngine;

public enum PlayerDamageId
{
    LIGHT = 1, KNOCKBACK = 2
}

public class Player : MonoBehaviour
{
    public float Health = 100;
    public bool IsInvulnerable = false;

    private Hitbox hitbox;

    public CharacterController2D Controller;
    public Animator Animator;
    public PlayerAim Aim;
    public PlayerActionController ActionController;

    public PlayerItem CurrentItem;
    public PlayerItem[] EquippedItems;

    private const float THROW_FORCE = 10f;
    private const float THROW_AIM_MOD = .25f;

    private const float RUN_SPEED = 15f;
    private float x_input;
    private bool jump = false;
    private bool crouch = false;

    public bool stopInput = false;

    private bool attacking = false;
    public bool CanCancelAttackAnim = false;

    // Prevents the player from taking damage from the same source multiple times
    private Guid lastDamageId;

    // Thresholds for damage animations
    private const float DMG_KNOCKBACK_THRESHOLD = 30f;
    private const float KNOCKBACK_FORCE = 10f;
    private const float DMG_LIGHT_THRESHOLD = 10f;

    private void Awake()
    {
        // TODO Definitely not where loading should be, put here for now since no scene code is done yet
        EffectsManager.Instance.LoadEffectsBundle();
        ProjectileMananger.Instance.LoadProjectileBundle();

        ActionController = GetComponentInChildren<PlayerActionController>();
    }

    private void Start()
    {
        SceneLinkedSMB<Player>.Initialise(Animator, this);
        //TODO Hardcoded for now, needs to be assigned from the currently equipped weapon & item from an equipment class later
        ActionController.CurrentWeaponType = WeaponType.ONE_HAND;
        CurrentItem = PlayerItemLibrary.FireBomb;

        hitbox = GetComponent<Hitbox>();
        hitbox.Handler += new Hitbox.HitboxEventHandler(OnHit);
    }

    private void Update()
    {
        // Check for cancel animations in Update since they will be called before in order for the animator to cancel and act within the same frame
        if (Input.GetButtonDown("MainWeaponAction") || Input.GetButtonDown("Equipment") || Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0 && !stopInput)
        {
            ApplyAttackAnimationCancel();
        }
        // Override animation cancel checks for maximum reactiveness in controls
        if (Input.GetButtonDown("Jump") || Input.GetButtonDown("Crouch") && !stopInput)
        {
            if (isAttacking && Controller.IsGrounded)
            {
                ApplyAttackAnimationCancel(true);
            }
        }
    }

    void LateUpdate()
    {
        // All inputs should go into this conditional
        if (!stopInput)
        {
            if (Input.GetButtonDown("Jump") && Controller.IsGrounded && !isAttacking)
            {
                jump = true;
            }

            if (Input.GetButtonDown("Crouch") && Controller.IsGrounded)
            {
                if (!crouch) crouch = true;
            }
            else if (Input.GetButtonUp("Crouch"))
            {
                crouch = false;
            }

            if (Input.GetButtonDown("MainWeaponAction") && !isAttacking)
            {
                MainWeaponAction();
            }

            if (Input.GetButtonDown("Equipment") && !isAttacking)
            {
                UseEquipment();
            }

            x_input = attacking || crouch || stopInput ? 0 : Input.GetAxisRaw("Horizontal") * RUN_SPEED;
        }

        Animator.SetFloat("Speed", Mathf.Abs(x_input));
        Animator.SetBool("IsCrouching", crouch);
        Animator.SetBool("IsAttacking", attacking);
        Animator.SetBool("IsGrounded", Controller.IsGrounded);
        Animator.SetFloat("yVelocity", Controller.Velocity.y);
    }

    private void ApplyAttackAnimationCancel(bool overrideCheck = false)
    {
        if (CanCancelAttackAnim || overrideCheck)
        {
            Animator.SetTrigger("CancelAnimation");
            CanCancelAttackAnim = false;
        }
    }

    void FixedUpdate()
    {
        Controller.Move(x_input * Time.fixedDeltaTime, crouch, jump);
        jump = false;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        Hitbox hitbox = col.GetComponent<Hitbox>();
        if (hitbox != null && hitbox.IsActive)
        {
            ApplyDamage(hitbox.ActiveHitBoxDamage);
        }
    }

    private void OnHit(object sender, HitboxEventArgs e)
    {
        ApplyDamage(e.Damage);
    }

    private void ApplyDamage(in Damage dmg)
    {
        if (!dmg.ID.Equals(lastDamageId) && !IsInvulnerable)
        {
            ApplyAttackAnimationCancel(true);
            EndAttack();
            lastDamageId = dmg.ID;
            // TODO Apply damage type mitigation to Player based on armor
            float damage = dmg.Value;
            Vector3 dmgPos = dmg.Position;
            Vector3 forceDir = Vector3.zero;

            Animator.SetInteger("DamageId", 0);
            if (DMG_KNOCKBACK_THRESHOLD <= damage || !Controller.IsGrounded)
            {
                // Face towards damage and apply force direction
                if (dmgPos.x >= transform.position.x)
                {
                    forceDir = Vector3.left;
                    if (!Controller.FacingRight) Controller.Flip();
                }
                else if (dmgPos.x <= transform.position.x)
                {
                    forceDir = Vector3.right;
                    if (Controller.FacingRight) Controller.Flip();
                }
                // Apply force away from damage position
                Controller.ApplyImpulse(KNOCKBACK_FORCE, forceDir);
                // Set animation ID
                Animator.SetInteger("DamageId", (int)PlayerDamageId.KNOCKBACK);
            }
            else if (DMG_LIGHT_THRESHOLD <= damage && DMG_KNOCKBACK_THRESHOLD >= damage)
            {
                Animator.SetInteger("DamageId", (int)PlayerDamageId.LIGHT);
            }
            Animator.SetTrigger("Damage");
            // TODO Apply Hit spark here

            Health -= damage;
        }
    }

    public void OnLanding()
    {
        if (isAttacking)
        {
            ApplyAttackAnimationCancel(true);
        }
    }

    private void MainWeaponAction()
    {
        //Face aim direction before attacking
        if (Controller.FacingRight && (90 < Aim.AimAngle && Aim.AimAngle < 270))
        {
            Controller.Flip();
        }
        else if (!Controller.FacingRight && (90 > Aim.AimAngle && Aim.AimAngle >= 0 || Aim.AimAngle > 270))
        {
            Controller.Flip();
        }
        attacking = true;
        Animator.SetInteger("Aim", (int)Aim.ToEnum);
        Animator.SetTrigger("WeaponAction");
    }

    // Peforms a fatal attack, a cinematic attack that does large damage independent of the player weapon on a staggered creature
    public void FatalAttack(in CreatureSystems.Creature creature)
    {
        if (creature != null)
        {
            ApplyAttackAnimationCancel(true);
            attacking = true;
            ActionController.CurrentNonWeaponAttackID = ActionLibrary.FATAL_ATK_ID;
            ActionController.fatalAttackCreature = creature;
            Animator.SetInteger("ActionId", ActionController.CurrentNonWeaponAttackID);
            Animator.SetTrigger("Action");
        }
    }
    // Throw currently equipped item
    public void ThrowItem()
    {
        if (CurrentItem.Type.Equals(ItemType.THROW))
        {
            // Grab player aim and apply mod to account for throw arc
            Vector2 aim = new Vector2(Aim.ToVector.x, Aim.ToVector.y + THROW_AIM_MOD);
            // Spawn item to be thrown
            GameObject item = Instantiate(CurrentItem.Prefab);
            item.transform.position = this.transform.position;
            // Ignore collision with player colliders, this could probably be done better
            BoxCollider2D itemCol = item.GetComponent<BoxCollider2D>();
            Physics2D.IgnoreCollision(itemCol, GetComponent<BoxCollider2D>());
            Physics2D.IgnoreCollision(itemCol, GetComponent<CircleCollider2D>());
            // Apply throw force to item
            item.GetComponent<Rigidbody2D>().AddForce(aim * THROW_FORCE, ForceMode2D.Impulse);
        }
    }

    private void SecondaryWeaponAction() { }

    private void UseEquipment()
    {
        stopInput = true;
        if (CurrentItem.Type.Equals(ItemType.THROW))
        {
            // Set if item is a throwable
            if (Controller.FacingRight && (90 < Aim.AimAngle && Aim.AimAngle < 270))
            {
                Controller.Flip();
            }
            else if (!Controller.FacingRight && (90 > Aim.AimAngle && Aim.AimAngle >= 0 || Aim.AimAngle > 270))
            {
                Controller.Flip();
            }
            ActionController.CurrentNonWeaponAttackID = ActionLibrary.THROW_ID;
            Animator.SetInteger("Aim", (int)Aim.ToEnum);
            Animator.SetInteger("ActionId", ActionController.CurrentNonWeaponAttackID);
            Animator.SetTrigger("Action");
        }
    }

    public void EndAttack()
    {
        attacking = false;
        ActionController.EndAttackOrAction();
        CanCancelAttackAnim = false;
    }

    public bool isAttacking { get { return attacking; } }
    public bool isCrouching { get { return crouch; } }
}
