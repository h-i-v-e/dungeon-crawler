using UnityEngine;

namespace Depravity
{
    public class SkyDome : MonoBehaviour
    {
        private void Update()
        {
            transform.position = Controller.Player.transform.position;
        }
    }
}
