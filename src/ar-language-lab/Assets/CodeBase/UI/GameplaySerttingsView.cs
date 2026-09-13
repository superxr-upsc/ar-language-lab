using CodeBase.Infrastructure.Vuforia;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace CodeBase.UI
{
    public class GameplaySerttingsView : MonoBehaviour
    {
        [SerializeField] private Toggle _flashToggle;
        
        private IVuforiaService _vuforiaService;

        [Inject]
        private void Construct(IVuforiaService vuforiaService)
        {
            _vuforiaService = vuforiaService;
        }
        
        private void Awake()
        {
            _flashToggle.isOn = false;
            _flashToggle.onValueChanged.AddListener(ToggleFlash);
        }

        private void OnDestroy() => 
            _flashToggle.onValueChanged.RemoveListener(ToggleFlash);

        private void ToggleFlash(bool isOn) => 
            _vuforiaService.SetDeviceFlashTorch(isOn);
    }
}