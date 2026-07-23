using System;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace CodeBase.Infrastructure.GameFactory
{
    public class GameFactory : IGameFactory
    {
        private readonly IInstantiator _instantiator;

        public GameFactory(IInstantiator instantiator) =>
            _instantiator = instantiator;

        public T Create<T>(params object[] args) where T : class =>
            _instantiator.Instantiate<T>(args);

        public object Create(Type type, params object[] args) =>
            _instantiator.Instantiate(type, args);

        public T CreateFromPrefab<T>(Object prefab) where T : Component =>
            _instantiator.InstantiatePrefabForComponent<T>(prefab);

        public T CreateFromPrefab<T>(Object prefab, Transform parent) where T : Component =>
            _instantiator.InstantiatePrefabForComponent<T>(prefab, parent);

        public GameObject CreatePrefab(Object prefab, Transform parent) =>
            _instantiator.InstantiatePrefab(prefab, parent);
    }
}
