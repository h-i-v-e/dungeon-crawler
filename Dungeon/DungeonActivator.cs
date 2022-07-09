using UnityEngine;

namespace Depravity
{
    public abstract class DungeonActivator : MonoBehaviour
    {
        public Dungeon Dungeon
        {
            get; internal set;
        }

        public abstract void ActivateReachableFrom(Block block);
    }
}