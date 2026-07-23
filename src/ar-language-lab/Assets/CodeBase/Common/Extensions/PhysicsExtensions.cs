using UnityEngine;

namespace CodeBase.Extentions
{
    public static class PhysicsExtensions
    {
        public static void IgnoreCollision(this GameObject firstTarget, GameObject secondTarget) => 
            SetCollisionMode(firstTarget, secondTarget, true);

        public static void EnableCollision(this GameObject firstTarget, GameObject secondTarget) => 
            SetCollisionMode(firstTarget, secondTarget, false);

        private static void SetCollisionMode(GameObject firstTarget, GameObject secondTarget, bool ignoreCollision)
        {
            var firstTargetColliders = GetAllCollidersFromObject(firstTarget);
            var secondTargetColliders = GetAllCollidersFromObject(secondTarget);

            foreach (var first in firstTargetColliders)
            foreach (var second in secondTargetColliders)
                Physics.IgnoreCollision(first, second, ignoreCollision);
        }

        private static Collider[] GetAllCollidersFromObject(GameObject firstTarget) => 
            firstTarget.GetComponentsInChildren<Collider>();
    }
}
