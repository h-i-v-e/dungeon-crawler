using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Depravity
{
    [RequireComponent(typeof(Activity)), RequireComponent(typeof(BlockNavigator))]
    public class ActivityManager : MonoBehaviour
    {
        private static readonly List<Activity> activityDeactivatorList = new List<Activity>();
        private readonly Stack<Activity> activities = new Stack<Activity>();
        private readonly Dictionary<Type, Activity> options = new Dictionary<Type, Activity>();

        private Activity defaultActivity;

        public Monster Monster { get; private set; }

        private Activity GetActivity(Type type)
        {
            if (options.TryGetValue(type, out var activity))
            {
                activity.enabled = true;
                return activity;
            }
            activity = options[type] = gameObject.AddComponent(type) as Activity;
            activity.SetActivityManager(this);
            return activity;
        }

        private T PushNoCheck<T>() where T : Activity
        {
            var activity = GetActivity(typeof(T));
            activities.Push(activity);
            return activity as T;
        }

        public T SetActivity<T>() where T : Activity
        {
            if (activities.Count == 0)
            {
                defaultActivity.enabled = false;
            }
            else {
                activities.Peek().enabled = false;
                activities.Clear();
            }
            return PushNoCheck<T>();
        }

        public T Push<T>() where T : Activity
        {
            if (activities.Count == 0)
            {
                defaultActivity.enabled = false;
            }
            else {
                activities.Peek().enabled = false;
            }
            return PushNoCheck<T>();
        }

        private void Died(Monster monster)
        {
            activityDeactivatorList.Clear();
            GetComponents(activityDeactivatorList);
            for (int i = 0, j = activityDeactivatorList.Count; i != j; activityDeactivatorList[i++].enabled = false) ;
            enabled = false;
        }

        private void Attacked(Monster victim, Monster attacker)
        {
            if (attacker.GoneRogue || (activities.Count != 0 && activities.Peek() is Attacking))
            {
                return;
            }
            if (attacker.teamProfile != null && attacker.teamProfile.teamName == victim.teamProfile.teamName)
            {
                attacker.GoneRogue = true;
            }
            var attacking = Push<Attacking>();
            attacking.Victim = attacker;
            attacking.Activate();
        }

        private void Awake()
        {
            Monster = GetComponent<Monster>();
            Monster.OnDied += Died;
            Monster.OnAttacked += Attacked;
        }

        private IEnumerator ActivateDefaultActivity()
        {
            yield return null;
            defaultActivity.enabled = true;
            defaultActivity.Activate();
        }

        private void Start()
        {
            defaultActivity = GetComponent<Activity>();
            defaultActivity.SetActivityManager(this);
            StartCoroutine(ActivateDefaultActivity());
        }

        private void Activate(Activity activity)
        {
            activity.enabled = true;
            activity.Activate();
        }

        public void Pop()
        {
            switch (activities.Count)
            {
                case 0:
                    return;
                case 1:
                    activities.Pop().enabled = false;
                    Activate(defaultActivity);
                    return;
                default:
                    activities.Pop().enabled = false;
                    Activate(activities.Peek());
                    return;
            }
        }
    }
}
