using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ObjectPooling {
    public static class ObjectPool<T> where T : MonoBehaviour, IPoolable<T>
    {
        private static string errorString = $"{typeof(T).Name} Pool Prefab is null. 'IPoolable<T>.SetPoolPrefab(PoolPrefab);' must be called in OnValidate!";

        private static string Key;
        public static AsyncOperationHandle<GameObject> loadHandle { get; private set; }
        
        private static readonly Stack<T> pool = new();
        private static Transform parent;
        private static GameObject prefab;
        
        public static int Count => pool.Count;

        static ObjectPool()
        {
            //Get the IPoolable to set the key - Holy ass code
            parent = new GameObject($"{typeof(T).Name} Pool").transform;
            T t = parent.gameObject.AddComponent<T>();
            Key = t.SetKey();
            Object.Destroy(t);
            
            InitialisePool();
        }

        private static async void InitialisePool() {
            loadHandle = Addressables.LoadAssetAsync<GameObject>(Key);
            await loadHandle.Task;
            if (loadHandle.Status == AsyncOperationStatus.Succeeded) {
                prefab = loadHandle.Result;
            } else {
                Debug.LogError($"Failed to load pool prefab for {typeof(T).Name} with key {Key}.");
            }
        }

        public static T Pull(Vector3? position = null, Quaternion? rotation = null)
        {
            T instance;
            if (pool.Count > 0) //Get an item from the pool if one exists
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
    }
    
    public static class ObjectPool
    {
        private static List<Type> runningTasks = new();
        
        public static async Task InitialisePool<T>(int numberToInitialise = 0) where T : MonoBehaviour, IPoolable<T> {
            if (!ObjectPool<T>.loadHandle.IsDone && !runningTasks.Contains(typeof(T))) {
                runningTasks.Add(typeof(T));
                Debug.Log("Starting pool initialisation for " + typeof(T).Name);
            }
            else {
                Debug.Log("Initialisation is already running for " + typeof(T).Name);
                return;
            }
            
            while (!ObjectPool<T>.loadHandle.IsDone) {
                await Task.Yield();
            }

            Stack<T> stack = new Stack<T>();
            for (int i = 0; i < numberToInitialise; i++) {
                stack.Push(ObjectPool<T>.Pull());
            }
            while (stack.Count > 0) {
                ObjectPool<T>.Push(stack.Pop());
            }
            
        }
        
        public static bool TryPull<T>(Vector3 pos, Quaternion rot, out T t) where T : MonoBehaviour, IPoolable<T> {
            if (ObjectPool<T>.loadHandle.IsDone) {
                t = ObjectPool<T>.Pull(pos, rot);
                return true;
            }
            t = null;
            return false;
        }
        
        public static void Push<T>(T t) where T : MonoBehaviour, IPoolable<T> {
            ObjectPool<T>.Push(t);
        }
    }
    
    public interface IPoolable<T> where T : MonoBehaviour, IPoolable<T> {
        public Action<T> ReturnToPoolAction { get; set; }

        public static string Key = string.Empty;

        string SetKey();
        
        void Initialise(Action<T> returnToPoolAction) {
            ReturnToPoolAction = returnToPoolAction;
        }
        
        /// Unity calls this when the object is spawned from the pool.
        void OnPoolSpawn();

        /// Unity calls this before the object is returned to the pool.
        void OnPoolDespawn();
    }
}