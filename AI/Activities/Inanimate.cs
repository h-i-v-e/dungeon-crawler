using UnityEngine;

namespace Depravity
{
    public class Inanimate : Activity
    {
        private float reviveAt;

        public override void Activate()
        {
            var anim = activityManager.Monster.AnimationManager;
            anim.SetActivity(MonsterAnimationManager.Activity.IDLE);
            anim.Down();
            reviveAt = Time.timeSinceLevelLoad + Random.Range(0.0f, 5.0f);
        }

        private void Update()
        {
            if (Time.timeSinceLevelLoad >= reviveAt)
            {
                var player = Controller.Player;
                if (player.IsDead)
                {
                    enabled = false;
                    return;
                }
                activityManager.Push<KillEverything>();
                var attacking = activityManager.Push<Attacking>();
                attacking.Victim = player;
                attacking.Activate();
            }
        }
    }
}
