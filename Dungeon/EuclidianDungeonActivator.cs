using UnityEngine;

namespace Depravity
{
    public class EuclidianDungeonActivator : DungeonActivator
    {
        [SerializeField]
        protected int deactivateBlockAtDepth;

        private static void SetActive(Dungeon dungeon, int x, int y, bool active)
        {
            if (dungeon.FindBlockAt(new Vector2Int(x, y), out var block) && block.gameObject.activeSelf != active)
            {
                dungeon.SetBlockActiveState(block, active);
            }
        }

        public override void ActivateReachableFrom(Block block)
        {
            var dungeon = Dungeon.Instance;
            var offset = dungeon.GetOffset(block.transform.position);
            int xbegin = offset.x - deactivateBlockAtDepth,
                xend = offset.x + deactivateBlockAtDepth,
                ybegin = offset.y - deactivateBlockAtDepth,
                yend = offset.y + deactivateBlockAtDepth;
            for (int y = ybegin; y <= yend; ++y)
            {
                SetActive(dungeon, xbegin, y, false);
                SetActive(dungeon, xend, y, false);
            }
            for (int x = xbegin + 1; x < xend; ++x)
            {
                SetActive(dungeon, x, ybegin, false);
                SetActive(dungeon, x, yend, false);
            }
            for (++ybegin; ybegin < yend; ++ybegin)
            {
                for (int x = xbegin + 1; x < xend; ++x)
                {
                    SetActive(dungeon, x, ybegin, true);
                }
            }
        }
    }
}