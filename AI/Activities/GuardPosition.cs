using UnityEngine;

namespace Depravity
{

    public class GuardPosition : Activity
    {
        public LoiterMarker loiterMarker;

        private float nextScan = 0.0f;

        private void FacingTarget()
        {
            activityManager.Monster.AnimationManager.Animator.applyRootMotion = false;
        }

        private void ReachedTarget()
        {
            var nav = activityManager.Monster.BlockNavigator;
            nav.OnReachedTarget = FacingTarget;
            nav.Rotate(loiterMarker.transform.rotation);
        }

        public override void Activate()
        {
            var nav = activityManager.Monster.BlockNavigator;
            nav.OnReachedTarget = ReachedTarget;
            nav.MoveTo(loiterMarker.transform.position);
        }

        private void OnDisable()
        {
            activityManager.Monster.AnimationManager.Animator.applyRootMotion = true;
        }

        private void Update()
        {
            float now = Time.timeSinceLevelLoad;
            if (now >= nextScan)
            {
                if (MonsterManager.Instance.FindEnemy(activityManager.Monster, 2, out var victim))
                {
                    var attacking = activityManager.Push<Attacking>();
                    attacking.Victim = victim;
                    attacking.Activate();
                }
                nextScan = now + Random.Range(0.5f, 1.5f);
            }
        }
    }
}