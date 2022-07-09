using System;
using UnityEngine;

namespace Depravity
{
    [CreateAssetMenu(fileName = "WeaponOptionsProfile", menuName = "Depravity/Weapon Options Profile")]
    public class WeaponOptionsProfile : ScriptableObject
    {
        [Serializable]
        public struct Option : RandomActivationOption
        {
            public Weapon weapon;
            public Shield shield;
            public int chances;

            public int GetChances()
            {
                return chances;
            }
        }

        public Option[] options;
    }
}
