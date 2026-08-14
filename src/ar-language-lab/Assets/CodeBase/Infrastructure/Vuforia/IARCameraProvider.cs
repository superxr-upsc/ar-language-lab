using UnityEngine;

namespace CodeBase.Infrastructure.Vuforia
{
    public interface IARCameraProvider
    {
        Camera GetActiveCamera();
    }
}

