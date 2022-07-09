using System.Collections.Generic;
using UnityEngine;

namespace Depravity
{
    public struct DungeonNavigatorNode
    {
        public Vector2Int offset;
        public Vector3 join, centre;
    }

    public class DungeonNavigator : AStar<DungeonNavigatorNode>
    {
        private static Vector2Int goal;
        private static readonly DungeonNavigator instance = new DungeonNavigator();
        private static readonly Stack<List<Vector3>> paths = new Stack<List<Vector3>>();
        private static readonly List<DungeonNavigatorNode> path = new List<DungeonNavigatorNode>(); 

        private static List<Vector3> AllocatePath()
        {
            return paths.Count == 0 ? new List<Vector3>() : paths.Pop();
        }

        public static void ReleasePath(List<Vector3> path)
        {
            path.Clear();
            paths.Push(path);
        }

        private static void CopyOutPath(List<Vector3> output)
        {
            output.Add(path[0].centre);
            for (int i = 1, j = path.Count; i < j; ++i)
            {
                var node = path[i];
                output.Add(node.join);
                output.Add(node.centre);
            }
        }

        public static bool FindPath(Vector2Int from, Vector2Int to, out List<Vector3> path)
        {
            if (!Dungeon.Instance.FindBlockAt(to, out var block))
            {
                path = null;
                return false;
            }
            goal = from;
            path = AllocatePath();
            DungeonNavigator.path.Clear();
            if (instance.Search(new DungeonNavigatorNode {
                offset = to,
                centre = block.Centre
            }, DungeonNavigator.path))
            {
                CopyOutPath(path);
                return true;
            }
            ReleasePath(path);
            path = null;
            return false;
        }


        protected override float ApplyHueristic(AStarNode<DungeonNavigatorNode> parent, DungeonNavigatorNode from)
        {
            return (goal - from.offset).sqrMagnitude;
        }

        private static DungeonNavigatorNode CreateNode(DungeonNavigatorNode from, Block block, Vector2Int offset, Vector3 gate)
        {
            return new DungeonNavigatorNode
            {
                offset = from.offset + offset,
                join = block.transform.position + gate,
                centre = block.Centre
            };
        }

        protected override void Expand(DungeonNavigatorNode from, List<DungeonNavigatorNode> output)
        {
            if (!Dungeon.Instance.FindBlockAt(from.offset, out var block))
            {
                return;
            }
            if (block.attachedTop != null && block.attachedTop.enterBottom)
            {
                output.Add(CreateNode(from, block, Block.offsetTop, Block.BottomEntranceOffset));
            }
            if (block.attachedRight != null && block.attachedRight.enterLeft)
            {
                output.Add(CreateNode(from, block, Block.offsetRight, Block.LeftEntranceOffset));
            }
            if (block.attachedBottom != null && block.attachedBottom.enterTop)
            {
                output.Add(CreateNode(from, block, Block.offsetBottom, Block.TopEntranceOffset));
            }
            if (block.attachedLeft != null && block.attachedLeft.enterRight)
            {
                output.Add(CreateNode(from, block, Block.offsetLeft, Block.RightEntranceOffset));
            }
        }

        protected override bool IsGoal(DungeonNavigatorNode state)
        {
            return state.offset == goal;
        }
    }
}