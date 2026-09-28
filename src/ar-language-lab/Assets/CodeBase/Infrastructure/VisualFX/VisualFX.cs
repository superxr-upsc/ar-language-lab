using CodeBase.Infrastructure.ProjectResourcesProvider;
using UnityEngine;

namespace CodeBase.Infrastructure.VisualFX
{
    [RequireComponent(typeof(ParticleSystem))]
    public class VisualFX : MonoBehaviour, IResource
    {
        [SerializeField] private ParticleSystem _particleSystem;

        public void Initialize()
        {
            if (_particleSystem == null) _particleSystem = GetComponent<ParticleSystem>();
        }
        
        public bool AutomatLaunching() => 
            _particleSystem.main.playOnAwake;

        public float GetEffectDuration()
        {
            var mainModule = _particleSystem.main;
            return mainModule.duration + mainModule.startLifetime.constantMax;
        }

        public void PlayEffect()
        {
            if (_particleSystem.isStopped == false)
            {
                _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            
            _particleSystem.Play();
        }
    }
}