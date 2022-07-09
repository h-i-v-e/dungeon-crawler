using UnityEngine;

namespace Depravity
{
    [RequireComponent(typeof(Monster))]
    public class Vocaliser : MonoBehaviour
    {
        [SerializeField]
        private AudioSource audioSource;

        [SerializeField]
        private VocalisationProfile vocalisationProfile;

        private void PlayClip(AudioClip clip)
        {
            if (clip == null || audioSource.isPlaying)
            {
                return;
            }
            audioSource.clip = clip;
            audioSource.Play();
        }

        private void TookDamage(Monster monster, float amount)
        {
            PlayClip(vocalisationProfile.TookDamage());
        }

        private void Died(Monster monster)
        {
            PlayClip(vocalisationProfile.Died());
        }

        private void Revived(Monster monster)
        {
            audioSource.Stop();
        }

        private void Start()
        {
            var monster = GetComponent<Monster>();
            monster.OnTookDamage += TookDamage;
            monster.OnDied += Died;
            monster.OnRevived += Revived;
        }

        public void Chase()
        {
            PlayClip(vocalisationProfile.Chase());
        }

        public void Flee()
        {
            PlayClip(vocalisationProfile.Flee());
        }

        public void SeeEnemy()
        {
            PlayClip(vocalisationProfile.SeeEnemy());
        }
    }
}
