using UnityEngine;

namespace Depravity
{

    public class PinRotation : MonoBehaviour
    {
        private Quaternion originalRotation;

        private void Start()
        {
            originalRotation = transform.rotation;
        }

        private void LateUpdate()
        {
            transform.rotation = originalRotation;
        }
    }
}