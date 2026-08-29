using CodeBase.Gameplay.ARObjects;
using CodeBase.Gameplay.Lessons.Tasks;
using CodeBase.Infrastructure.ProjectResourcesProvider;
using JahroConsole;
using UnityEngine;

namespace CodeBase.DebugExtensions
{
    public class JahroARObjectCommands : IJahroCommands
    {
        private readonly IProjectResourcesProvider _projectResourcesProvider;
        private readonly TaskResolversSettings _taskResolversSettings;

        public JahroARObjectCommands(IProjectResourcesProvider projectResourcesProvider)
        {
            _projectResourcesProvider = projectResourcesProvider;
            _taskResolversSettings = _projectResourcesProvider.LoadResource<TaskResolversSettings>();
        }
        
        #region ARScanSettings

        [JahroCommand("set-enter-tracking-max-distance", "ARScanSettings", "Change the max object enter scan distance to track AR objects for speech recognition")]
        public void SetEnterTrackingMaxDistance(float value)
        {
            var arObjects = GameObject.FindObjectsByType<ARObjectObserver>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var arObject in arObjects) 
                arObject.ChangeEnterTrackingMaxDistance(value);
        }
        
        [JahroCommand("set-exit-tracking-max-distance", "ARScanSettings", "Change the max object exit scan distance to track AR objects for speech recognition")]
        public void SetExitTrackingMaxDistance(float value)
        {
            var arObjects = GameObject.FindObjectsByType<ARObjectObserver>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var arObject in arObjects) 
                arObject.ChangeExitTrackingMaxDistance(value);
        }
        
        [JahroCommand("set-scan-once-state", "ARScanSettings", "Change the max scan distance to track AR objects for speech recognition")]
        public void SetMaxScanDistance(bool value)
        {
            var arObjects = GameObject.FindObjectsByType<ARObjectObserver>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var arObject in arObjects) 
                arObject.SetOneShotOnlyState(value);
        }

        #endregion
        
        #region Task resolver settings change
        
        [JahroCommand("set-near-distance-meters", "ARTaskSettingsChange", "Distance to track if AR object is near or not, in meters.")]
        public void SetNearDistanceMeters(float value) => 
            _taskResolversSettings.NearDistanceMeters = value;

        [JahroCommand("set-side-offset-meters", "ARTaskSettingsChange", "Distance to track if AR object is on the side or not, and to track if objects are near in one line, in meters.")]
        public void SetSideOffsetMeters(float value) => 
            _taskResolversSettings.SideOffsetMeters = value;

        [JahroCommand("set-top-horizontal-to-tolerance-meters", "ARTaskSettingsChange", "Distance to track if AR object is on the top or not, in meters.")]
        public void SetTopHorizontalToleranceMeters(float value) => 
            _taskResolversSettings.TopHorizontalToleranceMeters = value;

        [JahroCommand("set-axis-adventage-meters", "ARTaskSettingsChange", "Distance to track if AR is placed on other sides from the reference object, in meters.")]
        public void SetAxisAdvantageMeters(float value) => 
            _taskResolversSettings.AxisAdvantageMeters = value;

        [JahroCommand("set-required-stable-frames", "ARTaskSettingsChange", "How much frames are need with stable position to track and check if AR object is placed correctly.")]
        public void SetRequiredStableFrames(int value) => 
            _taskResolversSettings.RequiredStableFrames = value;

        #endregion
    }
}