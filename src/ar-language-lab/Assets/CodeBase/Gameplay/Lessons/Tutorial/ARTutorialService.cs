using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CodeBase.Gameplay.ARObjects;
using CodeBase.Gameplay.Lessons.Tasks;
using CodeBase.Gameplay.SpeechSyntesis;
using CodeBase.Infrastructure.Localization;
using CodeBase.Infrastructure.ProjectResourcesProvider;
using CodeBase.Infrastructure.SaveLoad;
using CodeBase.Infrastructure.VisualFX;
using CodeBase.Infrastructure.Vuforia;
using CodeBase.Infrastructure.WindowsManagement;
using CodeBase.UI.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CodeBase.Gameplay.Lessons.Tutorial
{
    public class ARTutorialService : IDisposable
    {
        private readonly IReadOnlyList<ARObjectBase> _lessonObjects;
        private readonly Perspective _perspective;
        private readonly IWindowsManagementService _windowsManagementService;
        private readonly IARCameraProvider _arCameraProvider;
        private readonly ILocalizationService _localizationService;
        private readonly ISaveService _saveService;
        private readonly IVisualFXPlayer _visualFXPlayer;
        private readonly IProjectResourcesProvider _projectResourcesProvider;

        private readonly Speaker _speaker;
        private readonly TaskResolversSettings _settings;
        private readonly VisualFX _completeTaskFx;
        private readonly ActiveTaskData _activeTaskData;

        private ActiveTaskPresenter _activeTaskPresenter;
        private PlacementAreaIndicator _placementAreaIndicator;
        private ARObjectBase _highlightedObject;
        private CancellationTokenSource _tutorialCts;

        public ARTutorialService(
            IReadOnlyList<ARObjectBase> lessonObjects,
            Perspective perspective,
            IWindowsManagementService windowsManagementService,
            IARCameraProvider arCameraProvider,
            ILocalizationService localizationService,
            ISaveService saveService,
            IVisualFXPlayer visualFXPlayer,
            IProjectResourcesProvider projectResourcesProvider)
        {
            _lessonObjects = lessonObjects;
            _perspective = perspective;
            _windowsManagementService = windowsManagementService;
            _arCameraProvider = arCameraProvider;
            _localizationService = localizationService;
            _saveService = saveService;
            _visualFXPlayer = visualFXPlayer;
            _projectResourcesProvider = projectResourcesProvider;

            _speaker = arCameraProvider.GetSpeaker();
            _settings = projectResourcesProvider.LoadResource<TaskResolversSettings>();
            _completeTaskFx = projectResourcesProvider.LoadResource<VisualFX>();
            _activeTaskData = new ActiveTaskData();
        }

        public async UniTask RunTutorialAsync(CancellationToken cancellationToken)
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _tutorialCts = linkedCts;

            try
            {
                CreateTutorialView();

                // Step 1: Ensure all objects are visible
                await RunStep1EnsureAllObjectsVisible(linkedCts.Token);

                // Step 2: Detect the first object
                var firstObject = await RunStep2DetectFirstObject(linkedCts.Token);

                // Step 3: Place object near another object
                await RunStep3PlaceObjectNearAnother(firstObject, linkedCts.Token);

                // Step 4: Explain lesson structure
                await RunStep4ExplainLessonStructure(linkedCts.Token);

                // Mark tutorial completed and persist
                _saveService.SaveData.Lessons.TutorialCompleted = true;
                await _saveService.SaveAsync();

                CloseTutorialView();
            }
            finally
            {
                CleanupVisuals();
                _tutorialCts = null;
            }
        }

        private async UniTask RunStep1EnsureAllObjectsVisible(CancellationToken cancellationToken)
        {
            _activeTaskData.SetCurrentProgress(0.1f);

            if (!AreAllObjectsInView())
            {
                var message = "Position your camera so all objects are visible.";
                UpdateInstruction(message);
                SpeakInstruction(message, cancellationToken).Forget();

                while (!AreAllObjectsInView())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                }
            }

            _activeTaskData.SetCurrentProgress(0.25f);
        }

        private async UniTask<ARObjectBase> RunStep2DetectFirstObject(CancellationToken cancellationToken)
        {
            var orderedObjects = GetObjectsOrderedLeftToRight();
            var firstObject = orderedObjects.FirstOrDefault(o => o != null);
            if (firstObject == null)
                return null;

            _highlightedObject = firstObject;
            firstObject.SetOutlineState(true);

            var objectName = !string.IsNullOrEmpty(firstObject.LocalizedName)
                ? firstObject.LocalizedName
                : "object";

            var message = $"Bring the highlighted {objectName} closer to the camera.";
            UpdateInstruction(message);
            SpeakInstruction(message, cancellationToken).Forget();

            _activeTaskData.SetCurrentProgress(0.35f);

            if (firstObject.Observer == null || !firstObject.Observer.IsNear)
            {
                var detectionSource = new UniTaskCompletionSource();

                void OnNearEntered(float coverage, float distance)
                {
                    detectionSource.TrySetResult();
                }

                if (firstObject.Observer != null)
                {
                    firstObject.Observer.NearCameraEntered += OnNearEntered;
                    try
                    {
                        using (cancellationToken.Register(() => detectionSource.TrySetCanceled()))
                        {
                            await detectionSource.Task;
                        }
                    }
                    finally
                    {
                        firstObject.Observer.NearCameraEntered -= OnNearEntered;
                    }
                }
            }

            if (_completeTaskFx != null)
                _visualFXPlayer.PlayEffectInstant(_completeTaskFx, firstObject.transform.position);

            _activeTaskData.SetCurrentProgress(0.5f);
            return firstObject;
        }

        private async UniTask RunStep3PlaceObjectNearAnother(ARObjectBase firstObject, CancellationToken cancellationToken)
        {
            if (firstObject == null)
                return;

            var orderedObjects = GetObjectsOrderedLeftToRight();
            var secondObject = orderedObjects.FirstOrDefault(o => o != null && o != firstObject);
            if (secondObject == null)
                return;

            // Highlight target placement area separately from object outline
            var localOffset = _settings != null
                ? _settings.PlacementIndicatorOffset
                : new Vector3(0.18f, -0.08f, 0f);
            _placementAreaIndicator = PlacementAreaIndicator.Create(secondObject.transform, localOffset);

            var firstName = !string.IsNullOrEmpty(firstObject.LocalizedName) ? firstObject.LocalizedName : "object";
            var secondName = !string.IsNullOrEmpty(secondObject.LocalizedName) ? secondObject.LocalizedName : "another object";

            var message = $"Place the {firstName} near the {secondName}.";
            UpdateInstruction(message);
            SpeakInstruction(message, cancellationToken).Forget();

            _activeTaskData.SetCurrentProgress(0.6f);

            var nearDistance = _settings != null ? _settings.NearDistanceMeters : 0.25f;
            var sideOffsetMeters = _settings != null ? _settings.SideOffsetMeters : 0.08f;
            var axisAdvantageMeters = _settings != null ? _settings.AxisAdvantageMeters : 0.02f;
            var requiredFrames = _settings != null ? _settings.RequiredStableFrames : 10;
            var stableFrames = 0;

            CaptureSideDirections(out var sideRightDirection, out var sideForwardDirection);

            while (stableFrames < requiredFrames)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var subjectPosition = firstObject.transform.position;
                var referencePosition = secondObject.transform.position;
                var distance = Vector3.Distance(subjectPosition, referencePosition);
                var isNear = distance <= nearDistance;
                var isRightSide = IsOnRightSide(
                    subjectPosition,
                    referencePosition,
                    sideRightDirection,
                    sideForwardDirection,
                    sideOffsetMeters,
                    axisAdvantageMeters);
                var isTracked = (firstObject.Observer != null && firstObject.Observer.IsInView()) &&
                                (secondObject.Observer != null && secondObject.Observer.IsInView());

                if (isNear && isRightSide && isTracked)
                {
                    stableFrames++;
                }
                else
                {
                    stableFrames = 0;
                }

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            if (_completeTaskFx != null)
                _visualFXPlayer.PlayEffectInstant(_completeTaskFx, firstObject.transform.position);

            if (_placementAreaIndicator != null)
            {
                UnityEngine.Object.Destroy(_placementAreaIndicator.gameObject);
                _placementAreaIndicator = null;
            }

            firstObject.SetOutlineState(false);
            _highlightedObject = null;

            _activeTaskData.SetCurrentProgress(0.75f);
        }

        private async UniTask RunStep4ExplainLessonStructure(CancellationToken cancellationToken)
        {
            var message = "Great! Now you’re ready. Complete the tasks to finish your lesson.";
            UpdateInstruction(message);
            _activeTaskData.SetCurrentProgress(1.0f);

            AudioClip clip = null;
            if (_speaker != null)
            {
                clip = await _speaker.GenerateAudioClipAsync(message, cancellationToken);
                if (clip != null && !cancellationToken.IsCancellationRequested)
                {
                    _activeTaskData.TaskAudioClip = clip;
                    _speaker.Speak(clip);
                }
            }

            var duration = clip != null ? clip.length + 0.5f : 3.5f;
            var elapsed = 0f;

            while (elapsed < duration)
            {
                cancellationToken.ThrowIfCancellationRequested();

                elapsed += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }
        }

        private bool AreAllObjectsInView()
        {
            if (_lessonObjects == null || _lessonObjects.Count == 0)
                return false;

            foreach (var arObject in _lessonObjects)
            {
                if (arObject == null || arObject.Observer == null || !arObject.Observer.IsInView())
                    return false;
            }

            return true;
        }

        private List<ARObjectBase> GetObjectsOrderedLeftToRight()
        {
            var activeCamera = _arCameraProvider.GetActiveCamera();
            var orderedObjects = new List<ARObjectBase>(_lessonObjects.Where(o => o != null));

            orderedObjects.Sort((left, right) =>
            {
                var leftX = GetViewportHorizontalPosition(left.transform.position, activeCamera);
                var rightX = GetViewportHorizontalPosition(right.transform.position, activeCamera);
                return leftX.CompareTo(rightX);
            });

            return orderedObjects;
        }

        private static float GetViewportHorizontalPosition(Vector3 worldPosition, Camera camera)
        {
            if (camera == null)
                return worldPosition.x;

            return camera.WorldToViewportPoint(worldPosition).x;
        }

        private void CaptureSideDirections(out Vector3 sideRightDirection, out Vector3 sideForwardDirection)
        {
            var activeCamera = _arCameraProvider.GetActiveCamera();
            if (activeCamera == null)
            {
                sideRightDirection = Vector3.right;
                sideForwardDirection = Vector3.forward;
                return;
            }

            // Keep left/right/front checks horizontal and stable for the step.
            var projectedRight = Vector3.ProjectOnPlane(activeCamera.transform.right, Vector3.up);
            sideRightDirection = projectedRight.sqrMagnitude > 0.0001f
                ? projectedRight.normalized
                : Vector3.right;

            sideForwardDirection = Vector3.Cross(Vector3.up, sideRightDirection).normalized;
        }

        private static bool IsOnRightSide(
            Vector3 subjectPosition,
            Vector3 referencePosition,
            Vector3 sideRightDirection,
            Vector3 sideForwardDirection,
            float sideOffsetMeters,
            float axisAdvantageMeters)
        {
            var worldDelta = subjectPosition - referencePosition;
            var rightAxisDelta = Vector3.Dot(worldDelta, sideRightDirection);
            var upAxisDelta = Vector3.Dot(worldDelta, Vector3.up);
            var forwardAxisDelta = Vector3.Dot(worldDelta, sideForwardDirection);

            return rightAxisDelta >= sideOffsetMeters
                   && rightAxisDelta >= Mathf.Abs(upAxisDelta) + axisAdvantageMeters
                   && rightAxisDelta >= Mathf.Abs(forwardAxisDelta) + axisAdvantageMeters;
        }

        private void CreateTutorialView()
        {
            CloseTutorialView();
            _activeTaskPresenter = _windowsManagementService
                .CreateWindow<ActiveTaskPresenter, ActiveTaskView, ActiveTaskData>(UILayer.NotificationLayer, _activeTaskData);
        }

        private void UpdateInstruction(string message)
        {
            _activeTaskData.TaskDescription = message;
            _activeTaskPresenter?.UpdateTaskDescription(message);
        }

        private async UniTaskVoid SpeakInstruction(string message, CancellationToken cancellationToken)
        {
            if (_speaker == null || _perspective is not (Perspective.Audio or Perspective.Mix))
                return;

            try
            {
                var clip = await _speaker.GenerateAudioClipAsync(message, cancellationToken);
                if (clip != null && !cancellationToken.IsCancellationRequested)
                {
                    _activeTaskData.TaskAudioClip = clip;
                    _speaker.Speak(clip);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Tutorial] TTS failed: {ex.Message}");
            }
        }

        private void CloseTutorialView()
        {
            _activeTaskData?.Cleanup();
            if (_activeTaskPresenter != null)
            {
                _activeTaskPresenter.Close().Catch(Debug.LogException);
                _activeTaskPresenter = null;
            }
        }

        private void CleanupVisuals()
        {
            if (_highlightedObject != null)
            {
                _highlightedObject.SetOutlineState(false);
                _highlightedObject = null;
            }

            if (_placementAreaIndicator != null)
            {
                UnityEngine.Object.Destroy(_placementAreaIndicator.gameObject);
                _placementAreaIndicator = null;
            }

            CloseTutorialView();
        }

        public void Dispose()
        {
            if (_tutorialCts != null)
            {
                _tutorialCts.Cancel();
                _tutorialCts.Dispose();
                _tutorialCts = null;
            }

            CleanupVisuals();

            if (_settings != null)
            {
                _projectResourcesProvider.ReleaseResource(_settings);
            }
        }
    }
}
