using System.Collections;
using UnityEngine;

namespace Depravity
{
    [RequireComponent(typeof(AudioSource))]
    public class HandToHandWeapon : Weapon
    {
        private const float MINIMUM_SQR_VELOCITY_TO_CAUSE_DAMAGE = 100.0f;

        public float damage = 1.0f;
        public DamageType damageType = DamageType.BASH;
        public AnimationType animationType = AnimationType.MACE;

        [SerializeField]
        private RandomAudioClipsProfile swishSounds;

        private AudioSource audioSource;
        private Monster lastStruck;
        private Vector3 entryPoint;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            Held = false;
        }

        private IEnumerator GetVelocity()
        {
            const float DAMAGE_SCALE = 1.0f / MINIMUM_SQR_VELOCITY_TO_CAUSE_DAMAGE;
            yield return null;
            //yes this does happen
            if (lastStruck == null || Wielder == null)
            {
                yield break;
            }
            var direction = transform.position - entryPoint;
            var damage = (direction / Time.deltaTime).sqrMagnitude;
            if (damage >= MINIMUM_SQR_VELOCITY_TO_CAUSE_DAMAGE)
            {
                if (!lastStruck.TakeDamage(damageType, entryPoint, direction.normalized, damage * this.damage * DAMAGE_SCALE)) {
                    Wielder.AnimationManager.Blocked();
                }
            }
            else
            {
                Wielder.AnimationManager.Blocked();
            }
            lastStruck = null;
        }

        private void ApplyHit(Collider collider)
        {
            var monster = collider.gameObject.GetComponentInParent<Monster>();
            if (monster == null || monster == Wielder)
            {
                return;
            }
            monster.NotifyUnderAttack(Wielder);
            entryPoint = transform.position;
            lastStruck = monster;
            StartCoroutine(GetVelocity());
        }

        public void OnTriggerEnter(Collider collider)
        {
            if (Held && collider.gameObject.layer == Controller.DamageLayer)
            {
                ApplyHit(collider);
            }
        }

        public void Swish()
        {
            if (!audioSource.isPlaying)
            {
                audioSource.clip = swishSounds.Get();
                audioSource.Play();
            }
        }

        public override AnimationType GetAnimationType()
        {
            return animationType;
        }

        public override Hand GetHand()
        {
            return Hand.RIGHT;
        }
    }
}