using UnityEngine;

namespace Depravity{

    public class MaterialRandomizer : MonoBehaviour{
        [SerializeField]
        private MaterialOptionsProfile options;

        private void Start()
        {
            if (options == null)
            {
                return;
            }
            GetComponent<SkinnedMeshRenderer>().material = options.GetRandomMaterial();
        }
    }
}