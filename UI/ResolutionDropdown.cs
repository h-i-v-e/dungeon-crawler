using UnityEngine;
using TMPro;
using System.Collections.Generic;

namespace Depravity
{
    [RequireComponent(typeof(TMP_Dropdown))]
    public class ResolutionDropdown : MonoBehaviour
    {
        private void OptionChosen(int choice)
        {
            var opt = Screen.resolutions[choice];
            Screen.SetResolution(opt.width, opt.height, true);
        }

        private void Start()
        {
            var res = Screen.resolutions;
            int len = res.Length;
            var options = GetComponent<TMP_Dropdown>();
            List<string> labels = new List<string>(len);
            int chosen = 0, width = Screen.width, height = Screen.height;
            Debug.Log(width + " x " + height);
            for (int i = 0, j = res.Length; i != j; ++i)
            {
                var opt = res[i];
                int w = opt.width, h = opt.height;
                labels.Add(w + " x " + h);
                if (w == width && h == height)
                {
                    chosen = i;
                }
            }
            options.AddOptions(labels);
            options.value = chosen;
            options.onValueChanged.AddListener(OptionChosen);
        }
    }
}