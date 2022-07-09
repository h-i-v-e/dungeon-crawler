#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections.Generic;
using UnityEngine;

namespace Depravity
{

    internal class PositionWithTolerance : LookAtTarget
    {
        public Vector3 position;
        public float tolerance;

        public Vector3 GetPosition()
        {
            return position;
        }
    }

    [RequireComponent(
        typeof(Rigidbody),
        typeof(Monster),
        typeof(MonsterAnimationManager)
    )]
    public class BlockNavigator : MonoBehaviour
    {
        public enum State
        {
            STATIONARY, FORCE_ROTATE_LEFT, FORCE_ROTATE_RIGHT,
            BACKING_UP, TRYING_TO_STOP, MOVING, FORCE_ROTATE_TO_FACE,
            ROTATING
        }

        private const float ALLOWED_DISTANCE_ERROR = 0.01f, STOP_FOR_TURNS_OVER_COS = 0.75f;

        private static PositionWithTolerance MakePositionWithTolerance()
        {
            return new PositionWithTolerance();
        }

        private static readonly ResourcePool<PositionWithTolerance> targetPool = new ResourcePool<PositionWithTolerance>(MakePositionWithTolerance);

        private readonly List<PositionWithTolerance> targets = new List<PositionWithTolerance>();

        private float nearEnough, animationScale,
            eyeLevel, lastTurn, currentSpeed, stopAt, minDistance;

        private bool reachedTargetTriggered = false, forcedRotationApplied = false; 

        public int debugStackSize = 0;

        internal Monster monster;

        private GameObject follow;
        private Vector2Int currentTarget = Dungeon.nowhere;
        private PositionWithTolerance nextWaypoint;
        //private Vector3 backupTarget;
        private Quaternion forcedRotation;
        private Vector3 targetRotation;

        public float backwardSpeed = 1.0f, acceleration = 2.0f, stationaryRotationBoostDeg = 180.0f;

        public string thinking = "", lastObsticle = "", state = "";

        public delegate void ReachedTarget();

        public ReachedTarget OnReachedTarget;

        public State CurrentState { get; private set; }

        public float TimeToCoverDistance(float distance, float maxSpeed)
        {
            float timeToFullSpeed = maxSpeed / acceleration;
            float coveredWhileAdjustingSpeed = timeToFullSpeed * maxSpeed * 0.5f;
            if (coveredWhileAdjustingSpeed > distance)
            {
                return distance * 2.0f / acceleration;
            }
            else
            {
                float remaining = distance - coveredWhileAdjustingSpeed;
                return timeToFullSpeed + remaining / maxSpeed;
            }
        }

        private static bool NearEnough(Vector3 diff, float tolerance)
        {
            return (new Vector2(diff.x, diff.z)).sqrMagnitude <= tolerance * tolerance;
        }

        private bool ClearPathToFollow(Vector3 pos, Vector3 target)
        {
            //Debug.Assert(pos.y > 0.1f && target.y > 0.1f, "Too low, will hit floor " + pos.y + ", " + target.y);
            var dir = target - pos;
            var mag = dir.magnitude;
            if (Physics.Raycast(pos, dir, out var hit, mag, 1) && hit.collider.gameObject != follow)
            {
                lastObsticle = hit.collider.gameObject.name;
                return false;
            }
            var across = Vector3.Cross(dir.normalized, Vector3.up).normalized * monster.radius;
            //Will we bang into anything on our left side
            if (Physics.Raycast(pos + across, dir, out hit, mag, 1) && hit.collider.gameObject != follow)
            {
                lastObsticle = hit.collider.gameObject.name;
                return false;
            }
            //Will we bang into anything on our right side
            if (Physics.Raycast(pos - across, dir, out hit, mag, 1) && hit.collider.gameObject != follow)
            {
                lastObsticle = hit.collider.gameObject.name;
                return false;
            }
            return true;
        }

        private void Awake()
        {
            SetState(State.STATIONARY);
        }

        private Vector3 Lifted(Vector3 pos)
        {
            return new Vector3(pos.x, pos.y + eyeLevel, pos.z);
        }

        private static PositionWithTolerance AllocateWaypoint(Vector3 position, float tolerance)
        {
            var waypoint = targetPool.Allocate();
            waypoint.position = position;
            waypoint.tolerance = tolerance;
            return waypoint;
        }

        private void NavigateToAnotherBlock(Vector2Int myOffset, Vector2Int targetOffset)
        {
            if (DungeonNavigator.FindPath(myOffset, targetOffset, out var path))
            {
                for (int i = 0, j = path.Count; i != j; targets.Add(AllocateWaypoint(path[i++], monster.radius))) ;
                DungeonNavigator.ReleasePath(path);
            }
        }

        public void SetForcedRotation(Vector3 direction)
        {
            SetState(State.FORCE_ROTATE_TO_FACE);
            direction.y = 0.0f;
            if (Mathf.Approximately(direction.x, 0.0f) && Mathf.Approximately(direction.z, 0.0f))
            {
                forcedRotationApplied = true;
                return;
            }
            direction.Normalize();
            if (Mathf.Approximately(direction.x, 0.0f) && Mathf.Approximately(direction.z, 0.0f))
            {
                forcedRotationApplied = true;
                return;
            }
            forcedRotation = Quaternion.LookRotation(direction);
            forcedRotationApplied = false;
        }

        public LookAtTarget ForceRotateToFace
        {
            set
            {
                //SetForcedRotation(value.GetPosition() - transform.position);
                monster.AnimationManager.LookingAt = value;
                Rotate(value.GetPosition() - transform.position);
            }
        }

        public void Rotate(Vector3 direction)
        {
            targetRotation = direction;
            SetState(State.ROTATING);
            reachedTargetTriggered = false;
        }

        public void Rotate(Quaternion quaternion)
        {
            Rotate(quaternion * Vector3.forward);
        }

        private void Check(string message)
        {
            if (gameObject == Controller.Player.gameObject)
            {
                return;
            }
            Debug.Assert(!(nextWaypoint.position.x == 0.0f && nextWaypoint.position.y == 0.0f), message);
        }

        private static Vector2 Vec32Vec2(Vector3 vec3)
        {
            return new Vector2(vec3.x, vec3.z);
        }

        private Vector3 Vec22Vec3(Vector2 vec2)
        {
            return new Vector3(vec2.x, transform.position.y + eyeLevel, vec2.y);
        }

        private void NavigateAroundObsticles(Vector3 target)
        {
            var path = ObsticleNavigator.FindPath(Vec32Vec2(transform.position), Vec32Vec2(target), monster.radius * 1.1f).GetWaypoints();
            for (int i = 0, j = path.Count; i != j; ++i)
            {
                targets.Add(AllocateWaypoint(Vec22Vec3(path[i].pos), monster.radius));
            }
            if (targets.Count == 0)
            {
                SetState(State.STATIONARY);
            }
            else
            {
                nextWaypoint = targets[targets.Count - 1];
                targets.RemoveAt(targets.Count - 1);
            }
        }

        private void SetNextWaypoint(Vector3 pos, float tolerance)
        {
            if (nextWaypoint == null)
            {
                nextWaypoint = targetPool.Allocate();
            }
            nextWaypoint.position = pos;
            nextWaypoint.tolerance = tolerance;
        }

        private void PopWaypoint()
        {
            int last = targets.Count - 1;
            var output = targets[last];
            targets.RemoveAt(last);
            if (nextWaypoint != null)
            {
                targetPool.Release(nextWaypoint);
            }
            nextWaypoint = output;
        }

        private void ResolveBlocked(Vector3 pos, Vector3 value, float tolerance)
        {
            targets.Clear();
            Dungeon dungeon = Dungeon.Instance;
            var targetOffset = dungeon.GetOffset(pos);
            var myOffset = dungeon.GetOffset(pos);
            if (myOffset == targetOffset)
            {
                thinking = "Navigating around obsticles";
                NavigateAroundObsticles(new Vector3(value.x, 0.0f, value.z));
            }
            else if (currentTarget != targetOffset || targets.Count == 0)
            {
                currentTarget = targetOffset;
                targets.Add(AllocateWaypoint(new Vector3(value.x, 0.0f, value.z), tolerance));
                NavigateToAnotherBlock(myOffset, targetOffset);
                PopWaypoint();
                if (!ClearPathToFollow(pos, Lifted(nextWaypoint.position)))
                {
                    NavigateAroundObsticles(nextWaypoint.position);
                }
                thinking = "Going for a wonder";
                Check("@Wander");
            }
            debugStackSize = targets.Count;
        }

        private void SetTargetPosition(Vector3 value, float tolerance)
        {
            var pos = Lifted(transform.position);
            if (ClearPathToFollow(pos, Lifted(value)))
            {
                targets.Clear();
                debugStackSize = 0;
                thinking = "Clear path";
                SetNextWaypoint(value, tolerance);//target;
                return;
            }
            else if (targets.Count != 0)
            {
                thinking = "Already got path";
                return;
            }
            ResolveBlocked(pos, value, tolerance);
        }

        /*public float Backup
        {
            set
            {
                backupTarget = transform.position - transform.forward * (value + monster.radius);
                SetState(State.BACKING_UP);
                monster.AnimationManager.Animator.applyRootMotion = true;
            }
        }*/

        public float MoveTo(Vector3 pos, float nearEnough = 0.0f)
        {
            targets.Clear();
            reachedTargetTriggered = false;
            currentTarget = Dungeon.nowhere;
            follow = null;
            SetState(State.MOVING);
            float dist = monster.radius + nearEnough;
            SetTargetPosition(pos, dist);
            return dist;
        }

        public float Follow(Monster value, float atDistance = 0.0f, float minDistance = 0.0f)
        {
            if (value != null)
            {
                follow = value.gameObject;
                targets.Clear();
                currentTarget = Dungeon.nowhere;
                float dist = monster.radius + value.radius;
                this.minDistance = dist + minDistance;
                nearEnough = dist + atDistance;
                UpdateFollow();
                return nearEnough + ALLOWED_DISTANCE_ERROR;
            }
            else
            {
                follow = null;
                monster.AnimationManager.LookingAt = null;
            }
            return 0.0f;
        }

        private float ComputeStoppingDistance()
        {
            float stoppingTime = currentSpeed / acceleration;
            return stoppingTime * currentSpeed * 0.5f; 
        }

        private void KickedTheBucket(Monster monster)
        {
            enabled = false;
        }

        private void Revived(Monster monster)
        {
            enabled = true;
        }

        private void Start()
        {
            CurrentState = State.STATIONARY;
            monster.OnBlockStateChanged += BlockStateChanged;
            monster.OnDied += KickedTheBucket;
            monster.OnRevived += Revived;
            var extents = monster.radius;
            Debug.Log(monster.Height);
            eyeLevel = monster.Height * 0.95f;
            animationScale = 1.0f / monster.Height;
        }

        private void UpdateFollow()
        {
            var targetPos = follow.transform.position;
            var dir = targetPos - transform.position;
            if (NearEnough(dir, CurrentState == State.TRYING_TO_STOP ? stopAt : nearEnough))
            {
                if (dir.sqrMagnitude < minDistance * minDistance)
                {
                    reachedTargetTriggered = false;
                    SetState(State.BACKING_UP);
                    SetNextWaypoint(Lifted(targetPos) - dir.normalized * (minDistance + nearEnough), nearEnough);
                }
                else
                {
                    thinking = "near enough";
                    CurrentState = State.STATIONARY;
                    follow = null;
                }
                return;
            }
            reachedTargetTriggered = false;
            if (CurrentState == State.STATIONARY)
            {
                CurrentState = State.MOVING;
            }
            SetTargetPosition(Lifted(targetPos), nearEnough);
        }

        private void BlockStateChanged(Monster monster, bool state)
        {
            currentTarget = Dungeon.nowhere;
        }

#if UNITY_EDITOR
        private void DrawTarget(Color colour, Vector3 from, Vector3 target)
        {
            Gizmos.color = colour;
            Gizmos.DrawCube(target, new Vector3(0.25f, 0.25f, 0.25f));
            Gizmos.DrawRay(from, target - from);
        }

        private void OnDrawGizmos()
        {
            if (!Selection.Contains(gameObject))
            {
                return;
            }
            if (CurrentState != State.STATIONARY)
            {
                var to = nextWaypoint.position;
                DrawTarget(Color.magenta, transform.position, to);
                for (int i = targets.Count - 1; i >= 0; --i)
                {
                    DrawTarget(Color.magenta, to, targets[i].position);
                    to = targets[i].position;
                }
            }
            if (follow != null)
            {
                DrawTarget(Color.green, transform.position, follow.transform.position);
            }
        }
#endif

        private void Update()
        {
            if (follow != null)
            {
                UpdateFollow();
            }
            switch (CurrentState)
            {
                case State.STATIONARY:
                    if (!reachedTargetTriggered)
                    {
                        reachedTargetTriggered = true;
                        OnReachedTarget?.Invoke();
                    }
                    return;
                case State.BACKING_UP:
                    return;
                default:
                    if (nextWaypoint == null)
                    {
                        Stop(transform.forward, false);
                        return;
                    }
                    if (!ClearPathToFollow(Lifted(transform.position), Lifted(nextWaypoint.position)))
                    {
                        ResolveBlocked(Lifted(transform.position), nextWaypoint.position, monster.radius);
                    }
                    return;

            }
        }

        private Vector3 PopWaypoint(Vector3 pos)
        {
            PopWaypoint();
            var dir = nextWaypoint.position - pos;
            dir.y = 0;
            return dir;
        }

        private void AdjustVelocity(float desired, string from)
        {
            if (desired < currentSpeed)
            {
                currentSpeed -= acceleration * Time.fixedDeltaTime;
                if (currentSpeed < desired)
                {
                    currentSpeed = desired;
                }
            }
            else if (desired > currentSpeed)
            {
                currentSpeed += acceleration * Time.fixedDeltaTime;
                if (currentSpeed > desired)
                {
                    currentSpeed = desired;
                }
            }
            Debug.Assert(desired > 0.0001f || CurrentState != State.MOVING, from);
        }

        private bool Blocked(Vector3 pos, Vector3 direction)
        {
            return Physics.Raycast(pos, direction, out RaycastHit hit, monster.radius + ComputeStoppingDistance(), 1) &&
                hit.collider.gameObject != follow;
        }

        private void SetState(State newState)
        {
            if (CurrentState == newState)
            {
                return;
            }
            monster.AnimationManager.Animator.applyRootMotion = newState != State.FORCE_ROTATE_LEFT && newState != State.FORCE_ROTATE_RIGHT && newState != State.FORCE_ROTATE_TO_FACE;
            CurrentState = newState;
            state = CurrentState.ToString();
        }

        private void LateUpdate()
        {
            switch (CurrentState)
            {
                case State.FORCE_ROTATE_LEFT:
                    transform.localRotation *= Quaternion.Euler(0.0f, -stationaryRotationBoostDeg * Time.deltaTime, 0.0f);
                    return;
                case State.FORCE_ROTATE_RIGHT:
                    transform.localRotation *= Quaternion.Euler(0.0f, stationaryRotationBoostDeg * Time.deltaTime, 0.0f);
                    return;
                case State.FORCE_ROTATE_TO_FACE:
                    transform.rotation = forcedRotation;
                    forcedRotationApplied = true;
                    return;
                default:
                    return;

            }
        }

        private void UpdateAnimation()
        {
            if (CurrentState != State.FORCE_ROTATE_TO_FACE && CurrentState != State.BACKING_UP && CurrentState != State.TRYING_TO_STOP)
            {
                monster.AnimationManager.SetState(currentSpeed * animationScale, lastTurn, 0.0f, nextWaypoint);
            }
            else
            {
                monster.AnimationManager.SetState(currentSpeed * animationScale, lastTurn, 0.0f);
            }
        }

        private float ComputeTurn(Vector3 dir)
        {
            return Mathf.Clamp(Vector3.Dot(transform.right, dir) * 5.0f, -1.0f, 1.0f);
        }

        private void StationaryTurn(float turn)
        {
            if (turn != 0.0f)
            {
                lastTurn = turn;
            }
            if (lastTurn < 0.0f)
            {
                lastTurn = -1.0f;
                SetState(State.FORCE_ROTATE_LEFT);
            }
            else
            {
                lastTurn = 1.0f;
                SetState(State.FORCE_ROTATE_RIGHT);
            }
        }

        private void Stop(Vector3 dir, bool forceToFace)
        {
            SetState(State.TRYING_TO_STOP);
            AdjustVelocity(0.0f, "Stop");
            if (currentSpeed == 0.0f)
            {
                lastTurn = 0.0f;
                if (forceToFace)
                {
                    SetForcedRotation(dir);
                }
            }
            else
            {
                lastTurn = ComputeTurn(dir.normalized);
            }
            UpdateAnimation();
        }

        private void CalculateBackupSpeed(Vector3 dir)
        {
            float speed = Vector3.Dot(-transform.forward, dir);
            float turn = -ComputeTurn(dir);
            if (speed >= STOP_FOR_TURNS_OVER_COS)
            {
                AdjustVelocity(speed * -backwardSpeed, "Backing up");
                lastTurn = turn;
            }
            else
            {
                StationaryTurn(turn);
            }
        }

        private void BackingUp()
        {
            var pos = transform.position;
            var dir = nextWaypoint.position - pos;
            stopAt = nextWaypoint.tolerance - ComputeStoppingDistance();
            if (NearEnough(dir, stopAt) || Blocked(pos, -transform.forward))
            {
                thinking = "Stopping after back up";
                Stop(dir, true);
            }
            else
            {
                CalculateBackupSpeed(dir.normalized);
                UpdateAnimation();
            }
        }

        private void FixedUpdate()
        {
            switch (CurrentState)
            {
                case State.STATIONARY:
                    return;
                case State.FORCE_ROTATE_TO_FACE:
                    if (forcedRotationApplied)
                    {
                        SetState(State.STATIONARY);
                    }
                    return;
                case State.BACKING_UP:
                    BackingUp();
                    return;
                case State.ROTATING:
                    Rotating();
                    return;
                default:
                    break;
            }
            CalculateDirection();
        }

        private void Rotating()
        {
            float off = Vector3.Dot(targetRotation, transform.right);
            if (off < 0.05f && off > -0.05f)
            {
                SetForcedRotation(targetRotation);
                monster.AnimationManager.SetState(0.0f, 0.0f, 0.0f);
            }
            else
            {
                monster.AnimationManager.SetState(0.0f, Vector3.Dot(targetRotation, transform.right) * 2.0f, 0.0f);
            }
        }

        private void CalculateSpeed(Vector3 pos, Vector3 dir)
        {
            float speed = Vector3.Dot(transform.forward, dir);
            float turn = ComputeTurn(dir);
            if (speed >= STOP_FOR_TURNS_OVER_COS)
            {
                if (Blocked(pos, transform.forward))
                {
                    //Backup = monster.radius * 4.0f;
                    //Debug.Log("Blocked");
                    thinking = "stuck";
                    Stop(dir, true);
                }
                else
                {
                    SetState(State.MOVING);
                    AdjustVelocity(speed * monster.speed, "Calculate speed");
                    lastTurn = turn;
                }
            }
            else
            {
                StationaryTurn(turn);
            }
        }

        private void CalculateDirection()
        {
            var pos = Lifted(transform.position);
            var dir = nextWaypoint.position - pos;
            dir.y = 0;
            if (CurrentState == State.TRYING_TO_STOP)
            {
                Stop(dir, true);
                return;
            }
            stopAt = nextWaypoint.tolerance + ComputeStoppingDistance();
            if (NearEnough(dir, stopAt))
            {
                if (targets.Count == 0)
                {
                    Stop(dir, true);
                    return;
                }
                else
                {
                    dir = PopWaypoint(pos);
                }
            }
            while (targets.Count != 0 && ClearPathToFollow(pos, Lifted(targets[targets.Count - 1].position)))
            {
                dir = PopWaypoint(pos);
            }
            CalculateSpeed(pos, dir.normalized);
            UpdateAnimation();
        }
    }
}
