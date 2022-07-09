namespace Depravity
{
    public class IdleWaypoint : DoSomethingForAWhileWaypoint
    {
        protected override void AssignAnimation(IWaypointAnimationManager animationManager)
        {
            animationManager.Idle();
        }
    }
}