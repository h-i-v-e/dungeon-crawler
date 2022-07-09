using UnityEngine;
using System.Collections;

namespace Depravity
{
    public abstract class DoSomethingForAWhileWaypoint : Waypoint
    {
        [SerializeField]
        private float minTime, maxTime;

        private float snapOutOfIt;

        protected abstract void AssignAnimation(IWaypointAnimationManager animationManager);

        private IEnumerator DoSomething()
        {
            AssignAnimation(agent.AnimationManager);
            snapOutOfIt = Time.unscaledTime + Random.Range(minTime, maxTime);
            do
            {
                yield return null;
            }
            while (Time.unscaledTime < snapOutOfIt);
            MoveToNextWaypoint(false);
        }

        protected override void Activated()
        {
            StartCoroutine(DoSomething());
        }
    }
}
