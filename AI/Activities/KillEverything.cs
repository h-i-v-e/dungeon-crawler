using UnityEngine;

namespace Depravity
{

    public class KillEverything : Activity
    {
        private const float ENVIRONMENT_CHECK_INTERVAL = 1.0f;

        /*private static Monster closest;
        private static float sqrDist;*/

        private float nextCheck = 0.0f;

        /*private void CheckMonster(Monster monster)
        {
            if (monster.gameObject == gameObject || monster.IsDead || activityManager.Monster.IsFriendly(monster))
            {
                return;
            }
            var dist = (transform.position - monster.transform.position).sqrMagnitude;
            if (closest == null || dist < sqrDist)
            {
                sqrDist = dist;
                closest = monster;
            }
        }*/

        /*private void CheckBlock(Block block)
        {
            if (!MonsterManager.Instance.FindMonstersAt(Dungeon.Instance.GetOffset(block.transform.position), out var monsters))
            {
                return;
            }
            do
            {
                CheckMonster(monsters.Value);
                monsters = monsters.Next;
            } while (monsters != null);
        }*/

        /*private bool FindNextVictim()
        {
            closest = null;
            Helpers.WithEachActiveBlock(CheckBlock);
            return closest != null;
        }*/

        private void IncrementNextCheck()
        {
            nextCheck += Random.Range(ENVIRONMENT_CHECK_INTERVAL * 0.5f, ENVIRONMENT_CHECK_INTERVAL * 1.5f);
        }

        private void CheckEnvironment()
        {
            if (Time.timeSinceLevelLoad < nextCheck)
            {
                return;
            }
            if (MonsterManager.Instance.FindEnemy(activityManager.Monster, 2, out var _))
            {
                activityManager.Pop();
            }
            else
            {
                IncrementNextCheck();
            }
        }

        private void Bored()
        {
            if (MonsterManager.Instance.FindEnemy(activityManager.Monster, 2, out var closest))//FindNextVictim())
            {
                var attacking = activityManager.Push<Attacking>();
                attacking.Victim = closest;
                attacking.Activate();
            }
            else
            {
                IncrementNextCheck();
                var move = activityManager.Push<MoveToRandomNeighbouringBlock>();
                move.CheckEnvironment = CheckEnvironment;
                move.Activate();
            }
        }

        /*private void OnVictimDied(Monster victim)
        {
            victim.OnDied -= OnVictimDied;
            Bored();
        }*/

        public override void Activate()
        {
            Bored();
        }
    }
}
