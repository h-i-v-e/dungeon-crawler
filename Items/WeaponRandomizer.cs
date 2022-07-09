using UnityEngine;

namespace Depravity
{
    [RequireComponent(typeof(Monster))]
    public class WeaponRandomizer : MonoBehaviour
    {
        [SerializeField]
        private WeaponOptionsProfile profile;

        private Holdable CloneHoldable(Holdable thing)
        {
            return Instantiate(thing.gameObject).GetComponent<Holdable>();
        }
        
        private void Start()
        {
            var opt = RandomActivationOptions.SelectFrom(profile.options);
            var monster = GetComponent<Monster>();
            if (opt.weapon != null)
            {
                CloneHoldable(opt.weapon).AddToRightHandOf(monster);
            }
            if (opt.shield != null)
            {
                CloneHoldable(opt.shield).AddToLeftHandOf(monster);
            }
        }
    }
}
