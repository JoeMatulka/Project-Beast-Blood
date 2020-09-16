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

        public Damage Damage {
            get { return damage; }
        }
    }

    [RequireComponent(typeof(BoxCollider2D))]
    public class Hitbox : MonoBehaviour
    {
        public delegate void HitboxEventHandler(object sender, HitboxEventArgs e);
        private event HitboxEventHandler handler;

        public void Hit(Damage dmg) {
            HitboxEventArgs e = new HitboxEventArgs(dmg);
            Handler?.Invoke(this, e);
        }

        public HitboxEventHandler Handler
        {
            get { return handler; }
            set { handler = value; }
        }
    }
}
