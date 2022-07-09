using UnityEngine;

namespace Depravity
{

    public class DecorationProvider : MonoBehaviour
    {
        [SerializeField]
        private float probability = 0.1f;

        private void Add(Transform parent, Transform child)
        {
            var go = child.gameObject;
            var add = Instantiate(go, parent);
            var ltran = add.transform;
            ltran.localPosition = child.localPosition;
            ltran.localRotation = child.localRotation;
            add.SetActive(true);
        }

        public void Decorate(Transform parent)
        {
            for (int i = 0, j = transform.childCount; i < j; ++i)
            {
                if (Random.value < probability)
                {
                    Add(parent, transform.GetChild(i));
                }
            }
        }
    }
}
