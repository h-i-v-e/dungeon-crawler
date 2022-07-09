using UnityEngine;

namespace Depravity
{
    public class FirstActivationOnlySpawnPoint : SpawnPoint
    {
        [SerializeField, Range(0.0f, 1.0f)]
        private float probability = 1.0f;

        [SerializeField]
        private int minSpawned = 0, maxSpawned = 4;

        private int ChooseNumberToSpawn()
        {
            if (probability == 1.0f || Random.value <= probability)
            {
                return Random.Range(minSpawned, maxSpawned);
            }
            return 0;
        }

        private Quaternion CalculateFacing()
        {
            return Quaternion.LookRotation(Controller.Player.transform.position - transform.position, Vector3.up);
        }

        private void Spawn(Vector3 offsetDirection, Quaternion facing)
        {
            var monster = Spawn();
            monster.transform.rotation = facing;
            monster.transform.localPosition = offsetDirection * monster.radius * Random.Range(2.0f, 4.0f);
        }

        private void Spawn(int number)
        {
            var rot = Quaternion.Euler(0.0f, 360.0f / number, 0.0f);
            var dir = Vector3.forward;
            var facing = CalculateFacing();
            Spawn(dir, facing);
            for (int i = 1; i < number; ++i)
            {
                dir = rot * dir;
                Spawn(dir, facing);
            }
        }

        private void Start()
        {
            int num = ChooseNumberToSpawn();
            Debug.Log("Spawning " + num);
            switch (num)
            {
                case 0:
                    break;
                case 1:
                    Spawn();
                    break;
                default:
                    Spawn(num);
                    break;
            }
            enabled = false;
        }
    }
}
