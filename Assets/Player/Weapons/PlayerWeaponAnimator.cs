using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(SpriteRenderer))]
public class PlayerWeaponAnimator : MonoBehaviour
{
    public Sprite[] sprites;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = this.GetComponent<SpriteRenderer>();
    }

    public void LoadSpritesForWeaponId(string weaponId) { 
    
    }

    public void SetSpriteByDirectionAndIndex(Vector2 direction, int index) {
        //spriteRenderer.sprite = sprites[index];
        // TODO account for animation from direction
    }

    public void ClearSprite() {
        spriteRenderer.sprite = null;
    }
}
