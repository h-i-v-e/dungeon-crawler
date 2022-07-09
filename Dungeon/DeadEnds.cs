using UnityEngine;

namespace Depravity
{

    public class DeadEnds : MonoBehaviour
    {
        public static DeadEnds Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public GameObject AddTo(Transform parent)
        {
            var item = Instantiate(transform.GetChild(Random.Range(0, transform.childCount)).gameObject, parent);
            item.SetActive(true);
            return item;
        }
    }
}