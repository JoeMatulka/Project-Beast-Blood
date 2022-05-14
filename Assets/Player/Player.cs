using Gamekit2D;
using HitboxSystem;
using ResourceManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum PlayerDamageId
{
    LIGHT = 1, KNOCKBACK = 2
}

public class Player : MonoBehaviour
{
    public readonly float MAX_HEALTH = 100;
    public float Health = 0;
    public bool IsInvulnerable = false;

    private Hitbox hitbox;

    public CharacterController2D Controller;
    public Animator Animator;
    public PlayerAim Aim;
    public PlayerActionController ActionController;

    public PlayerWeapon EquippedWeapon;

    // Dictionary is the key being the current item in the inventory, the tuple being first the amount in the inventory and the second being the max amount
    public Dictionary<PlayerItem, Tuple<int, int>> EquippedItems;
    public int CurrentEquippedItemIndex = 0;
    private PlayerItem itemCurrentInUse;

    private const float THROW_FORCE = 10f;
    private const float THROW_AIM_MOD = .25f;

    private const float RUN_SPEED = 15f;
    private float x_input;
    private bool jump = false;
    private bool crouch = false;

    public bool stopInput = false;

    private bool attacking = false;
    private bool doingAction = false;
    public bool CanCancelAnim = false;

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
        PlayerItemMananger.Instance.LoadPlayerItemBundle();

        ActionController = GetComponentInChildren<PlayerActionController>();
        Health = MAX_HEALTH;
    }

    private void Start()
    {
        SceneLinkedSMB<Player>.Initialise(Animator, this);

        // TODO These should come from some inventory load out in the future
        EquippedWeapon = PlayerWeaponLibrary.Weapons[PlayerWeaponLibrary.IRON_SWORD_ID];
        EquippedItems = new Dictionary<PlayerItem, Tuple<int, int>>()
        {
            { PlayerItemLibrary.Items[PlayerItemLibrary.FIREBOMB_ID], new Tuple<int, int>(PlayerItemLibrary.GetItemStackSize(ItemType.THROW), PlayerItemLibrary.GetItemStackSize(ItemType.THROW)) },
            { PlayerItemLibrary.Items[PlayerItemLibrary.MEDICINE_ID], new Tuple<int, int>(PlayerItemLibrary.GetItemStackSize(ItemType.CONSUME), PlayerItemLibrary.GetItemStackSize(ItemType.CONSUME)) },
        };

        // Set up hitbox listener for damage events
        hitbox = GetComponent<Hitbox>();
        hitbox.Handler += new Hitbox.HitboxEventHandler(OnHit);
    }

    private void Update()
    {
        // Lock flipping on controller for Air controls
        Controller.LockFlipping = IsAttacking || IsDoingAction;

        // Check for cancel animations in Update since they will be called before in order for the animator to cancel and act within the same frame
        if (Input.GetButtonDown("MainWeaponAction") || Input.GetButtonDown("Equipment") || Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0 && !stopInput)
        {
            ApplyAttackAnimationCancel();
        }
        // Override animation cancel checks for maximum reactiveness in controls
        if (Input.GetButtonDown("Jump") || Input.GetButtonDown("Crouch") && !stopInput)
        {
            if ((IsAttacking || IsDoingAction) && Controller.IsGrounded)
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
            if (Input.GetButtonDown("Jump") && Controller.IsGrounded)
            {
                jump = true;
            }

            if (Input.GetButtonDown("Crouch") && Controller.IsGrounded)
            {
                if (!crouch) crouch = true;
            }
            else if (Input.GetButtonUp("Crouch"))
            {
                StartCoroutine(Uncrouch());
            }

            if (Input.GetButtonDown("MainWeaponAction") && !IsAttacking && !IsDoingAction)
            {
                MainWeaponAction();
            }

            if (Input.GetAxis("ItemChange") != 0)
            {
                ChangeItemIndex(Input.GetAxis("ItemChange") > 0);
            }

            if (Input.GetButtonDown("Equipment") && !IsAttacking && !IsDoingAction)
            {
                UseEquipment();
            }

            x_input = (IsAttacking && Controller.IsGrounded) || crouch || stopInput || (IsDoingAction && Controller.IsGrounded) ? 0 : Input.GetAxisRaw("Horizontal") * RUN_SPEED;
        }

        Animator.SetFloat("Speed", Mathf.Abs(x_input));
        Animator.SetBool("IsCrouching", crouch);
        Animator.SetBool("IsGrounded", Controller.IsGrounded);
        Animator.SetFloat("yVelocity", Controller.Velocity.y);
    }

    // Since button up is called in an update cycle and it needs to wait until action and attack is false, do in a Coroutine
    private IEnumerator Uncrouch()
    {
        yield return new WaitUntil(() => IsDoingAction == false && IsAttacking == false);
        crouch = false;
    }

    private void ApplyAttackAnimationCancel(bool overrideCheck = false)
    {
        if (CanCancelAnim || overrideCheck)
        {
            ActionController.ActivateAttackCancelAnimation();
            // Unset can cancel for next cancel call
            CanCancelAnim = false;
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
            lastDamageId = dmg.ID;
            // TODO Apply damage type mitigation to Player based on armor
            float damage = dmg.Value;
            Vector3 dmgPos = dmg.Position;
            Vector3 forceDir = Vector3.zero;

            // If no damage values are being applied return
            if (damage <= 0) return;

            Animator.SetInteger("DamageId", 0);
            if (DMG_KNOCKBACK_THRESHOLD <= damage || !Controller.IsGrounded)
            {
                ClearActionsAndAttacksFromDamage();
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
                ClearActionsAndAttacksFromDamage();
                Animator.SetInteger("DamageId", (int)PlayerDamageId.LIGHT);
            }
            Animator.SetTrigger("Damage");

            // spawn hit spark
            if (dmg.Type.Equals(DamageElementType.FIRE))
            {
                // Spawn fire spark for hit effect
                GameObject spark = EffectsManager.Instance.Spark;
                Instantiate(spark, this.transform.position, Quaternion.identity, this.transform);
            }
            else if (dmg.Type.Equals(DamageElementType.POISON))
            {
                // Spawn poison puff for hit effect
                GameObject puff = EffectsManager.Instance.PoisonPuff;
                Instantiate(puff, this.transform.position, Quaternion.identity, this.transform);
            }
            GameObject splash = EffectsManager.Instance.BloodSplashSmall;
            Instantiate(splash, this.transform.position, Quaternion.identity, this.transform);
            Health -= damage;
        }
    }

    // Used to ensure you are entering a new animation with a clear animator
    private void ClearActionsAndAttacksFromDamage() {
        // Restart damage animation from first
        Animator.ResetTrigger("Damage");

        // Cancel any animations currently happening
        ApplyAttackAnimationCancel(true);
        EndAttack();
        EndAction();
    }

    public void OnLanding()
    {

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
        ActionController.ActivateMainWeaponAction();
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
        if (itemCurrentInUse != null && itemCurrentInUse.Type.Equals(ItemType.THROW))
        {
            // Grab player aim and apply mod to account for throw arc
            Vector2 aim = new Vector2(Aim.ToVector.x, Aim.ToVector.y + THROW_AIM_MOD);
            // Spawn item to be thrown
            GameObject item = Instantiate(itemCurrentInUse.Prefab);
            item.transform.position = this.transform.position;
            // Ignore collision with player colliders, this could probably be done better
            BoxCollider2D itemCol = item.GetComponent<BoxCollider2D>();
            Physics2D.IgnoreCollision(itemCol, GetComponent<BoxCollider2D>());
            Physics2D.IgnoreCollision(itemCol, GetComponent<CircleCollider2D>());
            // Apply throw force to item
            item.GetComponent<Rigidbody2D>().AddForce(aim * THROW_FORCE, ForceMode2D.Impulse);
            RemoveItemCountFromCurrentEquippedItem();
        }
    }

    // Consume currently equipped item
    public void ConsumeItem()
    {
        if (itemCurrentInUse != null && itemCurrentInUse.Type.Equals(ItemType.CONSUME))
        {
            GameObject item = Instantiate(itemCurrentInUse.Prefab);
            item.transform.position = this.transform.position;
            item.transform.parent = this.transform;
            RemoveItemCountFromCurrentEquippedItem();
        }
    }

    private void ChangeItemIndex(bool increaseIndex)
    {
        int newIndex = 0;
        // increase index
        if (increaseIndex)
        {
            newIndex = CurrentEquippedItemIndex + 1;
            // Start from first index if we go up from the max count
            if (newIndex >= EquippedItems.Count) { newIndex = 0; }
        }
        //decrease index
        else
        {
            newIndex = CurrentEquippedItemIndex - 1;
            // Start from last index if we go below 0
            if (newIndex < 0) { newIndex = EquippedItems.Count - 1; }
        }
        CurrentEquippedItemIndex = newIndex;
    }

    private void RemoveItemCountFromCurrentEquippedItem()
    {
        // Get current item uses by the item currently being used
        Tuple<int, int> uses = EquippedItems[itemCurrentInUse];
        // Subtract one of the uses from the dictionary
        int amountOfUses = uses.Item1;
        amountOfUses = amountOfUses - 1;
        // Assign that back to the equipped items dictionary
        EquippedItems[itemCurrentInUse] = new Tuple<int, int>(amountOfUses, uses.Item2);
    }

    private void SecondaryWeaponAction() { }

    private void UseEquipment()
    {
        if (EquippedItems.Count > 0)
        {
            KeyValuePair<PlayerItem, Tuple<int, int>> currentItem = EquippedItems.ElementAt(CurrentEquippedItemIndex);
            if (currentItem.Value.Item1 > 0)
            {
                doingAction = true;
                itemCurrentInUse = currentItem.Key;
                switch (itemCurrentInUse.Type)
                {
                    case ItemType.THROW:
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
                        break;
                    case ItemType.CONSUME:
                        ActionController.CurrentNonWeaponAttackID = ActionLibrary.CONSUME_ID;
                        Animator.SetInteger("ActionId", ActionController.CurrentNonWeaponAttackID);
                        Animator.SetTrigger("Action");
                        break;
                    default:
                        break;
                }
            }
        }
    }

    public void EndAttack()
    {
        attacking = false;
        ActionController.EndAttackOrAction();
        CanCancelAnim = false;
    }

    public void EndAction()
    {
        doingAction = false;
        ActionController.EndAttackOrAction();
        CanCancelAnim = false;
        itemCurrentInUse = null;
    }

    public bool IsAttacking { get { return attacking; } }
    public bool IsDoingAction { get { return doingAction; } }
    public bool IsCrouching { get { return crouch; } }
}
