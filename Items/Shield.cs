using UnityEngine;

namespace Depravity
{
    [RequireComponent(
        typeof(Rigidbody),
        typeof(Collider)
    )]
    public class Shield : Holdable
    {
        //private new Collider collider;

        public override Hand GetHand()
        {
            return Hand.LEFT;
        }

        public override int GetPhysicsLayer()
        {
            return Controller.ShieldLayer;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!Held)
            {
                return;
            }
            var weapon = other.GetComponent<Weapon>();
            if (weapon == null)
            {
                return;
            }
            var wielder = weapon.Wielder;
            if (wielder != null) {
                Wielder.NotifyUnderAttack(wielder);
                wielder.AnimationManager.Blocked();
                Wielder.AnimationManager.Block();
            }
        }

        protected override void OnHeldStateChanged(bool held) {
            if (Wielder == null)
            {
                return;
            }
            Wielder.AnimationManager.CarryingShield = held;
            Wielder.shield = held ? this : null;
            //gameObject.layer = GetPhysicsLayer();
            //collider.isTrigger = dropCollider != null;
            /*if (Wielder != null)
            {
                Physics.IgnoreCollision(collider, Wielder.Collider, held);
                Wielder.AnimationManager.CarryingShield = held;
                Wielder.shield = held ? this : null;
                collider.isTrigger = dropCollider != null;
            }*/
        }

        /*private void Awake()
        {
            gameObject.layer = GetPhysicsLayer();
            //collider = GetComponent<Collider>()
            //;
        }*/

        public override void Bind()
        {
            Wielder.AnimationManager.CarryingShield = true;
        }
    }
}