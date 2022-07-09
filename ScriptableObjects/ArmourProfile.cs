using UnityEngine;

namespace Depravity
{
    [CreateAssetMenu(fileName = "ArmourProfile", menuName = "Depravity/Armour Profile")]
    public class ArmourProfile : ScriptableObject
    {
        [Header("Protection Against")]
        public float slash;
        public float stab, bash, burn, psych;

        [Header("Sound Effects")]
        public RandomAudioClipsProfile slashClips;
        public RandomAudioClipsProfile stabClips, bashClips, burnClips, psychClips;
    }
}
