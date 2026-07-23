using System.Collections;
using UnityEngine;

namespace CodeBase.Infrastructure.CoroutineRunner
{
    public class CoroutineRunner : MonoBehaviour, ICoroutineRunner
    {
        public Coroutine RunCoroutine(IEnumerator coroutine)
        {
            return StartCoroutine(coroutine);
        }

        public void EndCoroutine(Coroutine coroutine)
        {
            if (coroutine != null) 
                StopCoroutine(coroutine);
        }
    }
}