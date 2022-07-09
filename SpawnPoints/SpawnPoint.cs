using System;
using UnityEngine;

namespace Depravity
{
    public abstract class SpawnPoint : MonoBehaviour
    {
        [Serializable]
        public struct Spawner : RandomActivationOption
        {
            public Monster monster;
            public int chances;

            public int GetChances()
            {
                return chances;
            }
        }

        [SerializeField]
        private Spawner[] spawners;

        protected Monster Spawn()
        {
            var go = Instantiate(RandomActivationOptions.SelectFrom(spawners).monster.gameObject);
            go.transform.SetParent(transform, false);
            var monster = go.GetComponent<Monster>();
            monster.AddToLevel();
            return monster;
        } 
    }
}