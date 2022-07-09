using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Depravity
{
    [RequireComponent(
        typeof(Animator),
        typeof(Rigidbody),
        typeof(Collider)
    ),
    RequireComponent(
        typeof(MonsterAnimationManager)
    )]
    public class Monster : MonoBehaviour, LookAtTarget
    {
        private const float WOUNDED_BLEND_SPEED = 2.0f;

        private static readonly List<Holdable> holdableList = new List<Holdable>(2);
        private static readonly List<Collider> colliderList = new List<Collider>();
        private static readonly List<Rigidbody> rigidbodiesList = new List<Rigidbody>();

        internal readonly LinkedListNode<Monster> locationNode;

        public delegate void BlockStateChanged(Monster monster, bool blockActive);
        public delegate void TookDamage(Monster monster, float amount);
        public delegate void Died(Monster monster);
        public delegate void UnderAttack(Monster victim, Monster attacker);

        public event BlockStateChanged OnBlockStateChanged;
        public event TookDamage OnTookDamage;
        public event Died OnDied, OnRevived;
        public event UnderAttack OnAttacked;

        private float woundAnimationProgress;
        private bool blendingInWounded = false;

        [Header("Attributes")]
        public float speed = 1.0f;
        public float fullHitPoints = 10.0f, currentHitPoints = 10.0f, radius = 0.35f;

        [SerializeField]
        private DamageProfile damageProfile;

        public TeamProfile teamProfile;

        [Header("Equipment")]
        public Armour armour;
        public Weapon weapon;
        public Shield shield;

        [Header("Special Effects")]
        [SerializeField]
        private ParticleSystem particles;

        [SerializeField]
        private AudioSource damageAudioSource;

        [SerializeField, Header("Attachment Points")]
        private Transform leftHand;
        
        [SerializeField]
        private Transform rightHand;

        [SerializeField]
        private Transform eyePosition;

        public Vector3 GetPosition()
        {
            return eyePosition.position;
        }

        public bool GoneRogue
        {
            get; set;
        }

        public bool IsFriendly(Monster other)
        {
            if (teamProfile == null || other.teamProfile == null || other.GoneRogue)
            {
                return false;
            }
            string name = other.teamProfile.teamName;
            if (name == teamProfile.teamName)
            {
                return true;
            }
            for (int i = 0, j = teamProfile.alliedTeams.Length; i != j; ++i)
            {
                if (name == teamProfile.alliedTeams[i])
                {
                    return true;
                }
            }
            return false;
        }

        public Vector2Int BlockOffset { get; internal set; }

        public float Height { get; private set; }

        public Collider Collider { get; private set; }

        public MonsterAnimationManager AnimationManager{ get; private set; }

        public BlockNavigator BlockNavigator { get; private set; }

        public Monster()
        {
            locationNode = new LinkedListNode<Monster>(this);
        }

        public bool IsDead
        {
            get
            {
                return currentHitPoints <= 0.0f;
            }
        }

        public void Collapse()
        {
            OnDied?.Invoke(this);
            AnimationManager.Animator.enabled = false;
            SetIKEnabled(false);
        }

        public void Revive()
        {
            damageAudioSource.clip = null;
            currentHitPoints = fullHitPoints;
            OnRevived?.Invoke(this);
            SetIKEnabled(true);
            AnimationManager.Animator.enabled = true;
        }

        #region Animation callbacks
        public void Shoot()
        {
            weapon.Shoot();
        }

        public void Hit()
        {
            if (weapon != null && weapon is HandToHandWeapon h2h)
            {
                h2h.Swish();
            }
        }
        #endregion

        private void UpdateWounded(float amount)
        {
            AnimationManager.Wounded = 1 - (amount / fullHitPoints);
        }

        public Transform LeftHand
        {
            get
            {
                return leftHand;
            }
        }

        public Transform RightHand
        {
            get
            {
                return rightHand;
            }
        }

        private IEnumerator BlendWounded()
        {
            blendingInWounded = true;
            for (; ; )
            {
                woundAnimationProgress -= Time.deltaTime * WOUNDED_BLEND_SPEED;
                if (woundAnimationProgress < currentHitPoints)
                {
                    UpdateWounded(currentHitPoints);
                    blendingInWounded = false;
                    yield break;
                }
                UpdateWounded(woundAnimationProgress);
                yield return null;
            }
        }

        private void PlayWoundAnimation(Vector3 direction)
        {
            float forward = Vector3.Dot(transform.forward, direction);
            if (forward < -0.5f)
            {
                AnimationManager.HitFront();
            }
            else if (forward > 0.5f)
            {
                AnimationManager.HitBack();
            }
            else if (Vector3.Dot(transform.right, direction) > 0.0f)
            {
                AnimationManager.HitRight();
            }
            else
            {
                AnimationManager.HitLeft();
            }
        }

        private IEnumerator Splatter(Vector3 impactPoint, Vector3 direction)
        {
            particles.transform.SetPositionAndRotation(impactPoint, Quaternion.LookRotation(-direction.normalized));
            particles.Play();
            do
            {
                yield return null;
            }
            while (particles.isPlaying);
        }

        private static void PlayClip(AudioSource audioSource, AudioClip clip)
        {
            if (audioSource != null && clip != null && !audioSource.isPlaying)
            {
                audioSource.clip = clip;
                audioSource.Play();
            }
        }

        public void NotifyUnderAttack(Monster attacker)
        {
            OnAttacked?.Invoke(this, attacker);
        }

        public bool TakeDamage(Weapon.DamageType type, Vector3 impactPoint, Vector3 direction, float amount)
        {
            Debug.Log("Taking Damage");
            //have them get knocked even if the armour saves them
            PlayWoundAnimation(direction);
            if (armour != null)
            {
                amount = armour.Absorb(type, amount, out var ding);
                if (amount == 0.0f)
                {
                    PlayClip(damageAudioSource, ding);
                    return false;
                }
            }
            if (particles != null)
            {
                StartCoroutine(Splatter(impactPoint, direction));
            }
            amount = damageProfile.AdjustDamageAndGetSoundEffect(type, amount, out var splat);
            PlayClip(damageAudioSource, splat);
            currentHitPoints -= amount;
            if (currentHitPoints <= 0.0f)
            {
                Collapse();
            }
            else
            {
                if (!blendingInWounded)
                {
                    StartCoroutine(BlendWounded());
                }
                OnTookDamage?.Invoke(this, amount);
            }
            return true;
        }

        public float BlockTraversalRate { get; private set; }

        private static void SetChildCollidersEnabled(GameObject parent, bool enabled)
        {
            parent.GetComponentsInChildren(colliderList);
            for (int i = 0, j = colliderList.Count; i != j; ++i)
            {
                var collider = colliderList[i];
                collider.enabled = collider.isTrigger ? !enabled : enabled;
            }
        }

        private void EnableRagDole(bool enabled)
        {
            int originalLayer = gameObject.layer;
            int layer = enabled ? 0 : Controller.DamageLayer;
            gameObject.GetComponentsInChildren(colliderList);
            for (int i = 0, j = colliderList.Count; i != j; colliderList[i++].gameObject.layer = layer);
            gameObject.layer = originalLayer;
        }

        private static void EnableHoldableColliders(Holdable holdable, bool enabled)
        {
            var go = holdable.gameObject;
            SetChildCollidersEnabled(go, enabled);
            go.layer = enabled ? 0 : holdable.GetPhysicsLayer();
            /*if (enabled)
            {
                go.layer = 0;
                return;
            }
            else
            if (holdable is Weapon)
            {
                go.layer = enabled ? 0 : Controller.WeaponLayer;
            }*/
        }

        public void SetIKEnabled(bool enabled)
        {
            gameObject.GetComponentsInChildren(rigidbodiesList);
            for (int i = 0, j = rigidbodiesList.Count; i != j; rigidbodiesList[i++].isKinematic = enabled) ;
            gameObject.GetComponent<Rigidbody>().isKinematic = !enabled;
            EnableRagDole(!enabled);
            gameObject.GetComponent<Collider>().enabled = enabled;
            gameObject.GetComponentsInChildren(holdableList);
            for (int i = 0, j = holdableList.Count; i != j; EnableHoldableColliders(holdableList[i++], !enabled)) ;
        }

        private void Awake()
        {
            Collider = GetComponent<Collider>();
            AnimationManager = GetComponent<MonsterAnimationManager>();
            BlockNavigator = GetComponent<BlockNavigator>();
            if (BlockNavigator != null)
            {
                BlockNavigator.monster = this;
            }
            Height = Collider.bounds.size.y;
        }

        /*private void OnEnable()
        {
            if (weapon != null)
            {
                AnimationManager.SetArmedWith(weapon.animationType);
            }
        }*/

        public void AddToLevel()
        {
            //SetIKEnabled(true);
            Debug.Log("Reviving");
            Revive();
            BlockTraversalRate = BlockLibrary.Instance.blockSize / speed;
            BlockOffset = new Vector2Int(int.MaxValue, int.MaxValue);
            MonsterManager.Instance.AddMonster(this);
            if (weapon != null)
            {
                if (weapon.Held)
                {
                    weapon.Bind();
                }
                else if (weapon.GetHand() == Holdable.Hand.RIGHT)
                {
                    weapon.AddToRightHandOf(this);
                }
                else
                {
                    weapon.AddToLeftHandOf(this);
                }
            }
            if (shield != null && !shield.Held)
            {
                shield.AddToLeftHandOf(this);
            }
        }

        private void Start()
        {
            gameObject.layer = Controller.MonsterLayer;
            //AddToLevel();
            if (weapon != null)
            {
                AnimationManager.SetArmedWith(weapon.GetAnimationType());
            }
            woundAnimationProgress = currentHitPoints;
        }

        internal void UpdateBlockState(bool blockActive)
        {
            gameObject.SetActive(blockActive);
            OnBlockStateChanged?.Invoke(this, blockActive);
        }

        private void Update()
        {
            MonsterManager.Instance.UpdateMonsterPosition(this);
        }

        private static Holdable GetHoldable(Transform hand)
        {
            return hand.childCount == 0 ? null : hand.GetChild(0).GetComponent<Holdable>();
        }

        internal void ClearHoldable(Holdable item)
        {
            if (item is Weapon)
            {
                weapon = null;
            }
            else if (item is Shield)
            {
                shield = null;
            }
        }

        private void AddToHand(Transform hand, Holdable item)
        {
            var holding = GetHoldable(hand);
            if (item == null)
            {
                holding.Drop();
                return;
            }
            if (holding != null)
            {
                if (holding == item)
                {
                    return;
                }
                holding.Drop();
            }
            item.gameObject.layer = item.GetPhysicsLayer();
            var trans = item.transform;
            trans.SetParent(hand, false);
            trans.localPosition = Vector3.zero;
            trans.localRotation = Quaternion.identity;
            if (item is Weapon weapon)
            {
                this.weapon = weapon;
            }
            else if (item is Shield shield)
            {
                this.shield = shield;
            }
        }

        public Holdable InLeftHand
        {
            set
            {
                AddToHand(leftHand, value);
            }
            get
            {
                return GetHoldable(leftHand);
            }
        }

        public Holdable InRightHand
        {
            set
            {
                AddToHand(rightHand, value);
            }
            get
            {
                return GetHoldable(rightHand);
            }
        }
    }
}
