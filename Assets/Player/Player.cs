using Gamekit2D;
using HitboxSystem;
using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Prevents the player from taking damage from the same source multiple times
    private Guid lastDamageId;

    public CharacterController2D Controller;
    public Animator Animator;
    public PlayerAim Aim;
    public PlayerWeapon Weapon;

    private const float RUN_SPEED = 15f;
    private float x_input;
    private bool jump = false;
    private bool crouch = false;

    private bool isAttacking = false;

    // Ability to withstand damage without being staggered
    private bool isSuperArmor = false;
    // TODO Set up Super Armor Thresholds, think of how poise works in Dark Souls 3

    private void Start()
    {
        SceneLinkedSMB<Player>.Initialise(Animator, this);
        //TODO Hardcoded for now, needs to be assigned from the currently equipped weapon from an equipment class later
        Weapon.CurrentWeaponType = WeaponType.ONE_HAND;
        Debug.Log("Creature parts are registering multiple hits from the same source");
    }

    void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            jump = true;
            Animator.SetBool("IsJumping", true);
            // TODO Cancel some attack animations with jump
        }

        if (Input.GetButtonDown("Crouch"))
        {
            crouch = true;
        }
        else if (Input.GetButtonUp("Crouch"))
        {
            crouch = false;
        }

        if (Input.GetButtonDown("MainWeaponAction"))
        {
            MainWeaponAction();
        }

        x_input = (isAttacking && Controller.IsGrounded) || crouch ? 0 : Input.GetAxisRaw("Horizontal") * RUN_SPEED;

        Animator.SetFloat("Speed", Mathf.Abs(x_input));
        Animator.SetBool("IsCrouching", crouch);
        Animator.SetBool("IsAttacking", isAttacking);
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

    private void ApplyDamage(Damage dmg)
    {
        if (!dmg.ID.Equals(lastDamageId))
        {
            lastDamageId = dmg.ID;
            // TODO Apply damage to Player
        }
    }

    public void OnLanding()
    {
        Animator.SetBool("IsJumping", false);
        // TODO Cancel some attack animations on landing
    }

    public void MainWeaponAction()
    {
        if (!isAttacking)
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
            isAttacking = true;
            Animator.SetInteger("Aim", (int)Aim.AimDirection);
            Animator.SetTrigger("WeaponAction");
        }
    }

    public void SecondaryWeaponAction() { }

    public void UseEquipment() { }

    public void EndAttack()
    {
        isAttacking = false;
        Weapon.EndAttack();
    }
}
