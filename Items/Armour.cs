using UnityEngine;

namespace Depravity
{
    public class Armour : MonoBehaviour
    {
        public ArmourProfile profile;

        private float Apply(float protection, float damage, RandomAudioClipsProfile clips, out AudioClip ding)
        {
            if (protection != 0.0f)
            {
                damage -= protection;
                if (damage <= 0.0f)
                {
                    ding = clips.Get();
                    return 0.0f;
                }
            }
            ding = null;
            return damage;
        }

        public float Absorb(Weapon.DamageType damageType, float damage, out AudioClip ding)
        {
            switch (damageType)
            {
                case Weapon.DamageType.SLASH:
                    return Apply(profile.slash, damage, profile.slashClips, out ding);
                case Weapon.DamageType.STAB:
                    return Apply(profile.stab, damage, profile.stabClips, out ding);
                case Weapon.DamageType.BASH:
                    return Apply(profile.bash, damage, profile.bashClips, out ding);
                case Weapon.DamageType.BURN:
                    return Apply(profile.burn, damage, profile.burnClips, out ding);
                case Weapon.DamageType.PSYCH:
                    return Apply(profile.psych, damage, profile.psychClips, out ding);
            }
            Debug.Assert(false, "Unknown damage type: " + damageType);
            ding = null;
            return 0.0f;
        }
    }
}
