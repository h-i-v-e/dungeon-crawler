using UnityEngine;

public class RandomTransform : MonoBehaviour
{
    [System.Serializable]
    struct RandomTransformOption
    {
        public float rotation;
        public Vector2 offset;
    }

    [SerializeField]
    private RandomTransformOption[] options;

    private void ApplyOption(RandomTransformOption option)
    {
        transform.localEulerAngles = new Vector3(0.0f, option.rotation, 0.0f);
        transform.localPosition = new Vector3(option.offset.x, 0.0f, option.offset.y);
    }

    private void Start()
    {
        ApplyOption(options[Random.Range(0, options.Length)]);
    }
}