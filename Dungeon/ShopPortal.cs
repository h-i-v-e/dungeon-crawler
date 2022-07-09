using UnityEngine;

namespace Depravity
{
    [RequireComponent(typeof(Collider))]
    public class ShopPortal : MonoBehaviour
    {
        [SerializeField]
        private string sceneName;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == Controller.Player.gameObject)
            {
                //Controller.StartPlayerAtExit = false;
                //Controller.EnterShop(sceneName);
            }
        }
    }
}