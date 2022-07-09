using System.Collections.Generic;
using UnityEngine;

namespace Depravity
{
    internal class ActiveBlockIterator
    {
        private Block.BlockCallback callback;
        private readonly HashSet<Block> visited = new HashSet<Block>();

        private void Visit(Block block)
        {
            visited.Add(block);
            callback(block);
            block.WithEachNeighbour(Check);
        }

        private void Check(Block block)
        {
            if (visited.Contains(block))
            {
                return;
            }
            Visit(block);
        }

        internal void WithEachActiveBlock(Block.BlockCallback callback)
        {
            if (!Dungeon.Instance.TryGetBlock(Controller.Player.gameObject, out var block))
            {
                return;
            }
            this.callback = callback;
            Visit(block);
            visited.Clear();
        }
    }

    internal class ActiveBlockIteratorPool : ResourcePool<ActiveBlockIterator>
    {
        private static ActiveBlockIterator New()
        {
            return new ActiveBlockIterator();
        }

        internal ActiveBlockIteratorPool() : base(New)
        { 
        }
    }

    public static class Helpers
    {
        private static readonly ActiveBlockIteratorPool activeBlockIteratorPool = new ActiveBlockIteratorPool();

        public static Vector3 ComputeExtents(GameObject gameObject)
        {
            var colliders = gameObject.GetComponents<Collider>();
            int len = colliders.Length;
            if (len == 0)
            {
                return Vector3.zero;
            }
            var bounds = colliders[0].bounds;
            for (int i = 1, j = colliders.Length; i < j; bounds.Encapsulate(colliders[i++].bounds)) ;
            return bounds.extents;
        }

        public static float ComputeGirth(Vector3 extents)
        {
            return Mathf.Max(extents.x, extents.z) + Mathf.Epsilon;
        }

        public static void WithEachActiveBlock(Block.BlockCallback callback)
        {
            var itr = activeBlockIteratorPool.Allocate();
            itr.WithEachActiveBlock(callback);
            activeBlockIteratorPool.Release(itr);
        }
    }
}
