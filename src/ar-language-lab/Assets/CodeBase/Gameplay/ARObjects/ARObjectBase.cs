using UnityEngine;

namespace CodeBase.Gameplay.ARObjects
{
    public class ARObjectBase : MonoBehaviour
    {
        private ARObjectConfig _data;
        public void Initialize()
        {
        }

        public void Refresh()
        {
        }

        public void Cleanup()
        {
            Destroy(gameObject);
        }
    }
}