using System;
using System.Collections.Generic;
using R3;

namespace CodeBase.Services.TimerService 
{
    public class TimerService : ITimerService, IDisposable
    {
        private readonly List<IGameTimer> _timers = new();

        public IReadOnlyList<IGameTimer> ActiveTimers => _timers;

        public IGameTimer CreateTimer(float duration)
        {
            var timer = new GameTimer(duration);

            _timers.Add(timer);

            timer.IsCompleted
                .Where(x => x)
                .Subscribe(_ => RemoveTimer(timer));

            timer.IsRunning
                .Where(x => !x)
                .Subscribe(_ =>
                {
                    if (!timer.IsCompleted.CurrentValue)
                        RemoveTimer(timer);
                });

            return timer;
        }

        public void PauseAll()
        {
            foreach (var timer in _timers)
                timer.Pause();
        }

        public void ResumeAll()
        {
            foreach (var timer in _timers)
                timer.Resume();
        }

        public void StopAll()
        {
            foreach (var timer in _timers)
                timer.Stop();

            _timers.Clear();
        }

        private void RemoveTimer(IGameTimer timer)
        {
            _timers.Remove(timer);
        }

        public void Dispose()
        {
            StopAll();
        }
    }
}