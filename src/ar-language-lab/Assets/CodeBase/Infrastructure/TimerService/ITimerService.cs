using System.Collections.Generic;

namespace CodeBase.Infrastructure.TimerService {
    public interface ITimerService
    {
        IGameTimer CreateTimer(float duration);

        IReadOnlyList<IGameTimer> ActiveTimers { get; }

        void PauseAll();
        void ResumeAll();
        void StopAll();
    }
}