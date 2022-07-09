namespace Depravity
{
    public class TransientWaypoint : Waypoint
    {
        protected override void Activated()
        {
            MoveToNextWaypoint(false);
        }
    }
}
