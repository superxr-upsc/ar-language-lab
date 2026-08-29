using CodeBase.Infrastructure.ProjectResourcesProvider;
using UnityEngine;

namespace CodeBase.Gameplay.Lessons.Tasks
{
    [CreateAssetMenu(fileName = "TaskResolversSettings", menuName = "AR/TaskResolversSettings")]
    public class TaskResolversSettings : ScriptableObject, IResource
    {
        public float NearDistanceMeters = 0.25f;
        public float SideOffsetMeters = 0.08f;
        public float TopHorizontalToleranceMeters = 0.18f;
        public float AxisAdvantageMeters = 0.02f;
        public int RequiredStableFrames = 10;
    }
}