namespace Depravity
{

    public class Patrolling : Activity
    {
        public override void Activate()
        {
            activityManager.Monster.AnimationManager.SetActivity(MonsterAnimationManager.Activity.IDLE);
        }
    }
}