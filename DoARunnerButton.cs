using UnityEngine;
using UnityEngine.UI;

namespace Depravity
{
    [RequireComponent(typeof(Button))]
    public class DoARunnerButton : MonoBehaviour
    {
        [SerializeField]
        private string exitToLevel;

        private void Exit()
        {
            //Controller.ExitShop(exitToLevel);
        }

        private void Start()
        {
            GetComponent<Button>().onClick.AddListener(Exit);
        }
    }
}
