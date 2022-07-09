using System.Collections.Generic;
using UnityEngine;

namespace Depravity
{
    public struct ObsticleNavigatorNode
    {
        public Vector2 pos;
        public float distanceTravelled;

        public ObsticleNavigatorNode(Vector2 pos)
        {
            this.pos = pos;
            distanceTravelled = 0.0f;
        }

        public override int GetHashCode()
        {
            return pos.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return (obj is ObsticleNavigatorNode o) && o.pos == pos; 
        }
    }

    public interface IObsticleNavigatorPath
    {
        List<ObsticleNavigatorNode> GetWaypoints();
    }

    public static class ObsticleNavigator
    {
        //private static readonly RaycastHit[] hits = new RaycastHit[16];

        /*public static bool Raycast(Ray ray, float maxDistance, int ignoreLayer, out RaycastHit hit)
        {
            for (int i = 0, j = Physics.RaycastNonAlloc(ray, hits, maxDistance, 0); i != j; ++i)
            {
                var h = hits[i];
                if (h.collider.gameObject.layer != ignoreLayer)
                {
                    hit = h;
                    return true;
                }
            }
            hit = new RaycastHit();
            return false;
        }*/

        public static bool FindObsticle(Vector3 pos, Vector3 target, float radius, out RaycastHit rh)
        {
            var dir = target - pos;
            var mag = dir.magnitude;
            dir /= mag;
            float rad = radius + float.Epsilon;
            if (Physics.Raycast(new Ray(pos, dir), out rh, mag))
            {
                return true;
            }
            var across = Vector3.Cross(dir.normalized, Vector3.up).normalized * rad;
            //Will we bang into anything on our left or right sides
            return Physics.Raycast(pos + across, dir, out rh, mag, 1) || Physics.Raycast(pos - across, dir, out rh, mag, 1);
        }

        private class ObsticleNavigatorPath : AStar<ObsticleNavigatorNode>, IObsticleNavigatorPath
        {
            private Vector2 goal;
            private float radius;
            private readonly List<ObsticleNavigatorNode> waypoints = new List<ObsticleNavigatorNode>();
            private HashSet<Collider> alreadyHit = new HashSet<Collider>();
            private int maxExpansions;

            internal ObsticleNavigatorPath() { }

            protected override float ApplyHueristic(AStarNode<ObsticleNavigatorNode> parent, ObsticleNavigatorNode from)
            {
                if (parent == null)
                {
                    from.distanceTravelled = 0.0f;
                    return (goal - from.pos).magnitude;
                }
                from.distanceTravelled = (parent.state.pos - from.pos).magnitude + parent.state.distanceTravelled;
                return (goal - from.pos).magnitude + from.distanceTravelled;
            }

            private void AddBumpPoint(RaycastHit rh, Vector2 pos, Vector2 moved, List<ObsticleNavigatorNode> output)
            {
                var dist = rh.distance - (radius + float.Epsilon);
                if (dist > 0.0)
                {
                    var p = pos + (moved - pos).normalized * dist;
                    if (!FindObsticle(Vec22Vec3(pos), Vec22Vec3(p), radius, out var _)) {
                        output.Add(new ObsticleNavigatorNode(p));
                    }
                }
            }

            private void AddOffsetCourner(Collider boxCollider, Vector2 pos, Vector2 moved, List<ObsticleNavigatorNode> output)
            {
                if (FindObsticle(Vec22Vec3(pos), Vec22Vec3(moved), radius, out var rh))
                {
                    if (rh.collider != boxCollider)
                    {
                        AddBumpPoint(rh, pos, moved, output);
                    }
                }
                else
                {
                    output.Add(new ObsticleNavigatorNode(moved));
                }
            }

            private void AddCourner(Collider boxCollider, Vector2 from, Vector2 courner, List<ObsticleNavigatorNode> output)
            {
                var comp = Vector2.Perpendicular((courner - from).normalized) * (radius + float.Epsilon);
                var moved = courner + comp;
                AddOffsetCourner(boxCollider, from, moved, output);
                moved = courner - comp;
                AddOffsetCourner(boxCollider, from, moved, output);
            }

            private static bool PointInBoxCollider(Vector3 point, BoxCollider box)
            {
                point = box.transform.InverseTransformPoint(point);
                float x = point.x - box.center.x, z = point.z - box.center.z;
                float halfX = box.size.x * 0.5f;
                float halfZ = box.size.z * 0.5f;
                return x < halfX && x > -halfX &&
                   z < halfZ && z > -halfZ;
            }

            private static bool PointInCircle(Vector2 point, Vector2 centre, float radius)
            {
                return (point - centre).sqrMagnitude <= radius * radius;
            }

            private static Vector2 Vec32Vec2(Vector3 vec3)
            {
                return new Vector2(vec3.x, vec3.z);
            }

            private Vector3 Vec22Vec3(Vector2 vec2)
            {
                return new Vector3(vec2.x, radius, vec2.y);
            }

            private void Bounce(Vector2 from, RaycastHit hit, List<ObsticleNavigatorNode> output)
            {
                var dir = Vec32Vec2(hit.point) - from;
                float dist = dir.magnitude;
                dir /= dist;
                dist -= radius + float.Epsilon;
                if (dist > 0.0f)
                {
                    output.Add(new ObsticleNavigatorNode(from + (dist * dir)));
                }
            }

            private void ExpandBoxCollider(Vector2 from, BoxCollider boxCollider, RaycastHit hit, List<ObsticleNavigatorNode> output)
            {
                if (PointInBoxCollider(goal, boxCollider))
                {
                    var point = Vec32Vec2(hit.point);
                    var dir = (point - from).normalized;
                    goal = point - dir * radius;
                    output.Add(new ObsticleNavigatorNode(goal));
                    return;
                }
                Bounce(from, hit, output);
                var size = boxCollider.size;
                var trans = boxCollider.transform;
                Vector2 centre = Vec32Vec2(boxCollider.center) + Vec32Vec2(trans.position),
                    right = Vec32Vec2(trans.right) * size.x,
                    forward = Vec32Vec2(trans.forward) * size.z;
                AddCourner(boxCollider, from, centre + forward - right, output);
                AddCourner(boxCollider, from, centre + forward + right, output);
                AddCourner(boxCollider, from, centre - forward - right, output);
                AddCourner(boxCollider, from, centre - forward + right, output);
            }

            private void ExpandRadiusCollider(Vector2 from, Vector2 centre, float rad, List<ObsticleNavigatorNode> output)
            {
                var dir = centre - from;
                float mag = dir.magnitude;
                dir /= mag;
                if (PointInCircle(goal, centre, rad))
                {
                    goal = from + dir * (mag - (rad + radius));
                    output.Add(new ObsticleNavigatorNode(goal));
                    return;
                }
                var right = Vector2.Perpendicular(dir);
                right *= rad + radius + float.Epsilon;
                var target = centre + right;
                if (FindObsticle(Vec22Vec3(from), Vec22Vec3(target), rad, out var rh))
                {
                    AddBumpPoint(rh, from, target, output);
                }
                else
                {
                    output.Add(new ObsticleNavigatorNode(target));
                }
                target = centre - right;
                if (FindObsticle(Vec22Vec3(from), Vec22Vec3(target), rad, out rh))
                {
                    AddBumpPoint(rh, from, target, output);
                }
                else
                {
                    output.Add(new ObsticleNavigatorNode(target));
                }
            }

            private void ExpandCapsuleCollider(Vector2 from, CapsuleCollider collider, RaycastHit hit, List<ObsticleNavigatorNode> output)
            {
                Bounce(from, hit, output);
                ExpandRadiusCollider(from, Vec32Vec2(collider.transform.position + collider.center), collider.radius, output);
            }

            private void ExpandSphereCollider(Vector2 from, SphereCollider collider, RaycastHit hit, List<ObsticleNavigatorNode> output)
            {
                Bounce(from, hit, output);
                ExpandRadiusCollider(from, Vec32Vec2(collider.transform.position + collider.center), collider.radius, output);
            }

            private static bool Between(float val, float a, float b)
            {
                return (val <= a && val >= b) || (val <= b && val >= a);
            }

            private void ExpandMeshCollider(Vector2 from, MeshCollider collider, RaycastHit hit, List<ObsticleNavigatorNode> output)
            {
                var bounds = collider.bounds;
                Vector3 max = bounds.max, min = bounds.min;
                if (Between(goal.x, min.x, max.x) && Between(goal.y, min.z, max.z))
                {
                    output.Add(new ObsticleNavigatorNode(goal));
                    return;
                }
                Bounce(from, hit, output);
                AddCourner(collider, from, Vec32Vec2(max), output);
                AddCourner(collider, from, Vec32Vec2(min), output);
                AddCourner(collider, from, new Vector2(min.x, max.z), output);
                AddCourner(collider, from, new Vector2(max.x, min.z), output);
            }

            private void TryAddHit(RaycastHit hit, Vector2 pos, List<ObsticleNavigatorNode> output)
            {
                if (hit.collider is BoxCollider bc)
                {
                    ExpandBoxCollider(pos, bc, hit, output);
                }
                else if (hit.collider is CapsuleCollider cc)
                {
                    ExpandCapsuleCollider(pos, cc, hit, output);
                }
                else if (hit.collider is SphereCollider sc)
                {
                    ExpandSphereCollider(pos, sc, hit, output);
                }
                else if (hit.collider is MeshCollider mc)
                {
                    ExpandMeshCollider(pos, mc, hit, output);
                }
            }

            private bool TryAdd(Vector2 from, Vector2 point, List<ObsticleNavigatorNode> output)
            {
                if (!FindObsticle(Vec22Vec3(from), Vec22Vec3(point), radius, out _))
                {
                    output.Add(new ObsticleNavigatorNode(point));
                    return true;
                }
                return false;
            }

            protected override void Expand(ObsticleNavigatorNode from, List<ObsticleNavigatorNode> output)
            {
                var pos = from.pos;
                if (FindObsticle(Vec22Vec3(pos), Vec22Vec3(goal), radius, out var hit))
                {
                    if (alreadyHit.Contains(hit.collider))
                    {
                        return;
                    }
                    if (maxExpansions == 0)
                    {
                        return;
                    }
                    --maxExpansions;
                    alreadyHit.Add(hit.collider);
                    if (!Mathf.Approximately(goal.y, pos.y))
                    {
                        TryAdd(pos, new Vector2(pos.x, goal.y), output);
                    }
                    if (!Mathf.Approximately(goal.x, pos.x))
                    {
                        TryAdd(pos, new Vector2(goal.x, pos.y), output);
                    }
                    TryAddHit(hit, pos, output);
                }
                else
                {
                    output.Add(new ObsticleNavigatorNode(goal));
                }
            }

            protected override bool IsGoal(ObsticleNavigatorNode node)
            {
                return node.pos == goal;
            }

            public List<ObsticleNavigatorNode> GetWaypoints()
            {
                return waypoints; 
            }

            internal void Solve(Vector2 pos, float radius, Vector2 goal, int maxExpansions)
            {
                this.radius = radius;
                this.goal = goal;
                this.maxExpansions = maxExpansions;
                waypoints.Clear();
                alreadyHit.Clear();
                Search(new ObsticleNavigatorNode(pos), waypoints);
            }
        }

        private static ObsticleNavigatorPath Create()
        {
            return new ObsticleNavigatorPath();
        }

        private static readonly ResourcePool<ObsticleNavigatorPath> pool = new ResourcePool<ObsticleNavigatorPath>(Create);

        public static IObsticleNavigatorPath FindPath(Vector2 from, Vector2 destination, float radius, int maxExpansions = 100)
        {
            var path = pool.Allocate();
            path.Solve(from, radius, destination, maxExpansions);
            return path;
        }

        public static void ReleasePath(IObsticleNavigatorPath path)
        {
            pool.Release((ObsticleNavigatorPath)path);
        }
    }
}
