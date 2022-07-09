using System.Collections;
using UnityEngine;

namespace Depravity
{
    [RequireComponent(typeof(Animation))]
    public class Bow : Weapon
    {
        [SerializeField]
        private new Animation animation;

        [SerializeField]
        private new SkinnedMeshRenderer renderer;

        [SerializeField]
        private int rootBoneIndex, stringBoneIndex = 0, top1 = 0, top2 = 0, top3 = 0, bottom1 = 0, bottom2 = 0, bottom3 = 0;

        private Vector3 vs, t1, t2, t3, b1, b2, b3;
        private float releaseTime, upperStringLength, lowerStringLength;

        public override int GetPhysicsLayer()
        {
            return Controller.WeaponLayer;
        }

#if UNITY_EDITOR
        private void DrawGizmo(Transform t)
        {
            Gizmos.DrawSphere(t.position, 0.03f);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            var bones = renderer.bones;
            if (stringBoneIndex > bones.Length)
            {
                stringBoneIndex = bones.Length - 1;
            }
            for (int i = 0; i < stringBoneIndex; DrawGizmo(bones[i++]));
            for (int i = stringBoneIndex + 1, j = bones.Length; i != j; DrawGizmo(bones[i++])) ;
            Gizmos.color = Color.green;
            DrawGizmo(bones[stringBoneIndex]);
        }
#endif

        private void Awake()
        {
            twoHanded = true;
            var bones = renderer.bones;
            /*for (int i = 0, j = bones.Length; i < j; ++i)
            {
                var bone = bones[i];
                if (bone.transform.parent == null)
                {
                    renderer.rootBone = bone;
                    return;
                }
            }*/
            //stringOriginalPosition = bones[stringBoneIndex].localPosition;
            var rootBone = bones[rootBoneIndex];
            var rootPos = rootBone.localPosition;
            renderer.rootBone = rootBone;
            vs = bones[stringBoneIndex].localPosition;
            t1 = bones[top1].localPosition;
            t2 = bones[top2].localPosition;
            t3 = bones[top3].localPosition;
            b1 = bones[bottom1].localPosition;
            b2 = bones[bottom2].localPosition;
            b3 = bones[bottom3].localPosition;
            upperStringLength = Vector3.Distance(rootPos, t3);
            lowerStringLength = Vector3.Distance(rootPos, b3);
        }

        private IEnumerator KeepBowAttached()
        {
            while (renderer.enabled)
            {
                renderer.rootBone.position = transform.position;
                renderer.rootBone.rotation = transform.rotation;
                renderer.bones[stringBoneIndex].position = Wielder.RightHand.position;

                yield return null;
            }
        }

        public override void Bind()
        {
            base.Bind();
            renderer.enabled = true;
            StartCoroutine(KeepBowAttached());
        }

        protected override void OnHeldStateChanged(bool held)
        {
            base.OnHeldStateChanged(held);
            if (!held)
            {
                renderer.enabled = false;
            }
        }

        public override void Shoot()
        {
            //releaseTime = Time.timeSinceLevelLoad;
            //releasePosition = Wielder.RightHand.position;

            Debug.Log("Shoot");
        }

        public override AnimationType GetAnimationType()
        {
            return AnimationType.BOW_2H;
        }

        public override Hand GetHand()
        {
            return Hand.LEFT;
        }
    }
}
