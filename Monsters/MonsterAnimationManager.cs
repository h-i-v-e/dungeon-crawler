using UnityEngine;

namespace Depravity
{
    public interface LookAtTarget
    {
        Vector3 GetPosition();
    }

    [RequireComponent(typeof(Animator), typeof(Rigidbody))]
    public class MonsterAnimationManager : MonoBehaviour
    {
        private const int NUM_ATTACK_IDS = 7, NUM_BLOCK_IDS = 2;

        private static readonly int forwardId = Animator.StringToHash("Forward"),
            turnId = Animator.StringToHash("Turn"),
            strafeId = Animator.StringToHash("Strafe"),
            layDownId = Animator.StringToHash("LayDown"),
            downId = Animator.StringToHash("Down"),
            pickupLeftId = Animator.StringToHash("PickUpLeft"),
            pickupRightId = Animator.StringToHash("PickUpRight"),
            armedWithId = Animator.StringToHash("ArmedWith"),
            blockedId = Animator.StringToHash("Blocked"),
            hitFront1Id = Animator.StringToHash("HitFront1"),
            hitFront2Id = Animator.StringToHash("HitFront2"),
            hitBackId = Animator.StringToHash("HitBack"),
            hitLeftId = Animator.StringToHash("HitLeft"),
            hitRightId = Animator.StringToHash("HitRight"),
            carryingShieldId = Animator.StringToHash("CarryingShield"),
            blockingId = Animator.StringToHash("Blocking"),
            activityId = Animator.StringToHash("Activity"),
            attack = Animator.StringToHash("Attack"),
            attackType = Animator.StringToHash("AttackType");

        private static readonly int[] blockIds = new int[NUM_ATTACK_IDS],
            numAttacks = new int[Weapon.NUMBER_OF_ANIMATION_TYPES + 1];

        private static void LoadStaticIds(string prefix, int[] ids, int num)
        {
            for (int i = 0; i != num; ++i)
            {
                ids[i] = Animator.StringToHash(prefix + (i + 1));
            }
        }

        static MonsterAnimationManager()
        {
            LoadStaticIds("Block", blockIds, NUM_BLOCK_IDS);
            numAttacks[(int)Weapon.AnimationType.SWORD] = 7;
            numAttacks[(int)Weapon.AnimationType.SWORD_2H] = 11;
            numAttacks[(int)Weapon.AnimationType.BOW_2H] = 4;
            numAttacks[(int)Weapon.AnimationType.MACE] = 3;
            numAttacks[(int)Weapon.AnimationType.AXE_2H] = 6;
            numAttacks[(int)Weapon.AnimationType.SPEAR] = 4;
        }

        private enum LookAtState
        {
            NOTHING, NEW, SHIFTING, INSTANT
        };

        public enum Activity
        {
            IDLE = 0,
            ATTACKING = 1,
            SHOPPING = 2
        }

        public Vector3 lastLookAt;
        private LookAtTarget lookAt;
        private LookAtState lookAtState = LookAtState.NOTHING;
        public Activity currentState = Activity.IDLE;
        private float lookStateChanged = 0.0f;
        private int numAttackTypes = 0, woundedLayerId, shieldLayerId, turnLayerId;
        private bool carryingShield;

        public float weight;
        public string state = "";

        public Animator Animator { get; private set; }

#if UNITY_EDITOR
        private float forward = 0.0f, turn = 0.0f;
#endif

        public void SetActivity(Activity state)
        {
            Animator.SetInteger(activityId, (int)state);
            currentState = state;
            UpdateCarryingShield();
        }

        private void SetLookingAt(LookAtState state, LookAtTarget target)
        {
            lookAtState = state;
            lookStateChanged = Time.timeSinceLevelLoad;
            lookAt = target;
        }

        public LookAtTarget LookingAt
        {
            set
            {
                if (value == null)
                {
                    if (lookAt != null)
                    {
                        lastLookAt = lookAt.GetPosition();
                        SetLookingAt(LookAtState.NOTHING, null);
                    }
                }
                else if (lookAtState == LookAtState.NOTHING)
                {
                    SetLookingAt(LookAtState.NEW, value);
                }
                else if (value != lookAt)
                {
                    lastLookAt = Vector3.Lerp(lastLookAt, lookAt.GetPosition(), Mathf.Clamp01(Time.timeSinceLevelLoad - lookStateChanged));
                    SetLookingAt(LookAtState.SHIFTING, value);
                }
            }
            get
            {
                return lookAt;
            }
        }

        private void UpdateState(float forward, float turn, float strafe)
        {
#if UNITY_EDITOR
            this.forward = forward;
            this.turn = turn;
#endif
            Animator.SetFloat(forwardId, forward);
            Animator.SetFloat(turnId, turn);
            Animator.SetFloat(strafeId, strafe);
            if (turn != 0.0f)
            {
                if (strafe != 0.0f)
                {
                    float absTurn = Mathf.Abs(turn), absStrafe = Mathf.Max(Mathf.Abs(strafe), 1.0f);
                    float total = Mathf.Max(absTurn, absStrafe);
                    Animator.SetLayerWeight(turnLayerId, Mathf.Max(absTurn - absStrafe, 0.0f) / total);
                }
                else
                {
                    Animator.SetLayerWeight(turnLayerId, 1.0f);
                }
            }
            else
            {
                Animator.SetLayerWeight(turnLayerId, 0.0f);
            }
        }

        public void SetState(float forward, float turn, float strafe, LookAtTarget lookAt = null)
        {
            if (lookAt != this.lookAt)
            {
                LookingAt = lookAt;
            }
            this.lookAt = lookAt;
            UpdateState(forward, turn, strafe);
        }

        private void UpdateCarryingShield()
        {
            bool apply = carryingShield && currentState == Activity.ATTACKING;
            Animator.SetBool(carryingShieldId, apply);
            Animator.SetLayerWeight(shieldLayerId, apply ? 1.0f : 0.0f);
        }

        public bool Blocking
        {
            set
            {
                Animator.SetBool(blockingId, value);
            }
        }

        public void Block()
        {
            Animator.SetTrigger(blockIds[Random.Range(0, NUM_BLOCK_IDS)]);
        }

        public void LayDown()
        {
            Animator.SetTrigger(layDownId);
        }

        public void Down() 
        {
            Animator.SetTrigger(downId);
        }

        public void Blocked()
        {
            Animator.SetTrigger(blockedId);
        }

        public void HitFront()
        {
            Animator.SetTrigger(Random.value < 0.5f ? hitFront1Id : hitFront2Id);
        }

        public void HitBack()
        {
            Animator.SetTrigger(hitBackId);
        }

        public void HitLeft()
        {
            Animator.SetTrigger(hitLeftId);
        }

        public void HitRight()
        {
            Animator.SetTrigger(hitRightId);
        }

        public void Attack()
        {
            if (numAttackTypes != 0)
            {
                Animator.SetInteger(attackType, Random.Range(1, numAttackTypes + 1));
                Animator.SetLayerWeight(turnLayerId, 0.0f);
                Animator.SetTrigger(attack);
            }
        }

        public void PickUpLeft()
        {
            Animator.SetTrigger(pickupLeftId);
        }

        public void PickUpRight()
        {
            Animator.SetTrigger(pickupRightId);
        }

        public void SetArmedWith(Weapon.AnimationType animationType)
        {
            int idx = (int)animationType;
            Animator.SetInteger(armedWithId, idx);
            numAttackTypes = numAttacks[idx];
        }

        public bool CarryingShield
        {
            set
            {
                carryingShield = value;
                UpdateCarryingShield();
            }
            get
            {
                return carryingShield;
            }
        }

        public float Wounded
        {
            set
            {
                Animator.SetLayerWeight(woundedLayerId, value);
            }
        }

        private void Awake()
        {
            Animator = GetComponent<Animator>();
            woundedLayerId = Animator.GetLayerIndex("Wounded");
            shieldLayerId = Animator.GetLayerIndex("Shield");
            turnLayerId = Animator.GetLayerIndex("Turn");
        }

        private void OnAnimatorIK(int layerIndex)
        {
            weight = Mathf.Clamp01(Time.timeSinceLevelLoad - lookStateChanged);
            state = lookAtState.ToString();
            switch (lookAtState)
            {
                /*case LookAtState.INSTANT:
                    Animator.SetLookAtWeight(1.0f);
                    Animator.SetLookAtPosition(lookAtPos);
                    return;*/
                case LookAtState.NOTHING:
                    Animator.SetLookAtWeight(1.0f - weight);
                    return;
                case LookAtState.NEW:
                    Animator.SetLookAtWeight(weight);
                    Animator.SetLookAtPosition(lookAt.GetPosition());
                    break;
                default:
                    Animator.SetLookAtWeight(1.0f);
                    Animator.SetLookAtPosition(Vector3.Lerp(lastLookAt, lookAt.GetPosition(), weight));
                    break;
            }
            /*if (weight == 1.0f)
            {
                watingForTimestamp = true;
                lastLookAt = lookAtPos;
                lookAtState = LookAtState.SHIFTING;
            }*/
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            var pos = transform.position;
            pos.y = 0.5f;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(pos, pos + transform.forward * forward * 2.0f);
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(pos, pos + transform.right * turn * 2.0f);
        }
#endif
    }
}