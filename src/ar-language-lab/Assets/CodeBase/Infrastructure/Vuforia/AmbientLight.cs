using CodeBase.Infrastructure.EventBroker;
using CodeBase.Infrastructure.EventBroker.Handlers;
using UnityEngine;
using Vuforia;
using Zenject;

namespace CodeBase.Infrastructure.Vuforia
{
    [RequireComponent(typeof(Light))]
    public class AmbientLight : MonoBehaviour, IGameLoopInitializable
    {
        private IVuforiaService _vuforiaService;
        private IEventBrokerService _eventBrokerService;
        private Light _directionalLight;
        
        private World _vuforiaWorld;
        private float _maxIntensity;

        [Inject]
        private void Construct(IVuforiaService vuforiaService, IEventBrokerService eventBrokerService)
        {
            _vuforiaService =  vuforiaService;
            _eventBrokerService =  eventBrokerService;
            _directionalLight = GetComponent<Light>();

            _maxIntensity = _directionalLight.intensity;
            _eventBrokerService.Subscribe(this);
        }

        private void OnDestroy()
        {
            _eventBrokerService.Unsubscribe(this);
            _vuforiaWorld = null;
        }

        private void Update()
        {
            if (_vuforiaWorld == null || _vuforiaWorld.IlluminationData.AmbientIntensity == null)
            {
                _directionalLight.intensity = _maxIntensity;
                return;
            }

            var intensity = _vuforiaWorld.IlluminationData.AmbientIntensity.Value / 1000f;

            _directionalLight.intensity = Mathf.Clamp(intensity, 0, _maxIntensity);
            RenderSettings.ambientIntensity = Mathf.Clamp01(intensity);
        }

        public void OnGameLoopInitialized() => 
            _vuforiaWorld = _vuforiaService.GetWorld();
    }
}