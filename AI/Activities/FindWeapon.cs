using UnityEngine;

namespace Depravity
{
    public class FindWeapon : Activity
    {
        private Weapon closestWeapon;
        private float distance;

        private void EquipWeapon(Weapon weapon)
        {
            weapon.AddToRightHandOf(activityManager.Monster);
            activityManager.Pop();
        }

        private void CheckBlock(Block block)
        {
            if (!block.gameObject.activeInHierarchy)
            {
                return;
            }
            var pos = transform.position;
            var weapons = block.GetComponentsInChildren<Weapon>(false);
            for (int i = 0, j = weapons.Length; i != j; ++i)
            {
                var weapon = weapons[i];
                if (weapon.Wielder == null)
                {
                    float distance = (pos - weapon.transform.position).sqrMagnitude;
                    if (distance < this.distance)
                    {
                        closestWeapon = weapon;
                        this.distance = distance;
                    }
                }
            }
        }

        public void Hit()
        {
            if (enabled) {
                EquipWeapon(closestWeapon);
            }
        }

        private void ReachedTarget()
        {
            if (closestWeapon.Wielder != null)
            {
                //already picked up
                FindNearbyWeapon();
            }
            else
            {
                activityManager.Monster.AnimationManager.PickUpRight();
            }
        }

        private void FindNearbyWeapon()
        {
            var dungeon = Dungeon.Instance;
            if (!dungeon.FindBlockAt(dungeon.GetOffset(transform.position), out var block))
            {
                return;
            }
            closestWeapon = null;
            distance = float.MaxValue;
            CheckBlock(block);
            block.WithEachNeighbour(CheckBlock);
            if (closestWeapon == null)
            {
                //no weapons, better run
                activityManager.Push<Flee>().Activate();
            }
            else
            {
                var nav = activityManager.Monster.BlockNavigator;
                nav.OnReachedTarget = ReachedTarget;
                nav.MoveTo(closestWeapon.transform.position);
            }
        }

        public override void Activate()
        {
            FindNearbyWeapon();
        }
    }
}
