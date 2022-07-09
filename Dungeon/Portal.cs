using UnityEngine;

namespace Depravity {

    public class Portal : MonoBehaviour
    {
        [SerializeField]
        private string sceneName;

        [SerializeField]
        private bool placePlayerAtExit = false;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == Controller.Player.gameObject)
            {
                Controller.StartPlayerAtExit = placePlayerAtExit;
                Controller.SceneName = sceneName;
            }
        }
    }
}