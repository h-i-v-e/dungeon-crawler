using UnityEngine;
using System.Collections.Generic;

namespace Depravity
{
    public class TownWall : MonoBehaviour
    {
        [SerializeField]
        private GameObject wallSection;

        [SerializeField]
        private float shiftOut = 0.0f;

        private void AddWall(Transform parent, float rotations, float offsetX, float offsetZ, string name)
        {
            var wall = Instantiate(wallSection, parent);
            wall.transform.SetPositionAndRotation(
                new Vector3(offsetX, 0.0f, offsetZ),
                Quaternion.Euler(0.0f, rotations * 90.0f, 0.0f)
            );
            wall.name = name;
        }

        private void AddWalls(Block block)
        {
            var dungeon = Dungeon.Instance;
            var pos = block.transform.position;
            var offset = dungeon.GetOffset(block.transform.position);
            float halfSize = 0.5f * BlockLibrary.Instance.blockSize + shiftOut;
            if (block.attachedTop == null && !dungeon.HasBlockAt(offset + Block.offsetTop))
            {
                AddWall(block.transform, 0.0f, pos.x, pos.z + halfSize, "Top wall");
            }
            if (block.attachedRight == null && !dungeon.HasBlockAt(offset + Block.offsetRight))
            {
                AddWall(block.transform, 1.0f, pos.x + halfSize, pos.z, "Right wall");
            }
            if (block.attachedBottom == null && !dungeon.HasBlockAt(offset + Block.offsetBottom))
            {
                AddWall(block.transform, 2.0f, pos.x, pos.z - halfSize, "Bottom wall");
            }
            if (block.attachedLeft == null && !dungeon.HasBlockAt(offset + Block.offsetLeft))
            {
                AddWall(block.transform, 3.0f, pos.x - halfSize, pos.z, "Left wall");
            }
        }

        private void AddWalls(ICollection<Block> blocks)
        {
            foreach (var block in blocks)
            {
                AddWalls(block);
            }
            enabled = false;
        }

        private void Awake()
        {
            Dungeon.Instance.OnBuilt += AddWalls;
        }
    }
}