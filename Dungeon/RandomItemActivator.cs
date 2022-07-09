using UnityEngine;

namespace Depravity
{

    public class RandomItemActivator : MonoBehaviour
    {
        public float activationProbability;

        private void Start()
        {
            for (int i = 0, j = transform.childCount; i != j; ++i)
            {
                transform.GetChild(i).gameObject.SetActive(Random.value <= activationProbability);
            }
        }
    }
}