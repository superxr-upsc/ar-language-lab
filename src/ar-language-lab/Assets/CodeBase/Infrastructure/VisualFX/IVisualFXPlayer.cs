using System;
using UnityEngine;

namespace CodeBase.Infrastructure.VisualFX
{
    public interface IVisualFXPlayer : IDisposable
    {
        void PlayEffectInstant(VisualFX effectPrefab, Vector3 position, float duration = 0, Transform target = null);
        void PlayEffectWithDelay(VisualFX effectPrefab, Vector3 position, float delay, float duration = 0);
    }
}