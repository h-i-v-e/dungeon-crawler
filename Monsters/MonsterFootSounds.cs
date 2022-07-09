using UnityEngine;

namespace Depravity
{

    public class MonsterFootSounds : MonoBehaviour
    {
        [SerializeField]
        private AudioSource feetAudioSource;

        [SerializeField]
        private CyclicAudioClipsProfile audioClipsProfile;

        private void PlayFootstep()
        {
            if (!feetAudioSource.isPlaying)
            {
                feetAudioSource.clip = audioClipsProfile.Get();
                feetAudioSource.Play();
            }
        }

        //These are called from the animation
        public void FootR()
        {
            PlayFootstep();
        }

        public void FootL()
        {
            PlayFootstep();
        }
    }
}