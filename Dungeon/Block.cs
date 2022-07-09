#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Depravity
{

    public class Block : MonoBehaviour
    {
        public const byte TOP = 1, RIGHT = 2, BOTTOM = 4, LEFT = 8;
        public static readonly Vector2Int offsetTop = new Vector2Int(0, 1),
            offsetRight = new Vector2Int(1, 0),
            offsetBottom = new Vector2Int(0, -1),
            offsetLeft = new Vector2Int(-1, 0);

        public bool enterTop, enterRight, enterBottom, enterLeft, levelExit;
        public int spawnSlots = 1;

        [SerializeField]
        private Transform centre;

        [SerializeField]
        private DecorationProvider decorationProvider;

        public Block attachedTop, attachedRight, attachedBottom, attachedLeft;

        public delegate void BlockCallback(Block block);

        private byte deadEnds = 0;

        public Vector3 Centre
        {
            get
            {
                return (centre == null ? transform : centre).position;
            }
        }

        public void Decorate()
        {
            if (decorationProvider)
            {
                decorationProvider.Decorate(transform);
            }
        }

        public bool HasTop
        {
            get
            {
                return enterTop && attachedTop != null && attachedTop.enterBottom;
            }
        }

        public bool HasRight
        {
            get
            {
                return enterRight && attachedRight != null && attachedRight.enterLeft;
            }
        }

        public bool HasBottom
        {
            get
            {
                return enterBottom && attachedBottom != null && attachedBottom.enterTop;
            }
        }

        public bool HasLeft
        {
            get
            {
                return enterLeft && attachedLeft != null && attachedLeft.enterRight;
            }
        }

        public void WithEachNeighbour(BlockCallback callback)
        {
            if (HasTop)
            {
                callback(attachedTop);
            }
            if (HasRight)
            {
                callback(attachedRight);
            }
            if (HasBottom)
            {
                callback(attachedBottom);
            }
            if (HasLeft)
            {
                callback(attachedLeft);
            }
        }

        public bool IsDeadEnd(byte side)
        {
            return (deadEnds & side) != 0;
        }

        public static Vector3 TopEntranceOffset
        {
            get
            {
                return new Vector3(0.0f, 0.0f, BlockLibrary.Instance.halfBlockSize);
            }
        }

        public static Vector3 RightEntranceOffset
        {
            get
            {
                return new Vector3(BlockLibrary.Instance.halfBlockSize, 0.0f, 0.0f);
            }
        }

        public static Vector3 BottomEntranceOffset
        {
            get
            {
                return new Vector3(0.0f, 0.0f, -BlockLibrary.Instance.halfBlockSize);
            }
        }

        public static Vector3 LeftEntranceOffset
        {
            get
            {
                return new Vector3(-BlockLibrary.Instance.halfBlockSize, 0.0f, 0.0f);
            }
        }

        private void AddDeadEnd(Vector3 offset, Quaternion rotation)
        {
            var trans = transform;
            var end = DeadEnds.Instance.AddTo(trans).transform;
            end.rotation = rotation;
            end.position = trans.position + offset;
        }

        public void SetDeadEnd(byte side)
        {
            if (IsDeadEnd(side))
            {
                return;
            }
            deadEnds |= side;
            switch (side)
            {
                case TOP:
                    AddDeadEnd(TopEntranceOffset, Rotations.rot180);
                    break;
                case RIGHT:
                    AddDeadEnd(RightEntranceOffset, Rotations.rotNeg90);
                    break;
                case BOTTOM:
                    AddDeadEnd(BottomEntranceOffset, Quaternion.identity);
                    break;
                default:
                    AddDeadEnd(LeftEntranceOffset, Rotations.rot90);
                    break;
            }
        }

        private void RotateAttached()
        {
            var swap = attachedTop;
            attachedTop = attachedRight;
            attachedRight = attachedBottom;
            attachedBottom = attachedLeft;
            attachedLeft = swap;
        }

        private void RotateEntrances()
        {
            bool swap = enterTop;
            enterTop = enterRight;
            enterRight = enterBottom;
            enterBottom = enterLeft;
            enterLeft = swap;
        }

        public void Rotate(int steps)
        {
            transform.localRotation = Quaternion.Euler(0.0f, steps * -90.0f, 0.0f);
            while (--steps != -1)
            {
                RotateEntrances();
                RotateAttached();
            }
        }

        public bool CanAttachTop
        {
            get
            {
                return enterTop && attachedTop == null;
            }
        }

        public bool CanAttachRight
        {
            get
            {
                return enterRight && attachedRight == null;
            }
        }

        public bool CanAttachBottom
        {
            get
            {
                return enterBottom && attachedBottom == null;
            }
        }

        public bool CanAttachLeft
        {
            get
            {
                return enterLeft && attachedLeft == null;
            }
        }

        public int ExitCount
        {
            get
            {
                int count = 0;
                if (enterTop)
                {
                    ++count;
                }
                if (enterRight)
                {
                    ++count;
                }
                if (enterBottom)
                {
                    ++count;
                }
                if (enterLeft)
                {
                    ++count;
                }
                return count;
            }
        }

#if UNITY_EDITOR
        private void MarkNeighbour(Vector3 pos, Color color, Block block)
        {
            Gizmos.color = color;
            Gizmos.DrawRay(pos, block.transform.position - transform.position);
        }

        private void OnDrawGizmos()
        {
            if (Selection.Contains(gameObject))
            {
                Vector3 pos = transform.position;
                pos.y += 1.0f;
                if (HasTop)
                {
                    MarkNeighbour(pos, Color.red, attachedTop);
                }
                if (HasRight)
                {
                    MarkNeighbour(pos, Color.green, attachedRight);
                }
                if (HasBottom)
                {
                    MarkNeighbour(pos, Color.blue, attachedBottom);
                }
                if (HasLeft)
                {
                    MarkNeighbour(pos, Color.yellow, attachedLeft);
                }
            }
        }
#endif
    }
}
