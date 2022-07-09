using UnityEngine;

namespace Depravity
{
    [CreateAssetMenu(fileName = "VocalisationProfile", menuName = "Depravity/Vocalisation Profile")]
    public class VocalisationProfile : ScriptableObject
    {
        [SerializeField]
        private AudioClip[] tookDamage, died, chase, flee, seeEnemy;

        private static AudioClip GetRandom(AudioClip[] sources)
        {
            int len = sources.Length;
            if (len == 0)
            {
                return null;
            }
            return sources[Random.Range(0, len)];
        }

        public AudioClip TookDamage()
        {
            return GetRandom(tookDamage);
        }

        public AudioClip Died()
        {
            return GetRandom(died);
        }

        public AudioClip Chase()
        {
            return GetRandom(chase);
        }

        public AudioClip Flee()
        {
            return GetRandom(flee);
        }

        public AudioClip SeeEnemy()
        {
            return GetRandom(seeEnemy);
        }
    }
}