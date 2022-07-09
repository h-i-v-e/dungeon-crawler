using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using System.Collections;

namespace Depravity
{
    public sealed class DeathEffect : MonoBehaviour
    {
        [SerializeField]
        private float deathPause = 1.0f;

        [SerializeField]
        private GameObject playerCamera;

        private float dieAt;

        private IEnumerator Die()
        {
            do
            {
                yield return null;
            }
            while (Time.unscaledTime < dieAt);
            var vol = playerCamera.GetComponent<PostProcessVolume>();
            if (vol == null)
            {
                Debug.Assert(false, "PostProcessVolume on playerCamera");
                yield break;
            }
            if (vol.profile.TryGetSettings<DeathPPEffect>(out var effect))
            {
                effect.enabled.value = true;
                dieAt += effect.speed;
                do
                {
                    yield return null;
                }
                while (Time.unscaledTime < dieAt);
            }
            Controller.LoadHomeScreen();
        }

        private void StartEffect(Monster monster)
        {
            dieAt = Time.unscaledTime + deathPause;
            StartCoroutine(Die());
        }

        private void StopEffect(Monster monster)
        {
            var vol = playerCamera.GetComponent<PostProcessVolume>();
            if (vol != null)
            {
                vol.enabled = false;
            }
        }

        private void Start()
        {
            Controller.Player.OnDied += StartEffect;
            Controller.Player.OnRevived += StopEffect;
        }
    }
}