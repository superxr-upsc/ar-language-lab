using CodeBase.Gameplay.ARObjects;
using JahroConsole;
using UnityEngine;

namespace CodeBase.DebugExtensions
{
    public class JahroARObjectCommands : IJahroCommands
    {
        [JahroCommand("set-enter-tracking-max-distance", "AR", "Change the max object enter scan distance to track AR objects for speech recognition")]
        public void SetEnterTrackingMaxDistance(float value)
        {
            var arObjects = GameObject.FindObjectsByType<ARObjectObserver>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var arObject in arObjects) 
                arObject.ChangeEnterTrackingMaxDistance(value);
        }
        
        [JahroCommand("set-exit-tracking-max-distance", "AR", "Change the max object exit scan distance to track AR objects for speech recognition")]
        public void SetExitTrackingMaxDistance(float value)
        {
            var arObjects = GameObject.FindObjectsByType<ARObjectObserver>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var arObject in arObjects) 
                arObject.ChangeExitTrackingMaxDistance(value);
        }
        
        [JahroCommand("set-scan-once-state", "AR", "Change the max scan distance to track AR objects for speech recognition")]
        public void SetMaxScanDistance(bool value)
        {
            var arObjects = GameObject.FindObjectsByType<ARObjectObserver>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var arObject in arObjects) 
                arObject.SetOneShotOnlyState(value);
        }
    }
}