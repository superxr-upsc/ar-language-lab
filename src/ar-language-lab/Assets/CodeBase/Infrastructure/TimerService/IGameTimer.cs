using Cysharp.Threading.Tasks;
using R3;

namespace CodeBase.Services.TimerService 
{
    public interface IGameTimer
    {
        float Duration { get; }
        float Elapsed { get; }
        float Progress { get; }

        ReadOnlyReactiveProperty<float> ProgressObservable { get; }
        ReadOnlyReactiveProperty<bool> IsRunning { get; }
        ReadOnlyReactiveProperty<bool> IsPaused { get; }
        ReadOnlyReactiveProperty<bool> IsCompleted { get; }

        UniTask Completed { get; }

        void Start();
        void Pause();
        void Resume();
        void Stop();
    }
}