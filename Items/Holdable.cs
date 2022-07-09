using UnityEngine;

namespace Depravity
{
    [RequireComponent(
        typeof(Rigidbody)
    )]
    public abstract class Holdable : MonoBehaviour
    {
        public enum Hand
        {
            LEFT, RIGHT
        }

        [SerializeField]
        protected GameObject dropCollider;

        [SerializeField, Header("Handle")]
        private Vector3 offsetPosition;

        [SerializeField]
        private Vector3 offsetRotation;

        [SerializeField]
        protected bool twoHanded = false;

        private Material normalMaterial;

        public Monster Wielder { get; private set; }

        private void Highlight()
        {
            var renderer = GetComponent<MeshRenderer>();
            normalMaterial = renderer.sharedMaterial;
            renderer.sharedMaterial = Controller.InteractableMaterial;
            renderer.sharedMaterial.mainTexture = normalMaterial.mainTexture;
        }

        public abstract Hand GetHand();

        public bool Highlighted
        {
            set
            {
                if (value)
                {
                    if (normalMaterial == null)
                    {
                        Highlight();
                    }
                }
                else if (normalMaterial != null)
                {
                    GetComponent<MeshRenderer>().sharedMaterial = normalMaterial;
                    normalMaterial = null;
                }
            }
            get
            {
                return normalMaterial != null;
            }
        }

        public bool IsTwoHanded
        {
            get
            {
                return twoHanded;
            }
        }


        private void CoupleWith(Holdable h, bool couple)
        {
            if (h == null)
            {
                return;
            }
            Physics.IgnoreCollision(GetComponent<Collider>(), h.GetComponent<Collider>(), couple);
        }

        public bool Held
        {
            set
            {
                var rigidBody = GetComponent<Rigidbody>();
                rigidBody.isKinematic = value;
                rigidBody.useGravity = !value;
                if (dropCollider != null)
                {
                    dropCollider.SetActive(!value);
                }
                gameObject.layer = value ? GetPhysicsLayer() : 0;
                OnHeldStateChanged(value);
                /*if (value)
                {
                    gameObject.layer = LAYER;
                }
                else
                {
                    gameObject.layer = 0;
                }*/
            }
            get
            {
                return Wielder != null;
            }
        }

        private void Drop(Monster monster)
        {
            Debug.Log("Dropping " + name);
            if (monster != null)
            {
                monster.ClearHoldable(this);
                monster.OnDied -= Drop;
                var other = monster.InLeftHand;
                if (other == this)
                {
                    other = monster.InRightHand;
                }
                CoupleWith(other, false);
            }
            var dungeon = Dungeon.Instance;
            if (dungeon.FindBlockAt(dungeon.GetOffset(transform.position), out var block))
            {
                transform.parent = block.transform;
                Held = false;
                Wielder = null;
            }
        }

        public void Drop()
        {
            Drop(Wielder);
        }

        protected virtual void OnHeldStateChanged(bool held) { }

        public abstract void Bind();

        public abstract int GetPhysicsLayer();

        private void AddToHand(Monster monster)
        {
            transform.localPosition = offsetPosition;
            transform.localRotation = Quaternion.Euler(offsetRotation);
            monster.OnDied += Drop;
            Wielder = monster;
            Held = true;
        }

        public void AddToLeftHandOf(Monster monster)
        {
            monster.InLeftHand = this;
            CoupleWith(monster.InRightHand, true);
            AddToHand(monster);
        }

        public void AddToRightHandOf(Monster monster)
        {
            monster.InRightHand = this;
            CoupleWith(monster.InLeftHand, true);
            AddToHand(monster);
        }

        private void OnDestroy()
        {
            Debug.Log("Destroying " + name);
        }
    }
}
