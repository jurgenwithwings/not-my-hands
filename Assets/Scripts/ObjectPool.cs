using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ObjectPooling {
    public static class ObjectPool<T> where T : MonoBehaviour, IPoolable<T>
    {
        private static string errorString = $"{typeof(T).Name} Pool Prefab is null. 'IPoolable<T>.SetPoolPrefab(PoolPrefab);' must be called in OnValidate!";
        
        private static readonly Stack<T> pool = new();
        private static Transform parent;
        private static GameObject prefab;

        static ObjectPool()
        {
            string name = $"{typeof(T).Name} Pool";
            parent = new GameObject(name).transform;
            if (IPoolable<T>.PoolPrefab == null) {
                Debug.LogError(errorString);
            }else{
                prefab = IPoolable<T>.PoolPrefab;
            }
        }

        public static T Pull(Vector3? position = null, Quaternion? rotation = null)
        {
            T instance;
            if (pool.Count > 0)
            {
                instance = pool.Pop();
            }
            else
            {
                if (prefab == null)
                {
                    Debug.LogError(errorString);
                    return null;
                }
                instance = Object.Instantiate(prefab, parent).GetComponent<T>();
                instance.Initialise(Push);
            }

            if (position.HasValue) instance.transform.position = position.Value;
            if (rotation.HasValue) instance.transform.rotation = rotation.Value;

            instance.gameObject.SetActive(true);
            instance.OnPoolSpawn();

            return instance;
        }

        public static void Push(T instance)
        {
            instance.OnPoolDespawn();
            instance.gameObject.SetActive(false);
            instance.transform.SetParent(parent, false);
            pool.Push(instance);
        }

        public static int Count => pool.Count;
    }

    public interface IPoolable<T> where T : MonoBehaviour, IPoolable<T>
    {
        // Every poolable type MUST define this prefab reference.
        static GameObject PoolPrefab { get; set; }

        public Action<T> ReturnToPoolAction { get; set; }

        void Initialise(Action<T> returnToPoolAction) {
            ReturnToPoolAction = returnToPoolAction;
        }
        
        /// Unity calls this when the object is spawned from the pool.
        void OnPoolSpawn();

        /// Unity calls this before the object is returned to the pool.
        void OnPoolDespawn();
    }
    
    public static class IPoolableExtensions
    {
        public static T Pull<T>(Vector3 pos, Quaternion rot) where T : MonoBehaviour, IPoolable<T>
            => ObjectPool<T>.Pull(pos, rot);
    }
}