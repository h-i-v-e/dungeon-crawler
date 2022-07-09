using UnityEngine;
using UnityEngine.UI;

namespace Depravity {

    public class HealthBar : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Image image;

        private Vector2 anchorTop;
        private float lastHealth = float.MaxValue;

        [SerializeField]
        private Color healthy, dead;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            image = GetComponent<Image>();
            anchorTop = rectTransform.anchorMax;
        }

        private void Update()
        {
            var player = Controller.Player;
            float health = player.currentHitPoints;
            if (health == lastHealth)
            {
                return;
            }
            lastHealth = health;
            health /= player.fullHitPoints;
            image.color = Color.Lerp(dead, healthy, health);
            float level = Mathf.Lerp(rectTransform.anchorMin.y, anchorTop.y, health);
            rectTransform.anchorMax = new Vector2(anchorTop.x, level);
        }
    }
}
