using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CodeBase.Infrastructure.GameFactory
{
    public interface IGameFactory
    {
        T Create<T>(params object[] args) where T : class;
        object Create(Type type, params object[] args);
        T CreateFromPrefab<T>(Object prefab) where T : Component;
        T CreateFromPrefab<T>(Object prefab, Transform parent) where T : Component;
        GameObject CreatePrefab(Object prefab, Transform parent);
    }
}
