using System.Collections.Generic;
#if !UNITY_EDITOR
using UnityEngine.Scripting;
#endif
using UnityEngine;

namespace Depravity {
    public class Dungeon : MonoBehaviour
    {
        private readonly Dictionary<Vector2Int, Block> blocks = new Dictionary<Vector2Int, Block>();
        private readonly HashSet<Block> visited = new HashSet<Block>();
        private readonly Queue<Block> fringe = new Queue<Block>();

        private float nextUpdate = 0.0f, offsetScale;
        private Vector2Int lastPosition;
        private DungeonActivator dungeonActivator;

        public GameObject seamHider;
        public float updateInterval = 0.5f;
        public int seed;

        public static readonly Vector2Int nowhere = new Vector2Int(int.MaxValue, int.MaxValue);

        public delegate void BlockActiveStateChanged(Block block, bool active);
        public delegate void Built(ICollection<Block> blocks);

        public event BlockActiveStateChanged OnBlockActiveStateChanged;
        public event Built OnBuilt;

        public static Dungeon Instance { get; private set; }

        public Dungeon()
        {
            Instance = this;
        }

        private bool HasExit
        {
            get
            {
                int num = transform.childCount;
                return num > BlockLibrary.Instance.maxBlocks;
            }
        }

        private void Clear()
        {
            while (transform.childCount > 1)
            {
                var last = transform.GetChild(transform.childCount - 1).gameObject;
                last.transform.parent = null;
                Destroy(last);
            }
        }

        public bool TryGetBlock(GameObject gameObject, out Block block)
        {
            return blocks.TryGetValue(GetOffset(gameObject.transform.position), out block);
        }

        public Vector2Int GetOffset(Vector3 position)
        {
            return new Vector2Int(Mathf.RoundToInt(position.x * offsetScale), Mathf.RoundToInt(position.z * offsetScale));
        }

        public bool FindBlockAt(Vector2Int position, out Block block)
        {
            return blocks.TryGetValue(position, out block);
        }

        public bool HasBlockAt(Vector2Int position)
        {
            return blocks.ContainsKey(position);
        }

        public bool BlockActive(Vector2Int position)
        {
            if (!FindBlockAt(position, out var block))
            {
                return false;
            }
            return block.gameObject.activeInHierarchy;
        }

        public bool FindBlockAt(Vector3 position, out Block block)
        {
            return blocks.TryGetValue(GetOffset(position), out block);
        }

        private Block Add(GameObject go, Vector2Int pos)
        {

            return blocks[pos] = go.GetComponent<Block>();
        }

        private void HideSeamVert(Vector3 offset, Transform parent)
        {
            if (seamHider == null)
            {
                return;
            }
            Instantiate(seamHider, parent, true).transform.position = parent.position + offset;
        }

        private void HideSeamHoriz(Vector3 offset, Transform parent)
        {
            if (seamHider == null)
            {
                return;
            }
            var seam = Instantiate(seamHider, parent);
            var trans = seam.transform;
            trans.position = parent.position + offset;
            trans.rotation = Rotations.rot90;
        }

        private Block AttachTop(Block block, Vector2Int pos)
        {
            ++pos.y;
            if (blocks.TryGetValue(pos, out Block existing))
            {
                existing.attachedBottom = block;
                if (!existing.enterBottom)
                {
                    block.SetDeadEnd(Block.TOP);
                }
                return block.attachedTop = existing;
            }
            else
            {
                HideSeamVert(new Vector3(0.0f, 0.0f, BlockLibrary.Instance.halfBlockSize), block.gameObject.transform);
                return Add(BlockLibrary.Instance.AttachTop(block, transform), pos);
            }
        }

        private Block AttachRight(Block block, Vector2Int pos)
        {
            ++pos.x;
            if (blocks.TryGetValue(pos, out Block existing))
            {
                existing.attachedLeft = block;
                if (!existing.enterLeft)
                {
                    block.SetDeadEnd(Block.RIGHT);
                }
                return block.attachedRight = existing;
            }
            else
            {
                HideSeamHoriz(new Vector3(BlockLibrary.Instance.halfBlockSize, 0.0f, 0.0f), block.gameObject.transform);
                return Add(BlockLibrary.Instance.AttachRight(block, transform), pos);
            }
        }

        private Block AttachBottom(Block block, Vector2Int pos)
        {
            --pos.y;
            if (blocks.TryGetValue(pos, out Block existing))
            {
                existing.attachedTop = block;
                if (!existing.enterTop)
                {
                    block.SetDeadEnd(Block.BOTTOM);
                }
                return block.attachedBottom = existing;
            }
            else
            {
                HideSeamVert(new Vector3(0.0f, 0.0f, -BlockLibrary.Instance.halfBlockSize), block.gameObject.transform);
                return Add(BlockLibrary.Instance.AttachBottom(block, transform), pos);
            }
        }

        private Block AttachLeft(Block block, Vector2Int pos)
        {
            --pos.x;
            if (blocks.TryGetValue(pos, out Block existing))
            {
                existing.attachedRight = block;
                if (!existing.enterRight)
                {
                    block.SetDeadEnd(Block.LEFT);
                }
                return block.attachedLeft = existing;
            }
            else
            {
                HideSeamHoriz(new Vector3(-BlockLibrary.Instance.halfBlockSize, 0.0f, 0.0f), block.gameObject.transform);
                return Add(BlockLibrary.Instance.AttachLeft(block, transform), pos);
            }
        }

        private void Enqueue(Block block)
        {
            if (!visited.Contains(block))
            {
                visited.Add(block);
                fringe.Enqueue(block);
            }
        }

        public void SetBlockActiveState(Block block, bool active)
        {
            block.gameObject.SetActive(active);
            OnBlockActiveStateChanged?.Invoke(block, active);
        }

        private void BuildBlock()
        {
            var block = fringe.Dequeue();
            var go = block.gameObject;
            go.SetActive(false);
            var pos = GetOffset(go.transform.position);
            if (block.enterTop)
            {
                var attach = AttachTop(block, pos);
                if (attach.enterBottom)
                {
                    Enqueue(attach);
                }
            }
            if (block.enterRight)
            {
                var attach = AttachRight(block, pos);
                if (attach.enterLeft)
                {
                    Enqueue(attach);
                }
            }
            if (block.enterBottom)
            {
                var attach = AttachBottom(block, pos);
                if (attach.enterTop)
                {
                    Enqueue(attach);
                }
            }
            if (block.enterLeft)
            {
                var attach = AttachLeft(block, pos);
                if (attach.enterRight)
                {
                    Enqueue(attach);
                }
            }
        }

        public bool Ready
        {
            get
            {
                return nextUpdate != 0.0f;
            }
        }

        private void Awake()
        {
            dungeonActivator = GetComponent<DungeonActivator>();
            dungeonActivator.Dungeon = this;
            offsetScale = 1.0f / BlockLibrary.Instance.blockSize;
            var entrance = GetComponentInChildren<Block>();
            Debug.Assert(entrance != null, "Dungeon should have a single child with a Block component to act as the entrance");
            blocks[GetOffset(entrance.transform.position)] = entrance;
            lastPosition = nowhere;
        }

        public Block FindExitBlock()
        {
            foreach(var block in blocks.Values)
            {
                if (block.levelExit)
                {
                    return block;
                }
            }
            return null;
        }

        private void DecorateBlocks()
        {
            foreach (var block in blocks.Values)
            {
                block.Decorate();
            }
        }

        private void Start()
        {
            Random.InitState(seed);
            var entrance = GetComponentInChildren<Block>();
            if (entrance.ExitCount == 0)
            {
                entrance.gameObject.SetActive(true);
                return;
            }
            BlockLibrary.Instance.Load();
            do
            {
                Clear();
                visited.Clear();
                Enqueue(entrance);
                while (fringe.Count > 0)
                {
                    BuildBlock();
                }
                if (HasExit)
                {
                    entrance.gameObject.SetActive(true);
                }
            }
            while (!HasExit);
            DecorateBlocks();
            OnBuilt?.Invoke(blocks.Values);
            DestroyImmediate(BlockLibrary.Instance.gameObject);
#if !UNITY_EDITOR
            GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
            System.GC.Collect();
            GarbageCollector.GCMode = GarbageCollector.Mode.Disabled;
#endif
        }

        private void Update()
        {
            float currentTime = Time.timeSinceLevelLoad;
            if (currentTime < nextUpdate)
            {
                return;
            }
            nextUpdate = currentTime + updateInterval;
            var pos = GetOffset(Controller.Player.transform.position);
            if (pos == lastPosition)
            {
                return;
            }
            lastPosition = pos;
            if (blocks.TryGetValue(pos, out Block block))
            {
                dungeonActivator.ActivateReachableFrom(block);
            }
        }
    }
}