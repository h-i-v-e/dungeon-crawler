using UnityEngine;

namespace Depravity
{
    public class ShoppingTriggers : MonoBehaviour
    {
        [SerializeField]
        private int leatherIdx = 1, chainIdx = 2, plateIdx = 3;

        private static int activeOutfit = -1;
        private static CharacterSetup setup;

        private void Start()
        {
            if (setup == null)
            {
                setup = Controller.Player.GetComponent<CharacterSetup>();
            }
        }

        public void WearLeather()
        {
            setup.Selected = leatherIdx;
        }

        public void BuyLeather()
        {
            activeOutfit = leatherIdx;
        }

        public void WearChain()
        {
            setup.Selected = chainIdx;
        }

        public void BuyChain()
        {
            activeOutfit = chainIdx;
        }

        public void WearPlate()
        {
            setup.Selected = plateIdx;
        }

        public void BuyPlate()
        {
            activeOutfit = plateIdx;
        }

        public void StopShopping()
        {
            setup.Selected = activeOutfit;
        }

        public void StartShopping()
        {
            activeOutfit = setup.Selected;
        }
    }
}
