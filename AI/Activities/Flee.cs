using UnityEngine;

namespace Depravity
{
    public class Flee : Activity
    {
        private Vector3 furthest;
        private float distance;

        private void CheckBlock(Block block)
        {
            var pos = block.transform.position;
            float dist = (pos - Controller.Player.transform.position).sqrMagnitude;
            if (dist > distance)
            {
                distance = dist;
                furthest = pos;
            }
        }

        private void Fled()
        {
            activityManager.Pop();
        }

        public override void Activate()
        {
            var dungeon = Dungeon.Instance;
            dungeon.FindBlockAt(dungeon.GetOffset(transform.position), out var block);
            distance = float.MinValue;
            block.WithEachNeighbour(CheckBlock);
            var nav = activityManager.Monster.BlockNavigator;
            nav.OnReachedTarget = Fled;
            nav.MoveTo(furthest);
        }
    }
}
