using UnityEngine;

namespace Depravity
{
    internal class PlayerLookatTarget : LookAtTarget
    {
        internal Vector3 target;

        public Vector3 GetPosition()
        {
            return target;
        }
    }

    [RequireComponent(typeof(Monster))]
    public class PlayerController : MonoBehaviour
    {
        [Range(0.0f, 1000.0f)]
        public float turnSpeed = 300.0f;

        private float checkHighlight = 0.0f;
        private Transform cameraTransform;
        private Monster monster;
        private Holdable highlighted;
        private readonly PlayerLookatTarget lookatTarget = new PlayerLookatTarget();

        public Monster ChatWith { get; set; }

        private void Die(Monster monster)
        {
            enabled = false;
        }

        private void Revived(Monster monster)
        {
            enabled = true;
        }

        private void Awake()
        {
            monster = GetComponent<Monster>();
            monster.OnDied += Die;
            monster.OnRevived += Revived;
            cameraTransform = GetComponentInChildren<Camera>().gameObject.transform;
        }

        private void ClearHighlighted()
        {
            highlighted.Highlighted = false;
            highlighted = null;
            Controller.HideMessage();
        }

        private void LookForHoldableItems()
        {
            if (Dungeon.Instance.FindBlockAt(monster.BlockOffset, out var block)){
                highlighted = block.gameObject.GetComponentInChildren<Holdable>();
                if (highlighted != null)
                {
                    Controller.ShowMessage(highlighted is Weapon ? "Pick up weapon (F)" : "Pick up shield (F)");
                }
            }
        }

        private void CheckTwoHanded(Holdable.Hand hand)
        {
            if (highlighted.IsTwoHanded)
            {
                if (hand == Holdable.Hand.RIGHT)
                {
                    monster.InLeftHand = null;
                }
                else
                {
                    monster.InRightHand = null;
                }
                return;
            }
            var other = hand == Holdable.Hand.RIGHT ? monster.InLeftHand : monster.InRightHand;
            if (other != null && other.IsTwoHanded)
            {
                other.Drop();
            }
        }

        private void Pickup()
        {
            var hand = highlighted.GetHand();
            CheckTwoHanded(hand);
            if (hand == Holdable.Hand.RIGHT)
            {
                highlighted.AddToRightHandOf(monster);
            }
            else
            {
                highlighted.AddToLeftHandOf(monster);
            }
            ClearHighlighted();
        }

        private void SetBlocking(bool blocking)
        {
            monster.AnimationManager.Blocking = blocking;
        }

        private void Chat()
        {
            var animator = monster.AnimationManager;
            var victim = ChatWith;
            animator.SetState(0.0f, Vector3.Dot(cameraTransform.forward, transform.right) * 2.0f, 0.0f, victim);
        }

        private void Update()
        {
            if (ChatWith != null)
            {
                Chat();
                return;
            }
            var animator = monster.AnimationManager;
            lookatTarget.target = cameraTransform.position + cameraTransform.forward * 1000.0f;
            animator.SetState(
                Input.GetAxis("Vertical"),
                Vector3.Dot(cameraTransform.forward, transform.right) * 2.0f,
                Input.GetAxis("Horizontal"),
                lookatTarget
            );
            if (highlighted != null && Input.GetKeyDown(KeyCode.F))
            {
                Pickup();
            }
            else if (Input.GetButtonUp("Fire1"))
            {
                //monster.weapon.Swish();
                if (monster.weapon != null)
                {
                    monster.AnimationManager.Attack();
                }
            }
            else if (Time.timeSinceLevelLoad > checkHighlight)
            {
                LookForHoldableItems();
            }
            if (monster.shield != null)
            {
                SetBlocking(Input.GetButton("Fire2"));
            }
        }
    }
}