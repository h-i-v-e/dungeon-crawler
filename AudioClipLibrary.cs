using UnityEngine;

namespace Depravity
{

    public class AudioClipLibrary : MonoBehaviour
    {
        private static AudioClipLibrary instance;

        public AudioClip[] cuttingFleshSounds;

        private void Awake()
        {
            instance = this;
        }

        private static AudioClip SelectRandomClip(AudioClip[] clips)
        {
            return clips[Random.Range(0, clips.Length)];
        }

        public static AudioClip GetCuttingFleshClip()
        {
            return SelectRandomClip(instance.cuttingFleshSounds);
        }
    }
}