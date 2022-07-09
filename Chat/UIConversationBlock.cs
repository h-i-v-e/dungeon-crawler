using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Depravity
{

    public class UIConversationBlock : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text text;

        [SerializeField]
        private RectTransform rectTransform;

        [SerializeField]
        private Image image;

        public float Height
        {
            get
            {
                return rectTransform.rect.height;
            }
        }

        public Color Colour
        {
            get
            {
                return image.color;
            }
            set
            {
                image.color = value;
            }
        }

        public string Text
        {
            get
            {
                return text.text;
            }
            set
            {
                text.text = value;
            }
        }

        private static Vector2 SetXOffsetZero(Vector2 val)
        {
            val.x = 0;
            return val;
        }

        public float Offset
        {
            set
            {
                rectTransform.offsetMin = SetXOffsetZero(rectTransform.offsetMin);
                rectTransform.offsetMax = SetXOffsetZero(rectTransform.offsetMax);
                var pos = rectTransform.anchoredPosition;
                pos.y = value;
                rectTransform.anchoredPosition = pos;
            }
        }
    }
}