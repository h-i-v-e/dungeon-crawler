using UnityEngine;

namespace Depravity
{

    public class Flickering : MonoBehaviour
    {
        private Vector2 start;
        private Light target;
        private float baseComponent, variableComponent;

        [Range(0.0f, 10f)]
        public float speed = 2.0f;

        [Range(0.0f, 1.0f)]
        public float intensity = 0.5f;

        private void Start()
        {
            target = GetComponent<Light>();
            float intensity = target.intensity;
            baseComponent = intensity * this.intensity;
            variableComponent = intensity - baseComponent;
            start = new Vector2(Random.value * 1024f, Random.value * 1024f);
        }

        private void Update()
        {
            float noise = Mathf.PerlinNoise(start.x, start.y + Time.realtimeSinceStartup * speed);
            target.intensity = baseComponent + noise * variableComponent;
        }
    }
}