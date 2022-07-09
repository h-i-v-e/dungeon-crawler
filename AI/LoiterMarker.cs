using UnityEngine;

namespace Depravity
{
    public class LoiterMarker : MonoBehaviour
    {
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            const float rad = 0.15f;
            var pos = transform.position;
            pos.y = rad;
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(pos, rad);
            var direction = transform.rotation * Vector3.forward;
            pos += direction * rad;
            Gizmos.DrawRay(new Ray(pos, direction));
        }
#endif
    }
}