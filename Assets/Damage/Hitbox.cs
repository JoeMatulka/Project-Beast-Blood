using System;
using UnityEngine;

namespace HitboxSystem
{
    public class HitboxEventArgs : EventArgs
    {
        private readonly Damage damage;

        public HitboxEventArgs(Damage damage)
        {
            this.damage = damage;
        }

        public Damage Damage
        {
            get { return damage; }
        }
    }

    [RequireComponent(typeof(BoxCollider2D))]
    public class Hitbox : MonoBehaviour
    {
        // Prevents the hitbox from taking damage from the same source multiple times
        private Guid lastDamageId;

        private BoxCollider2D collider;

        public bool IsActive = false;

        public Damage ActiveHitBoxDamage;

        public delegate void HitboxEventHandler(object sender, HitboxEventArgs e);
        private event HitboxEventHandler handler;

        private void Awake()
        {
            collider = this.GetComponent<BoxCollider2D>();
        }

        public void ReceiveDamage(Damage dmg)
        {
            if (!dmg.ID.Equals(lastDamageId))
            {
                Debug.Log(name + " damaged");
                lastDamageId = dmg.ID;
                HitboxEventArgs e = new HitboxEventArgs(dmg);
                Handler?.Invoke(this, e);
            }
        }

        public HitboxEventHandler Handler
        {
            get { return handler; }
            set { handler = value; }
        }

        public BoxCollider2D Collider
        {
            get { return collider; }
        }
    }
}
