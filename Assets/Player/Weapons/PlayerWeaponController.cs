using CreatureSystems;
using Gamekit2D;
using HitboxSystem;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerWeaponController : MonoBehaviour
{
    private Player player;

    public Animator Animator;

    private SpriteRenderer m_renderer;

    // Number key in dictionary is active frame for weapon attack (zero indexed)
    public Dictionary<int, WeaponAttackFrame> WeaponAttackFrames;
    // Length of ray cast when weapon attacks
    private float weaponAttackRayLength;
    private readonly float diagonalWeaponRayMod = .25f;
    private readonly float playerCenterOffset = .2f;
    private readonly float playerCrouchOffect = .15f;

    private Vector2 defaultPosition;
    private Vector2 crouchPosition;

    private Sprite holsteredWeaponSprite;

    public Sprite OneHandedSwordHolsteredSprite;

    void Awake()
    {
        player = this.GetComponentInParent<Player>();

        m_renderer = this.GetComponent<SpriteRenderer>();

        defaultPosition = this.transform.localPosition;
        crouchPosition = new Vector2(defaultPosition.x, defaultPosition.y - playerCrouchOffect);

        AssignAttackFrames();
        AssignHolsteredSprite();
        SetHolsteredWeaponSprite();
    }

    private void Start()
    {
        ApplyWeaponEffects();
    }

    void Update()
    {
        //Change position if player is crouching
        this.transform.localPosition = player.IsCrouching ? crouchPosition : defaultPosition;
    }

    public void SetHolsteredWeaponSprite()
    {
        Animator.enabled = false;
        m_renderer.sprite = holsteredWeaponSprite;
    }

    public void HideHolsteredWeapon()
    {
        m_renderer.sprite = null;
    }

    public void DrawWeaponRaycast(Vector2 direction, LayerMask playerLayerMask)
    {
        //Determine Vector Direction based off of Aim Direction (It's not part of the enum since the AimDirection is used by the animator)
        Vector3 rayDirection = direction;
        float attackLength = weaponAttackRayLength;
        // Activate weapon raycast from offset of center of player
        Vector2 center = transform.position + (rayDirection * playerCenterOffset);

        // Adjust for diagonal raycasts
        if (Mathf.Abs(rayDirection.x) == 1 && Mathf.Abs(rayDirection.y) == 1)
        {
            // This is because rays are drawn longer at a diagonal angle from origin
            attackLength -= diagonalWeaponRayMod;

            center = rayDirection.y > 0 ? new Vector2(center.x, center.y + diagonalWeaponRayMod) : new Vector2(center.x, center.y - diagonalWeaponRayMod);
        }

        RaycastHit2D[] hits = Physics2D.RaycastAll(center, rayDirection, attackLength, playerLayerMask);
        Debug.DrawRay(center, rayDirection * attackLength, Color.green);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null)
            {
                // If the hit has a hitbox to receive damage, then damage it
                hit.collider.GetComponent<Hitbox>()?.ReceiveDamage(player.EquippedWeapon.Damage, hit.point);
                // If the hit is a creature that is staggered, perform a fatal attack
                Creature creature = hit.collider.transform.root.GetComponent<Creature>();
                if (creature != null && creature.IsStaggered)
                {
                    player.stopInput = true;
                    player.FatalAttack(creature);
                }
            }
        }
    }


    public void AssignAttackFrames()
    {
        // Assign weapon attack frames from the set current weapon type
        switch (player.EquippedWeapon.Type)
        {
            case WeaponType.ONE_HAND:
                WeaponAttackFrames = WeaponAttackFrameLibrary.ONE_HAND_ATK_FRAMES;
                weaponAttackRayLength = WeaponAttackFrameLibrary.ONE_HAND_ATK_WEAPON_LENGTH;
                break;
            default:
                Debug.LogError("Could not find that weapon type, cannot assign weapon attack frames");
                break;
        }

    }

    public void AssignHolsteredSprite()
    {
        if (player.EquippedWeapon.Sprite.Equals(WeaponSpriteType.SWORD) && player.EquippedWeapon.Type.Equals(WeaponType.ONE_HAND))
        {
            holsteredWeaponSprite = OneHandedSwordHolsteredSprite;
        }
    }

    public void ActivateWeaponAttackAnimation(int aim)
    {
        Animator.enabled = true;
        Animator.SetTrigger("WeaponAction");
        Animator.SetInteger("Aim", aim);
        Animator.SetInteger("WeaponType", (int)player.EquippedWeapon.Type);
        Animator.SetInteger("WeaponSpriteType", (int)player.EquippedWeapon.Sprite);
    }

    public void ApplyWeaponEffects()
    {
        m_renderer.materials = player.EquippedWeapon.Materials;
    }

    public void EndWeaponAttack()
    {
        SetHolsteredWeaponSprite();
    }
}

/**
 * Class meant to represent a single frame of attack within an weapon attack animation frame
 */
public struct WeaponAttackFrame
{
    // Is this the end of the recovery frame, allows for cancelling animations with movement, etc.
    public readonly bool IsEndOfRecoveryFrame;
    // Is the weapon hurt box active this frame?
    public readonly bool IsActiveHurtBox;

    public WeaponAttackFrame(bool isActiveHurtBox, bool isEndOfRecoveryFrame)
    {
        IsEndOfRecoveryFrame = isEndOfRecoveryFrame;
        IsActiveHurtBox = isActiveHurtBox;
    }
}

/**
 * Class meant to hold the weapon type data by archtype, not intended for specific weapons, but for specific weapon types
 */
public static class WeaponAttackFrameLibrary
{
    // One handed weapon
    public static Dictionary<int, WeaponAttackFrame> ONE_HAND_ATK_FRAMES = new Dictionary<int, WeaponAttackFrame> {
        { 5, new WeaponAttackFrame(true, false) },
        { 12, new WeaponAttackFrame(false, true) },
    };
    public static float ONE_HAND_ATK_WEAPON_LENGTH = .45f;
}
