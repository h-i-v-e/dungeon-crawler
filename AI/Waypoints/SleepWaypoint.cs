namespace Depravity
{
    public class SleepWaypoint : DoSomethingForAWhileWaypoint
    {
        protected override void AssignAnimation(IWaypointAnimationManager animationManager)
        {
            animationManager.Sleep();
        }
    }
}
