using UnityEngine;

namespace Depravity
{
    public interface IWaypointAnimationManager
    {
        void MoveTo(Vector3 position, bool run, WaypointAgent.ReachedTarget reachedTarget);

        void Idle();

        void Sleep();

        void Sit();
    }
}