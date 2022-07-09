using UnityEngine;

namespace Depravity{
    [CreateAssetMenu(fileName = "MaterialOptionsProfile", menuName = "Depravity/Material Options Profile")]
    public class MaterialOptionsProfile : ScriptableObject{
        [SerializeField]
        private Material[] options;

        public Material GetRandomMaterial()
        {
            return options[Random.Range(0, options.Length)];
        }
    }
}