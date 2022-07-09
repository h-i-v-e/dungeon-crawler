using UnityEngine;

namespace Depravity
{
    public struct LookTarget
    {
        public Transform target;
        public Vector3 offset;

        public LookTarget(Transform target, Vector3 offset)
        {
            this.target = target;
            this.offset = offset;
        }
    }
}
