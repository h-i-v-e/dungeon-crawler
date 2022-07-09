using UnityEngine;
using System.Collections;

namespace Depravity
{

    public abstract class Activity : MonoBehaviour
    {
        protected ActivityManager activityManager;

        public abstract void Activate();

        internal void SetActivityManager(ActivityManager manager)
        {
            activityManager = manager;
        }

        protected delegate void MonoBehaviourReady();

        private IEnumerator WhileActivityManagerNull(MonoBehaviourReady ready)
        {
            while (activityManager == null)
            {
                yield return null;
            }
            ready();
        }

        protected void WaitForActivityManager(MonoBehaviourReady ready)
        {
            if (activityManager == null)
            {
                StartCoroutine(WhileActivityManagerNull(ready));
            }
            else
            {
                ready();
            }
        }

        private IEnumerator WaitForMonoBehaviour(MonoBehaviour target, MonoBehaviourReady action)
        {
            while (!target.isActiveAndEnabled)
            {
                yield return null;
            }
            action();
        }

        protected void WhenReady(MonoBehaviour target, MonoBehaviourReady action)
        {
            if (target.isActiveAndEnabled)
            {
                action();
            }
            else
            {
                StartCoroutine(WaitForMonoBehaviour(target, action));
            }
        }
    }
}
