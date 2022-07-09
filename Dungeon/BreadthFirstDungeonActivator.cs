using System.Collections.Generic;
using UnityEngine;

namespace Depravity
{
    public class BreadthFirstDungeonActivator : DungeonActivator
    {
        private struct SearchNode
        {
            public int depth;
            public Block block;
        }

        private readonly HashSet<Block> visited = new HashSet<Block>();
        private readonly Queue<SearchNode> fringe = new Queue<SearchNode>();

        [SerializeField]
        private int deactivateBlockAtDepth;

        private void SetBlockActiveState(Block block, bool active)
        {
            block.gameObject.SetActive(active);
            Dungeon.SetBlockActiveState(block, active);
        }

        private void Enqueue(Block block, int depth)
        {
            if (!visited.Contains(block))
            {
                visited.Add(block);
                fringe.Enqueue(new SearchNode
                {
                    depth = depth,
                    block = block
                });
            }
        }

        private bool SetActive(Block block, int depth)
        {
            if (depth == deactivateBlockAtDepth)
            {
                if (block.gameObject.activeSelf)
                {
                    SetBlockActiveState(block, false);
                }
                return false;
            }
            if (!block.gameObject.activeSelf)
            {
                SetBlockActiveState(block, true);
            }
            return true;
        }

        private void ExpandBlock()
        {
            SearchNode node = fringe.Dequeue();
            var block = node.block;
            int depth = node.depth + 1;
            if (!SetActive(block, depth))
            {
                return;
            }
            if (block.enterTop)
            {
                var attach = block.attachedTop;
                if (attach.enterBottom)
                {
                    Enqueue(attach, depth);
                }
            }
            if (block.enterRight)
            {
                var attach = block.attachedRight;
                if (attach.enterLeft)
                {
                    Enqueue(attach, depth);
                }
            }
            if (block.enterBottom)
            {
                var attach = block.attachedBottom;
                if (attach.enterTop)
                {
                    Enqueue(attach, depth);
                }
            }
            if (block.enterLeft)
            {
                var attach = block.attachedLeft;
                if (attach.enterRight)
                {
                    Enqueue(attach, depth);
                }
            }
        }

        public override void ActivateReachableFrom(Block block)
        {
            visited.Clear();
            Enqueue(block, 0);
            while (fringe.Count > 0)
            {
                ExpandBlock();
            }
        }
    }
}
