using PrimeTween;
using UnityEngine;

namespace CodeBase.UI
{
    public class CrosshairAnimation : MonoBehaviour
    {
        [SerializeField] private RectTransform _targetTransform;
        [SerializeField] private float _minScale = 0.9f;
        [SerializeField] private float _maxScale = 1.1f;
        [SerializeField] private float _duration = 0.8f;
        [SerializeField] private Ease _ease = Ease.InOutSine;

        private Tween _scaleTween;

        private void Awake()
        {
            if (_targetTransform == null)
                _targetTransform = transform as RectTransform;
        }

        private void OnEnable()
        {
            StartAnimation();
        }

        private void OnDisable()
        {
            StopAnimation();
        }

        public void StartAnimation()
        {
            StopAnimation();

            Transform target = _targetTransform != null ? _targetTransform : transform;
            target.localScale = Vector3.one * _minScale;

            _scaleTween = Tween.Scale(
                target,
                startValue: _minScale,
                endValue: _maxScale,
                duration: _duration,
                ease: _ease,
                cycles: -1,
                cycleMode: CycleMode.Yoyo
            );
        }

        public void StopAnimation()
        {
            if (_scaleTween.isAlive)
                _scaleTween.Stop();
        }
    }
}

