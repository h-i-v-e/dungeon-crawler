using UnityEngine;

namespace Depravity
{
    public class CharacterSetup : MonoBehaviour
    {
        [System.Serializable]
        public struct Outfit
        {
            public string name;
            public bool showHair;
            public ArmourProfile armour;
            public GameObject[] parts;
        }

        public Outfit[] outfits;

        [SerializeField]
        private GameObject head, hair, eyebrows;

        [SerializeField]
        private Transform partTreeRoot;

        private int selected = 0;
        private Armour armour;

        private static void HideAllChildren(Transform transform)
        {
            for (int i = 0, j = transform.childCount; i != j; HideAllChildren(transform.GetChild(i++))) ;
            transform.gameObject.SetActive(false);
        }

        private static void Activate(GameObject go)
        {
            do
            {
                go.SetActive(true);
                go = go.transform.parent.gameObject;
            } while (go != null && !go.activeSelf);
        }

        private static void Activate(GameObject[] parts)
        {
            for (int i = 0, j = parts.Length; i != j; Activate(parts[i++])) ;
        }

        private void SetArmour(ArmourProfile profile)
        {
            var monster = GetComponent<Monster>();
            if (profile == null)
            {
                monster.armour = null;
                return;
            }
            monster.armour = armour;
            armour.profile = profile;
        }

        private void Activate(Outfit outfit)
        {
            HideAllChildren(partTreeRoot);
            Activate(head);
            if (outfit.showHair)
            {
                Activate(hair);
            }
            Activate(eyebrows);
            Activate(outfit.parts);
            SetArmour(outfit.armour);
        }

        public int Selected
        {
            get
            {
                return selected;
            }
            set
            {
                Activate(outfits[selected = value]);
            }
        }

        private void Awake()
        {
            armour = gameObject.AddComponent<Armour>();
        }

        private void Start()
        {
            Activate(outfits[selected]);
        }
    }
}