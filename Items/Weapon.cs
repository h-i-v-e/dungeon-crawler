using UnityEngine;

namespace Depravity
{

    public abstract class Weapon : Holdable
    {
        public const int NUMBER_OF_ANIMATION_TYPES = 11;

        public enum AnimationType
        {
            UNARMED = 0,
            AXE_2H = 1,
            BOW_2H = 2,
            XBOW_2H = 3,
            SPEAR_2H = 4,
            SWORD_2H = 5,
            DAGGER = 6,
            MACE = 7,
            SPEAR = 8,
            STAFF = 9,
            SWORD = 10
        }

        public enum DamageType
        {
            SLASH, STAB, BASH, BURN, PSYCH
        };

        public override Hand GetHand()
        {
            return Hand.RIGHT;
        }

        public float minimumReach, maximumReach, slowness = 1.0f;

        public abstract AnimationType GetAnimationType();

        public virtual void Shoot() { }

        public override int GetPhysicsLayer()
        {
            return Controller.WeaponLayer;
        }

        public override void Bind()
        {
            Wielder.AnimationManager.SetArmedWith(GetAnimationType());
            Wielder.weapon = this;
        }

        protected override void OnHeldStateChanged(bool held)
        {
            var collider = GetComponent<Collider>();
            if (collider)
            {
                GetComponent<Collider>().isTrigger = dropCollider != null || held;
            }
            if (held)
            {
                Bind();
                gameObject.layer = GetPhysicsLayer();
            }
            else if (Wielder != null)
            {
                Wielder.AnimationManager.SetArmedWith(0);
                Wielder.weapon = null;
                gameObject.layer = 0;
            }
        }
    }
}
