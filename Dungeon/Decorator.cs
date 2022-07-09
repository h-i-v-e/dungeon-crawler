using UnityEngine;

namespace Depravity
{
    public class Decorator : MonoBehaviour
    {
        [SerializeField]
        private DecorationProvider provider;

        private void Start()
        {
            provider.Decorate(transform);
        }
    }
}
