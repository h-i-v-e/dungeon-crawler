using UnityEngine;

namespace Depravity
{
    [CreateAssetMenu(fileName = "CyclicAudioClipsProfile", menuName = "Depravity/Cyclic Audio Clips Profile")]
    public class CyclicAudioClipsProfile : ScriptableObject
    {
        [SerializeField]
        private AudioClip[] audioClips;

        private int offset = 0;

        public AudioClip Get()
        {
            offset %= audioClips.Length;
            return audioClips[offset++];
        }
    }
}