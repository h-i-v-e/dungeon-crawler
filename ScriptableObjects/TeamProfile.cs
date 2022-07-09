using UnityEngine;

namespace Depravity
{
    [CreateAssetMenu(fileName = "TeamProfile", menuName = "Depravity/Team Profile")]
    public class TeamProfile : ScriptableObject
    {
        public string teamName;
        public string[] alliedTeams;
    }
}
