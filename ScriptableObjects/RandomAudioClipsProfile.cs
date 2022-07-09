using UnityEngine;

namespace Depravity
{
    [CreateAssetMenu(fileName = "RandomAudioClipsProfile", menuName = "Depravity/Random Audio Clips Profile")]
    public class RandomAudioClipsProfile : ScriptableObject
    {
        [SerializeField]
        private AudioClip[] audioClips;

        public AudioClip Get()
        {
            return audioClips[Random.Range(0, audioClips.Length)];
        }
    }
}
