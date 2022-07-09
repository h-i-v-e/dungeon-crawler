using System.Collections.Generic;

using UnityEngine;
using TMPro;

namespace Depravity
{
    [RequireComponent(typeof(TMP_Dropdown))]
    public class WeaponSelector : MonoBehaviour
    {
        public enum Hand
        {
            Left, Right
        }

        [SerializeField]
        private Hand hand;

        [SerializeField]
        private Holdable[] options;

        private void DropWeapon(Monster player)
        {
            var weapon = player.weapon;
            if (weapon != null)
            {
                weapon.Drop();
                player.weapon = null;
            }
        }

        private void DropShield(Monster player)
        {
            var shield = player.shield;
            if (shield != null)
            {
                shield.Drop();
                player.shield = null;
            }
        }

        private void Clear(Monster player)
        {
            if (hand == Hand.Right)
            {
                DropWeapon(player);
            }
            else
            {
                DropShield(player);
            }
        }

        private void Selected(int option)
        {
            var player = Controller.Player;
            if (option == 0)
            {
                Clear(player);
                return;
            }
            var replace = options[option - 1];
            if (hand == Hand.Right)
            {
                replace.AddToRightHandOf(player);
                player.weapon = (Weapon)replace;
            }
            else
            {
                replace.AddToLeftHandOf(player);
                player.shield = (Shield)replace;
            }
        }

        private void Start()
        {
            var dropdown = GetComponent<TMP_Dropdown>();
            int len = options.Length;
            var list = new List<string>(len + 1)
            {
                "None"
            };
            for (int i = 0; i != len; ++i)
            {
                list.Add(options[i].name);
            }
            dropdown.AddOptions(list);
            dropdown.onValueChanged.AddListener(Selected);
        }
    }
}
