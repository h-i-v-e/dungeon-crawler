using UnityEngine;

namespace Depravity
{
    public class Attacking : Activity
    {
        private const float WAIT_BETWEEN_STRIKES = 1.0f;

        private enum State
        {
            TOO_FAR_AWAY,
            TOO_CLOSE,
            CLEAR_FOR_STRIKE,
            BLOCKED,
            BACK_BLOCKED
        }

        public string state = "";

        private Monster victim;
        private float closeEnoughSqr, tooCloseSqr,
            nextStrike = 0.0f;
        private bool lastWentLeft = false;

        private void WithinAttackDistance()
        {
            Debug.Log("Ready to strike");

        }

        public bool IsReady
        {
            get
            {
                return activityManager.Monster.AnimationManager.isActiveAndEnabled;
            }
        }

        private bool FindWeapon(Monster monster)
        {
            var weapon = GetComponentInChildren<Weapon>();
            if (weapon == null)
            {
                activityManager.Push<FindWeapon>().Activate();
                return false;
            }
            else
            {
                weapon.AddToRightHandOf(monster);
                return true;
            }
        }

        private void AssignFollow(BlockNavigator nav, Weapon weapon)
        {
            nav.OnReachedTarget = WithinAttackDistance;
            closeEnoughSqr = nav.Follow(victim, weapon.maximumReach, weapon.minimumReach);
            tooCloseSqr = closeEnoughSqr + weapon.minimumReach - weapon.maximumReach;
            closeEnoughSqr *= closeEnoughSqr;
            tooCloseSqr *= tooCloseSqr;
        }

        private void SetupAttack(Monster monster)
        {
            var anim = monster.AnimationManager;
            var weapon = monster.weapon;
            var nav = monster.BlockNavigator;
            anim.SetActivity(MonsterAnimationManager.Activity.ATTACKING);
            victim.OnBlockStateChanged += VictimDisabled;
            victim.OnDied += VictimDied;
            AssignFollow(nav, weapon);
        }

        private void StartAttacking()
        {
            var monster = activityManager.Monster;
            if (monster.weapon == null && !FindWeapon(monster))
            {
                return;
            }
            if (victim == null)
            {
                return;
            }
            SetupAttack(monster);
        }

        private void VictimDisabled(Monster victim, bool state)
        {
            activityManager.Pop();
        }

        private void VictimDied(Monster monster)
        {
            activityManager.Pop();
        }

        private void ClearVictim()
        {
            if (victim == null)
            {
                return;
            }
            victim.OnBlockStateChanged -= VictimDisabled;
            victim.OnDied -= VictimDied;
        }

        public Monster Victim
        {
            set
            {
                ClearVictim();
                victim = value;
            }
            get
            {
                return victim;
            }
        }

        private void OnDisable()
        {
            var monster = activityManager.Monster;
            var nav = monster.BlockNavigator;
            monster.AnimationManager.SetActivity(MonsterAnimationManager.Activity.IDLE);
            UpdateBlocking(monster, false);
            ClearVictim();
            nav.Follow(null);
        }

        public override void Activate()
        {
            StartAttacking();
        }

        private void Strike(float now, Monster monster, Weapon weapon)
        {
            nextStrike = now + WAIT_BETWEEN_STRIKES * weapon.slowness * Random.Range(0.75f, 1.25f);
            monster.AnimationManager.Attack();
        }

        private State CheckState(Monster monster)
        {
            var trans = monster.transform;
            var from = trans.position;
            from.y = monster.Height * 0.95f;
            var to = Victim.transform.position;
            to.y = Victim.Height * 0.95f;
            var dir = to - from;
            float sqrMag = dir.sqrMagnitude;
            if (sqrMag > closeEnoughSqr)
            {
                return State.TOO_FAR_AWAY;
            }
            if (sqrMag < tooCloseSqr)
            {
                if (Physics.Raycast(new Ray(from, -transform.forward), monster.weapon.maximumReach))
                {
                    return State.BACK_BLOCKED;
                }
                return State.TOO_CLOSE;
            }
            /*float len = Mathf.Sqrt(sqrMag);
            dir /= len;
            if (ObsticleNavigator.Raycast(new Ray(from, dir), len, Holdable.LAYER, out var hit) &&
                hit.collider != Victim.Collider
            ) {
                return State.BLOCKED;
            }*/
            return State.CLEAR_FOR_STRIKE;
        }

        private void MovedSideways()
        {
            var monster = activityManager.Monster;
            AssignFollow(monster.BlockNavigator, monster.weapon);
        }

        private bool TrySide(BlockNavigator nav, Vector3 from, Vector3 to, float girth)
        {
            float lift = girth * 2.0f;
            from.y += lift;
            to.y += lift;
            if (!ObsticleNavigator.FindObsticle(from, to, girth, out var _))
            {
                nav.Follow(null);
                nav.MoveTo(to);
                nav.OnReachedTarget = MovedSideways;
                return true;
            }
            return false;
        }

        private void MoveSideways(BlockNavigator nav, Monster monster)
        {
            var trans = monster.transform;
            var pos = trans.position;
            float girth = monster.radius * 2.0f;
            var right = Vector3.Cross((Victim.transform.position - pos).normalized, Vector3.up);
            if (lastWentLeft)
            {
                if (!TrySide(nav, pos, pos + right * girth, girth))
                {
                    if (TrySide(nav, pos, pos - right * girth, girth))
                    {
                        lastWentLeft = false;
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
            else if (!TrySide(nav, pos, pos - right * girth, girth))
            {
                if (TrySide(nav, pos, pos + right * girth, girth))
                {
                    lastWentLeft = true;
                    return;
                }
            }
            else
            {
                return;
            }
            MovedSideways();
        }

        private void UpdateBlocking(Monster monster, bool state)
        {
            var shield = monster.shield;
            if (shield == null)
            {
                return;
            }
            monster.AnimationManager.Blocking = state;
        }

        private void TryStrike(Monster monster)
        {
            float now = Time.timeSinceLevelLoad;
            if (nextStrike > now)
            {
                UpdateBlocking(monster, true);
                monster.BlockNavigator.ForceRotateToFace = Victim;
            }
            else
            {
                UpdateBlocking(monster, false);
                Strike(now, monster, monster.weapon);
            }
        }

        private void AdjustDistance(BlockNavigator nav, Monster monster)
        {
            var weapon = monster.weapon;
            nav.Follow(victim, weapon.maximumReach, weapon.minimumReach);
        }

        private void Update()
        {
            var monster = activityManager.Monster;
            var nav = monster.BlockNavigator;
            var navState = nav.CurrentState;
            if (navState != BlockNavigator.State.STATIONARY)
            {
                return;
            }
            Debug.Log("Stationary");
            State revised = CheckState(monster);
            state = revised.ToString();
            switch (revised)
            {
                case State.CLEAR_FOR_STRIKE:
                    TryStrike(monster);
                    break;
                case State.TOO_CLOSE:
                    UpdateBlocking(monster, true);
                    AdjustDistance(nav, monster);
                    break;
                case State.TOO_FAR_AWAY:
                    UpdateBlocking(monster, false);
                    AdjustDistance(nav, monster);
                    break;
                default:
                    MoveSideways(nav, monster);
                    break;
            }
        }
    }
}