using UnityEngine;

namespace Depravity
{
    [CreateAssetMenu(fileName = "DamageProfile", menuName = "Depravity/Damage Profile")]
    public class DamageProfile : ScriptableObject
    {
        [Header("Damage Multipliers")]
        public float slash = 1.0f;
        public float stab = 1.0f, bash = 1.0f, burn = 1.0f, psych = 1.0f;

        [Header("Sound Effects")]
        public AudioClip[] slashClips;
        public AudioClip[] stabClips, bashClips, burnClips, psychClips;

        private int slashOffset, stabOffset, bashOffset, burnOffset, psychOffset;

        private float AdjustDamageAndGetSoundEffect(AudioClip[] sources, float damageMultiplier, float damage, ref int offset, out AudioClip clip)
        {
            clip = sources[offset++];
            offset %= sources.Length;
            return damage * damageMultiplier;
        }

        public float AdjustDamageAndGetSoundEffect(Weapon.DamageType type, float damage, out AudioClip clip)
        {
            switch (type)
            {
                case Weapon.DamageType.SLASH:
                    return AdjustDamageAndGetSoundEffect(slashClips, slash, damage, ref slashOffset, out clip);
                case Weapon.DamageType.STAB:
                    return AdjustDamageAndGetSoundEffect(stabClips, stab, damage, ref stabOffset, out clip);
                case Weapon.DamageType.BASH:
                    return AdjustDamageAndGetSoundEffect(bashClips, bash, damage, ref bashOffset, out clip);
                case Weapon.DamageType.BURN:
                    return AdjustDamageAndGetSoundEffect(burnClips, burn, damage, ref burnOffset, out clip);
                case Weapon.DamageType.PSYCH:
                    return AdjustDamageAndGetSoundEffect(burnClips, psych, damage, ref psychOffset, out clip);
            }
            Debug.Assert(false, "Missing profile");
            clip = null;
            return 0.0f;
        }
    }
}
