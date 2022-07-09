using System.Collections;
using UnityEngine;

namespace Depravity
{

    public class MoveToRandomNeighbouringBlock : Activity
    {
        public delegate void EnvironmentCheck();

        private static readonly Block[] neighbours = new Block[4];

        public EnvironmentCheck CheckEnvironment { set; private get; }

        private Block lastBlock;

        private void ReachedTarget()
        {
            activityManager.Pop();
        }

        private int FillNeighbours(Block block)
        {
            int num = 0;
            if (block.HasTop && block.attachedTop != lastBlock)
            {
                neighbours[num++] = block.attachedTop;
            }
            if (block.HasRight && block.attachedRight != lastBlock)
            {
                neighbours[num++] = block.attachedRight;
            }
            if (block.HasBottom && block.attachedBottom != lastBlock)
            {
                neighbours[num++] = block.attachedBottom;
            }
            if (block.HasLeft && block.attachedLeft != lastBlock)
            {
                neighbours[num++] = block.attachedLeft;
            }
            return num;
        }

        private void MoveTo(Block block)
        {
            var nav = activityManager.Monster.BlockNavigator;
            nav.OnReachedTarget = ReachedTarget;
            nav.MoveTo(block.transform.position);
        }

        private void FindSomewhereToGo(bool hasWaited)
        {
            if (!Dungeon.Instance.TryGetBlock(gameObject, out var block))
            {
                Debug.Log("No block");
                return;
            }
            int options = FillNeighbours(block);
            if (options == 0)
            {
                if (lastBlock != null)
                {
                    MoveTo(lastBlock);
                    lastBlock = block;
                }
                else if (hasWaited)
                {
                    activityManager.Pop();
                }
                else
                {
                    StartCoroutine(WaitAFrame());
                }
                return;
            }
            MoveTo(neighbours[Random.Range(0, options)]);
            lastBlock = block;
        }

        private IEnumerator WaitAFrame()
        {
            yield return null;
            FindSomewhereToGo(true);
        }

        public override void Activate()
        {
            FindSomewhereToGo(false);
        }

        private void Update()
        {
            CheckEnvironment?.Invoke();
        }
    }
}