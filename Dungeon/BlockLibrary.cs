using System.Collections.Generic;
using UnityEngine;

namespace Depravity
{

    public class BlockLibrary : MonoBehaviour
    {
        private readonly List<Block> top = new List<Block>(),
                right = new List<Block>(),
                bottom = new List<Block>(),
                left = new List<Block>(),
                validOptions = new List<Block>(),
                filteredOptions = new List<Block>();

        public float blockSize = 15.0f;
        public int maxBlocks = 32;

        //[SerializeField]
        //private Block exit;

        [HideInInspector]
        public float halfBlockSize;

        private int blockCount = 0;

        public static BlockLibrary Instance { get; private set; }

        public BlockLibrary()
        {
            Instance = this;
            halfBlockSize = blockSize * 0.5f;
        }

        public void Reset()
        {
            blockCount = 0;
        }

        public bool Ready
        {
            get
            {
                return top.Count != 0;
            }
        }

        private void AddValidOptions(Vector3 position, Vector2Int offset)
        {
            validOptions.Clear();
            var dungeon = Dungeon.Instance;
            var pos = dungeon.GetOffset(position) + offset;
            for (int i = 0, j = filteredOptions.Count; i != j; ++i)
            {
                var block = filteredOptions[i];
                if (
                    (block.attachedTop == null || !dungeon.HasBlockAt(pos + Block.offsetTop)) &&
                    (block.attachedRight == null || !dungeon.HasBlockAt(pos + Block.offsetRight)) &&
                    (block.attachedBottom == null || !dungeon.HasBlockAt(pos + Block.offsetBottom)) &&
                    (block.attachedLeft == null || !dungeon.HasBlockAt(pos + Block.offsetLeft))
                )
                {
                    validOptions.Add(block);
                }
            }
        }

        public delegate bool AddBlock(Block block);

        private GameObject AttachBlock(Transform parent, Vector2Int offset, List<Block> blocks, AddBlock filter)
        {
            Filter(blocks, filter);
            AddValidOptions(parent.position, offset);
            return Instantiate(validOptions[Random.Range(0, validOptions.Count)].gameObject, parent);
        }

        private void Filter(List<Block> blocks, AddBlock addBlock)
        {
            filteredOptions.Clear();
            for (int i = 0, j = blocks.Count; i != j; ++i)
            {
                var block = blocks[i];
                if (addBlock(block))
                {
                    filteredOptions.Add(block);
                }
            }
        }

        public static bool HasExits(Block block)
        {
            return block.ExitCount > 1;
        }

        public static bool HasNoExits(Block block)
        {
            return block.ExitCount == 1 && !block.levelExit;
        }

        public static bool IsLevelExit(Block block)
        {
            return block.levelExit;
        }

        private AddBlock GetFilter()
        {
            if (blockCount == maxBlocks)
            {
                ++blockCount;
                return IsLevelExit;
            }
            if (blockCount++ < maxBlocks)
            {
                return HasExits;
            }
            return HasNoExits;
        }

        private static void MoveBy(GameObject go, GameObject from, Vector3 offset)
        {
            go.transform.position = from.transform.position + offset;
        }

        public GameObject AttachTop(Block block, Transform parent)
        {
            var go = block.gameObject;
            var attached = AttachBlock(parent, Block.offsetTop, bottom, GetFilter());
            var attachedBlock = attached.GetComponent<Block>();
            block.attachedTop = attachedBlock;
            attachedBlock.attachedBottom = block;
            MoveBy(attached, go, new Vector3(0.0f, 0.0f, blockSize));
            return attached;
        }

        public GameObject AttachRight(Block block, Transform parent)
        {
            var go = block.gameObject;
            var attached = AttachBlock(parent, Block.offsetRight, left, GetFilter());
            var attachedBlock = attached.GetComponent<Block>();
            block.attachedRight = attachedBlock;
            attached.GetComponent<Block>().attachedLeft = block;
            MoveBy(attached, go, new Vector3(blockSize, 0.0f, 0.0f));
            return attached;
        }

        public GameObject AttachBottom(Block block, Transform parent)
        {
            var go = block.gameObject;
            var attached = AttachBlock(parent, Block.offsetBottom, top, GetFilter());
            var attachedBlock = attached.GetComponent<Block>();
            block.attachedBottom = attachedBlock;
            attached.GetComponent<Block>().attachedTop = block;
            MoveBy(attached, go, new Vector3(0.0f, 0.0f, -blockSize));
            return attached;
        }

        public GameObject AttachLeft(Block block, Transform parent)
        {
            var go = block.gameObject;
            var attached = AttachBlock(parent, Block.offsetLeft, right, GetFilter());
            var attachedBlock = attached.GetComponent<Block>();
            block.attachedLeft = attachedBlock;
            attached.GetComponent<Block>().attachedRight = block;
            MoveBy(attached, go, new Vector3(-blockSize, 0.0f, 0.0f));
            return attached;
        }

        private static void Add(Block block, List<Block> list)
        {
            for (int i = 0, j = block.spawnSlots; i != j; ++i)
            {
                list.Add(block);
            }
        }

        private void Index(Block block, GameObject go)
        {
            go.SetActive(false);
            if (block.CanAttachTop)
            {
                Add(block, top);
            }
            if (block.CanAttachRight)
            {
                Add(block, right);
            }
            if (block.CanAttachBottom)
            {
                Add(block, bottom);
            }
            if (block.CanAttachLeft)
            {
                Add(block, left);
            }
        }

        private void GenerateRotatedDuplicate(GameObject go, int rotations)
        {
            var duplicate = Instantiate(go, transform);
            duplicate.name += rotations.ToString();
            var block = duplicate.GetComponent<Block>();
            block.Rotate(rotations);
            Index(block, duplicate);
        }

        public void Add(GameObject go)
        {
            Index(go.GetComponent<Block>(), go);
            for (int i = 0; i != 3; GenerateRotatedDuplicate(go, ++i)) ;
        }

        public void Load()
        {
            int children = transform.childCount;
            for (int i = 0; i != children; Add(transform.GetChild(i++).gameObject));
        }
    }
}