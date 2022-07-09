using UnityEngine;

namespace Depravity
{

    public class RandomSingleActivator : MonoBehaviour
    {
        private void Start()
        {
            int len = transform.childCount;
            int active = Random.Range(0, len);
            for (int i = 0; i != len; ++i)
            {
                transform.GetChild(i).gameObject.SetActive(i == active);
            }
        }
    }
}
