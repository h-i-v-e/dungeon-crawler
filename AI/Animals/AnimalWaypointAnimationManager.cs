using UnityEngine;
using System.Collections;

namespace Depravity
{
    [RequireComponent(typeof(Animator), typeof(CharacterController), typeof(AudioSource))]
    public sealed class AnimalWaypointAnimationManager : MonoBehaviour, IWaypointAnimationManager
    {
        private static int walking, running, idling, sleeping, sitting;
        private static bool statesSet = false;

        [SerializeField]
        [Range(0.1f, 25.0f)]
        private float runSpeed = 2.0f, walkSpeed = 1.0f, acceleration = 1.0f;

        [SerializeField]
        [Range(36.0f, 720.0f)]
        private float turnSpeed = 360.0f;

        [SerializeField]
        private AudioClip animalSound, walkingSound, eatingSound,
            runningSound, attackingSound, deathSound, sleepingSound;

        private CharacterController characterController;
        private AudioSource audioSource;
        private Animator animator;
        private Vector3 target;
        private float currentSpeed, maxSpeed;
        private int state;

        private static void InitStates()
        {
            walking = Animator.StringToHash("isWalking");
            running = Animator.StringToHash("isRunning");
            idling = Animator.StringToHash("isIdling");
            sleeping = Animator.StringToHash("isSleeping");
            sitting = Animator.StringToHash("isSitting");
            statesSet = true;
        }

        private void SetState(int state)
        {
            if (animator == null)
            {
                return;
            }
            animator.SetBool(this.state, false);
            animator.SetBool(state, true);
            this.state = state;
        }

        private float ComputeStoppingDistance()
        {
            if (currentSpeed < 0.01f)
            {
                return 0.0f;
            }
            float stoppingTime = currentSpeed / acceleration;
            return stoppingTime * currentSpeed * 0.5f;
        }

        private Vector3 AdjustFacing(Vector3 target)
        {
            var forward = transform.forward;
            var angle = Vector3.Angle(forward, target);
            return transform.forward = Vector3.Slerp(forward, target, Mathf.Clamp01(turnSpeed / angle) * Time.deltaTime);
        }

        private void AdjustSpeed(float target)
        {
            if (currentSpeed == target)
            {
                return;
            }
            float shift = acceleration * Time.deltaTime;
            if (target < currentSpeed)
            {
                currentSpeed -= shift;
                if (currentSpeed < target)
                {
                    currentSpeed = target;
                }
            }
            else
            {
                currentSpeed += shift;
                if (currentSpeed > target)
                {
                    currentSpeed = target;
                }
            }
            if (currentSpeed > walkSpeed)
            {
                SetState(running);
            }
            else
            {
                SetState(walking);
            }
        }

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();
            if (!statesSet)
            {
                InitStates();
            }
            state = idling;
        }

        private void OnEnable()
        {
            currentSpeed = 0.0f;
        }

        private IEnumerator Stop(WaypointAgent.ReachedTarget reachedTarget)
        {
            for (; ; )
            {
                AdjustSpeed(0.0f);
                if (currentSpeed == 0.0f)
                {
                    reachedTarget();
                    yield break;
                }
                var pos = transform.position;
                var direction = target - pos;
                characterController.Move(AdjustFacing(direction.normalized) * currentSpeed * Time.deltaTime);
                yield return null;
            }
        }

        private IEnumerator MovingToTarget(WaypointAgent.ReachedTarget reachedTarget)
        {
            for (; ; )
            {
                var pos = transform.position;
                var direction = target - pos;
                float stop = ComputeStoppingDistance();
                if (direction.sqrMagnitude < stop)
                {
                    reachedTarget();
                    yield break;
                }
                AdjustSpeed(maxSpeed);
                characterController.Move(AdjustFacing(direction.normalized) * currentSpeed * Time.deltaTime);
                yield return null;
            }
        }

        private void StartIdling()
        {
            SetState(idling);
        }

        private void StartSleeping()
        {
            SetState(sleeping);
        }

        private void StartSitting()
        {
            SetState(sitting);
        }

        public void MoveTo(Vector3 position, bool run, WaypointAgent.ReachedTarget reachedTarget)
        {
            maxSpeed = run ? runSpeed : walkSpeed;
            target = position;
            StartCoroutine(MovingToTarget(reachedTarget));
        }

        public void Idle()
        {
            StartCoroutine(Stop(StartIdling));

        }

        public void Sleep()
        {
            StartCoroutine(Stop(StartSleeping));
        }

        public void Sit()
        {
            StartCoroutine(Stop(StartSitting));
        }

        private void Play(AudioClip clip)
        {
            if (clip != null)
            {
                audioSource.clip = clip;
                audioSource.Play();
            }
        }

        public void AnimalSound()
        {
            Play(animalSound);
        }

        public void Walking()
        {
            Play(walkingSound);
        }

        public void Eating()
        {
            Play(eatingSound);
        }

        public void Running()
        {
            Play(runningSound);
        }

        public void Attacking()
        {
            Play(attackingSound);
        }

        public void Death()
        {
            Play(deathSound);
        }

        public void Sleeping()
        {
            Play(sleepingSound);
        }
    }
}