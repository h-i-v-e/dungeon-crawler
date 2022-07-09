using UnityEngine;
using System.Collections;

namespace Depravity
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class UIFader : MonoBehaviour
    {
        [SerializeField]
        private float fadeTime = 1.0f;

        private CanvasGroup canvasGroup;

        public delegate void OpacityChangeComplete();
        public delegate void OpacityChanged(float value);

        public event OpacityChanged OnOpacityChanged;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        private IEnumerator ChangeOpacity(float from, float to, OpacityChangeComplete ready)
        {
            float start = Time.unscaledTime, progress;
            do
            {
                yield return null;
                progress = Mathf.Clamp01((Time.unscaledTime - start) / fadeTime);
                float val = Mathf.Lerp(from, to, progress);
                canvasGroup.alpha = val;
                OnOpacityChanged?.Invoke(val);
            }
            while (progress != 1.0f);
            if (to == 0.0f)
            {
                gameObject.SetActive(false);
            }
            ready?.Invoke();
        }

        public void Hide(OpacityChangeComplete ready = null)
        {
            if (gameObject.activeSelf)
            {
                StartCoroutine(ChangeOpacity(1.0f, 0.0f, ready));
            }
            else
            {
                ready?.Invoke();
            }
        }

        public void Show(OpacityChangeComplete ready = null)
        {
            if (gameObject.activeSelf)
            {
                ready?.Invoke();
            }
            else
            {
                gameObject.SetActive(true);
                StartCoroutine(ChangeOpacity(0.0f, 1.0f, ready));
            }
        }
    }
}