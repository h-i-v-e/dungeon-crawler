using UnityEngine;

namespace Depravity
{
    public class MouseZoom : MonoBehaviour
    {
        [SerializeField]
        private float zoomSpeed;

        [SerializeField]
        private Transform target;

        private float distance;

        private void Start()
        {
            var dir = target.position - transform.position;
            distance = dir.magnitude;
            transform.rotation = Quaternion.LookRotation(dir / distance);
        }

        private void Update()
        {
            transform.localPosition += transform.forward * distance * Input.mouseScrollDelta.y * zoomSpeed;
        }
    }
}