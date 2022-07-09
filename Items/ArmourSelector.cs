using System.Collections.Generic;

using UnityEngine;
using TMPro;

namespace Depravity
{
    public class ArmourSelector : MonoBehaviour
    {
        private TMP_Dropdown dropdown;
        private CharacterSetup setup;

        private void Awake()
        {
            dropdown = GetComponent<TMP_Dropdown>();
        }

        private void ChangeOutfit(int val)
        {
            setup.Selected = val;
        }

        private void Start()
        {
            setup = Controller.Player.GetComponent<CharacterSetup>();
            var outfits = setup.outfits;
            int len = outfits.Length;
            var options = new List<string>(len);
            for (int i = 0; i != len; options.Add(outfits[i++].name)) ;
            dropdown.ClearOptions();
            dropdown.AddOptions(options);
            dropdown.onValueChanged.AddListener(ChangeOutfit);
        }
    }
}