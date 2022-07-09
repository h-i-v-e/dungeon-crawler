using System.Collections.Generic;
using UnityEngine;

namespace Depravity
{
    internal struct ScheduledMonsterEventHolder {
        internal MonsterManager.ScheduledMonsterEvent action;
        internal float time;
    }

    public class MonsterManager : MonoBehaviour
    {
        private readonly Dictionary<Vector2Int, LinkedList<Monster>> locations = new Dictionary<Vector2Int, LinkedList<Monster>>();
        private readonly HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        private readonly Heap<ScheduledMonsterEventHolder> scheduledEvents;

        public static MonsterManager Instance{ get; private set; }

        public MonsterManager()
        {
            scheduledEvents = new Heap<ScheduledMonsterEventHolder>(ScheduleLess);
            Instance = this;
        }

        private static bool ScheduleLess(ScheduledMonsterEventHolder a, ScheduledMonsterEventHolder b)
        {
            return a.time < b.time;
        }

        public delegate void ScheduledMonsterEvent();

        public void Schedule(ScheduledMonsterEvent evt, float time)
        {
            scheduledEvents.Push(new ScheduledMonsterEventHolder {
                action = evt,
                time = time
            });
        }

        private void BlockActiveStateChanged(Block block, bool active)
        {
            if (locations.TryGetValue(Dungeon.Instance.GetOffset(block.transform.position), out var monsters))
            {
                for (var i = monsters.First; i != null; i = i.Next)
                {
                    i.Value.UpdateBlockState(active);
                }
            }
        }

        private void Start()
        {
            Dungeon.Instance.OnBlockActiveStateChanged += BlockActiveStateChanged;
        }

        private void Update()
        {
           if (!scheduledEvents.IsEmpty && scheduledEvents.Peek().time < Time.timeSinceLevelLoad)
           {
                scheduledEvents.Pop().action();
           }
        }

        public void AddMonster(Monster monster)
        {
            monster.transform.parent = transform;
            MoveMonsterTo(monster, Dungeon.Instance.GetOffset(monster.transform.position));
        }

        public void UpdateMonsterPosition(Monster monster)
        {
            var pos = Dungeon.Instance.GetOffset(monster.transform.position);
            if (pos == monster.BlockOffset)
            {
                return;
            }
            MoveMonsterTo(monster, pos);
        }

        public void MoveMonsterTo(Monster monster, Vector2Int pos)
        {
            monster.BlockOffset = pos;
            var node = monster.locationNode;
            var list = node.List;
            if (list != null)
            {
                list.Remove(node);
            }
            if (!locations.TryGetValue(pos, out list)){
                list = new LinkedList<Monster>();
                locations[pos] = list;
            }
            list.AddFirst(node);
            if (!Dungeon.Instance.BlockActive(pos))
            {
                monster.UpdateBlockState(false);
            }
        }

        public bool FindMonstersAt(Vector2Int pos, out LinkedListNode<Monster> node)
        {
            if (!locations.TryGetValue(pos, out var list))
            {
                node = null;
                return false;
            }
            node = list.First;
            return node != null;
        }

        private bool FindClosestMonster(Monster monster, LinkedListNode<Monster> node, out Monster enemy)
        {
            float dist = float.MaxValue;
            enemy = null;
            do
            {
                if (!(node.Value.IsDead || monster.IsFriendly(node.Value)))
                {
                    float d = (node.Value.transform.position - monster.transform.position).sqrMagnitude;
                    if (d <= dist)
                    {
                        dist = d;
                        enemy = node.Value;
                    }
                }
                node = node.Next;
            } while (node != null);
            return enemy != null;
        }

        private bool CheckBlock(Monster monster, Vector2Int pos, int depth, int maxDepth, out Monster enemy)
        {
            visited.Add(pos);
            if (!Dungeon.Instance.FindBlockAt(pos, out var block))
            {
                enemy = null;
                return false;
            }
            if (FindMonstersAt(pos, out var node) && FindClosestMonster(monster, node, out enemy))
            {
                return true;
            }
            if (depth == maxDepth)
            {
                enemy = null;
                return false;
            }
            ++depth;
            if (block.enterTop && block.attachedTop.enterBottom && CheckBlock(monster, pos + Block.offsetTop, depth, maxDepth, out enemy))
            {
                return true;
            }
            if (block.enterRight && block.attachedRight.enterLeft && CheckBlock(monster, pos + Block.offsetRight, depth, maxDepth, out enemy))
            {
                return true;
            }
            if (block.enterBottom && block.attachedBottom.enterTop && CheckBlock(monster, pos + Block.offsetBottom, depth, maxDepth, out enemy))
            {
                return true;
            }
            if (block.enterLeft && block.attachedLeft.enterRight && CheckBlock(monster, pos + Block.offsetLeft, depth, maxDepth, out enemy))
            {
                return true;
            }
            enemy = null;
            return false;
        }

        public bool FindEnemy(Monster monster, int maxDepth, out Monster enemy)
        {
            return CheckBlock(monster, Dungeon.Instance.GetOffset(monster.transform.position), 0, maxDepth, out enemy);
        }
    }
}
