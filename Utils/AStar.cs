using System.Collections.Generic;

namespace Depravity
{
    public class AStarNode<T>
    {
        public T state;
        public float dist;
        public AStarNode<T> previous;
    }

    public abstract class AStar<T>
    {
        private readonly HashSet<T> visited = new HashSet<T>();
        private readonly Heap<AStarNode<T>> fringe;
        private readonly List<AStarNode<T>> pool = new List<AStarNode<T>>();
        private readonly List<T> branches = new List<T>();

        private int usedNodes;

        public AStar()
        {
            fringe = new Heap<AStarNode<T>>(Less);
        }

        public static bool Less(AStarNode<T> x, AStarNode<T> y)
        {
            return x.dist < y.dist;
        }

        protected abstract float ApplyHueristic(AStarNode<T> parent, T current);

        protected abstract bool IsGoal(T state);

        protected abstract void Expand(T from, List<T> output);

        private AStarNode<T> AllocateNode()
        {
            if (pool.Count == usedNodes)
            {
                pool.Add(new AStarNode<T>());
            }
            return pool[usedNodes++];
        }

        private void Push(AStarNode<T> parent, T state)
        {
            visited.Add(state);
            var node = AllocateNode();
            node.state = state;
            node.previous = parent;
            node.dist = ApplyHueristic(parent, state);
            fringe.Push(node);
        }

        public bool Search(T from, List<T> solution)
        {
            if (IsGoal(from))
            {
                solution.Add(from);
                return true;
            }
            visited.Clear();
            fringe.Clear();
            usedNodes = 0;
            Push(null, from);
            while (!fringe.IsEmpty)
            {
                var best = fringe.Pop();
                branches.Clear();
                Expand(best.state, branches);
                for (int i = 0, j = branches.Count; i != j; ++i)
                {
                    var state = branches[i];
                    if (visited.Contains(state))
                    {
                        continue;
                    }
                    if (IsGoal(state))
                    {
                        solution.Add(state);
                        solution.Add(best.state);
                        while (best.previous != null)
                        {
                            best = best.previous;
                            solution.Add(best.state);
                        }
                        return true;
                    }
                    Push(best, state);
                }
            }
            return false;
        }
    }
}
