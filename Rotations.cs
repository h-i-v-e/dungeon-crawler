using UnityEngine;

namespace Depravity
{

    class Rotations
    {
        public static readonly Quaternion rot180 = Quaternion.Euler(0.0f, 180.0f, 0.0f),
            rot90 = Quaternion.Euler(0.0f, 90.0f, 0.0f),
            rotNeg90 = Quaternion.Euler(0.0f, -90.0f, 0.0f);
    }
}