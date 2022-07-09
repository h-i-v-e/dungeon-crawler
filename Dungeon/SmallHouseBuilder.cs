using UnityEngine;

namespace Depravity
{
    public class SmallHouseBuilder : MonoBehaviour
    {
        [System.Serializable]
        public struct PartGroup
        {
            public GameObject leftExtendable, rightExtendable, doubleExtendable, notExtendable;
        }

        [Header("Core")]
        [SerializeField]
        private PartGroup tops;

        [SerializeField]
        private PartGroup bottoms;

        [Header("Extensions")]
        [SerializeField]
        private GameObject singleStorey;

        [SerializeField]
        private GameObject doubleStory;

        private void Swivel(GameObject go)
        {
            go.transform.RotateAround(transform.position, Vector3.up, 180.0f);
        }

        private GameObject Spawn(GameObject parent)
        {
            return Instantiate(parent, transform);
        }

        private void ExtendLeft()
        {
            Spawn(bottoms.leftExtendable);
            if (Random.value < 0.5f)
            {
                Spawn(tops.leftExtendable);
                Swivel(Spawn(doubleStory));
            }
            else
            {
                Spawn(tops.notExtendable);
                Swivel(Spawn(singleStorey));
            }
        }

        private void ExtendRight()
        {
            Spawn(bottoms.rightExtendable);
            if (Random.value < 0.5f)
            {
                Spawn(tops.rightExtendable);
                Spawn(doubleStory);
            }
            else
            {
                Spawn(tops.notExtendable);
                Spawn(singleStorey);
            }
        }

        private void ExtendBoth()
        {
            Spawn(bottoms.doubleExtendable);
            switch (Random.Range(0, 4))
            {
                case 0:
                    Spawn(tops.leftExtendable);
                    Swivel(Spawn(doubleStory));
                    Spawn(singleStorey);
                    return;
                case 1:
                    Spawn(tops.rightExtendable);
                    Spawn(doubleStory);
                    Swivel(Spawn(singleStorey));
                    return;
                case 2:
                    Spawn(tops.doubleExtendable);
                    Swivel(Spawn(doubleStory));
                    Spawn(doubleStory);
                    return;
                case 3:
                    Spawn(tops.notExtendable);
                    Swivel(Spawn(singleStorey));
                    Spawn(singleStorey);
                    return;
            }
        }


        private void ExtendNiether()
        {
            Spawn(bottoms.notExtendable);
            Spawn(tops.notExtendable);
        }

        private void Start()
        {
            switch (Random.Range(0, 4))
            {
                case 0:
                    ExtendLeft();
                    return;
                case 1:
                    ExtendRight();
                    return;
                case 2:
                    ExtendBoth();
                    return;
                case 3:
                    ExtendNiether();
                    return;
            }
        }
    }
}
