using Cysharp.Threading.Tasks;
using Vuforia;

namespace CodeBase.Infrastructure.Vuforia
{
    public interface IVuforiaService
    {
        bool IsActive { get; }
        UniTask InitializeVuforia();
        void SetupVuforiaBehaviour();
        World GetWorld();
        bool SetDeviceFlashTorch(bool on);
        bool SetDeviceFocusMode(FocusMode focusMode);
        MultiTargetBehaviour CreateTarget(string vuforiaKey);
        void SetVuforiaState(bool isOn);
    }
}