namespace Depravity
{
    public class SitWaypoint : DoSomethingForAWhileWaypoint
    {
        protected override void AssignAnimation(IWaypointAnimationManager animationManager)
        {
            animationManager.Sit();
        }
    }
}
