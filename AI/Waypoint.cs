using UnityEngine;

namespace Depravity
{
    public abstract class Waypoint : MonoBehaviour
    {
        public Waypoint nextWaypoint;

        protected WaypointAgent agent;

        protected abstract void Activated();

        private void ReachedNext()
        {
            nextWaypoint.Activate(agent);
        }

        protected void MoveToNextWaypoint(bool run)
        {
            agent.AnimationManager.MoveTo(nextWaypoint.transform.position, run, ReachedNext);
        }

        public void Activate(WaypointAgent agent)
        {
            this.agent = agent;
            Activated();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            const float RADIUS = 0.15f;
            var pos = transform.position;
            pos.y += RADIUS;
            Gizmos.color = new Color(1.0f, 0.75f, 0.75f);
            Gizmos.DrawSphere(pos, RADIUS);
            if (nextWaypoint != null)
            {
                var next = nextWaypoint.transform.position;
                next.y += RADIUS;
                Gizmos.DrawLine(pos, next);
            }
        }
#endif
    }
}