using System.Collections;
using UnityEngine;

namespace CodeBase.Infrastructure.CoroutineRunner
{
    public interface ICoroutineRunner
    {
        Coroutine RunCoroutine(IEnumerator coroutine);
        void EndCoroutine(Coroutine coroutine);
    }
}