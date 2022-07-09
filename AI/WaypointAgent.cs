using UnityEngine;

namespace Depravity
{
    [RequireComponent(typeof(IWaypointAnimationManager))]
    public class WaypointAgent : MonoBehaviour
    {
        [SerializeField]
        private Waypoint startAt;

        public delegate void ReachedTarget();

        public IWaypointAnimationManager AnimationManager { get; private set; }

        private void Awake()
        {
            AnimationManager = GetComponent<IWaypointAnimationManager>();
        }

        private void OnEnable()
        {
            var pos = startAt.transform.position;
            transform.position = pos;
            transform.rotation = Quaternion.LookRotation((startAt.nextWaypoint.transform.position - pos).normalized);
            startAt.Activate(this);
        }
    }
}