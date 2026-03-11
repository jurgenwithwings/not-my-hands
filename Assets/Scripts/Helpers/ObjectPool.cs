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
        private static string Key;
        public static bool isInitialised { get; private set; }= false;
        public static AsyncOperationHandle<GameObject> loadHandle { get; private set; }
        
        private static readonly Stack<T> pool = new();
        private static Transform parent;
        private static GameObject prefab;
        
        public static int Count => pool.Count;

        static ObjectPool() {
            //Get the IPoolable to set the key - Holy ass code
            parent = new GameObject($"{typeof(T).Name} Pool").transform;
            T t = parent.gameObject.AddComponent<T>();
            Key = t.ObjectPoolKey();
            Object.Destroy(t);

            loadHandle = Addressables.LoadAssetAsync<GameObject>(Key);
            loadHandle.Completed += InitialisePool;
        }

        private static void InitialisePool(AsyncOperationHandle<GameObject> handle) {
            loadHandle.Completed -= InitialisePool;
            if (loadHandle.Status == AsyncOperationStatus.Succeeded) {
                prefab = loadHandle.Result;
                isInitialised = true;
            } else {
                Debug.LogError($"Failed to load pool prefab for {typeof(T).Name} with key {Key}.");
            }
        }

        public static T Pull(Vector3? position = null, Quaternion? rotation = null) {
            T instance;
            if (pool.Count > 0) { //Get an item from the pool if one exists
                instance = pool.Pop();
            }
            else {
                if (prefab == null) {
                    Debug.LogError($"prefab is null for {typeof(T).Name}");
                    return null;
                }
                instance = Object.Instantiate(prefab, parent).GetComponent<T>();
                instance.Initialise(Push);
            }

            if (position.HasValue) instance.transform.position = position.Value;
            if (rotation.HasValue) instance.transform.rotation = rotation.Value;

            instance.gameObject.SetActive(true);
            instance.OnPoolPull();

            return instance;
        }

        public static void Push(T instance) {
            instance.OnPoolPush();
            instance.gameObject.SetActive(false);
            instance.transform.SetParent(parent, false);
            pool.Push(instance);
        }
    }
    
    public static class ObjectPool {
        private static List<Type> runningTasks = new();

        /// <summary>
        /// Initialises the pool for type T by preloading from Addressables. Optionally pre-instantiates a number of objects.
        /// DO NOT INITIALISE AN ASSET CONTAINING TEXT MESH PRO NEAR A SCENE LOAD WITH ANY PRE-SPAWNED.
        /// </summary>
        /// <param name="numberToInitialise">How many objects to initialise.</param>
        /// <typeparam name="T">Type of ObjectPool to preload</typeparam>
        public static void InitialisePool<T>(int numberToInitialise = 0) where T : MonoBehaviour, IPoolable<T> {
            if (!ObjectPool<T>.isInitialised && !runningTasks.Contains(typeof(T))) {
                Task.Run(() => Initialise<T>(numberToInitialise));
                runningTasks.Add(typeof(T));
            }
        }
        
        private static async Task Initialise<T>(int numberToInitialise) where T : MonoBehaviour, IPoolable<T> {
            while (!ObjectPool<T>.isInitialised) {
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
        
        /// <summary>
        /// Safe way to pull an Object from a ObjectPool. Returns false if the pool is not initialised.
        /// </summary>
        /// <param name="result">The object that was pulled from the pool.</param>
        /// <param name="position">Position to place the object.</param>
        /// <param name="rotation">Rotation to orient the object.</param>
        /// <returns></returns>
        public static bool TryPull<T>(out T result, Vector3? position = null, Quaternion? rotation = null) where T : MonoBehaviour, IPoolable<T> {
            if (ObjectPool<T>.isInitialised) {
                result = ObjectPool<T>.Pull(position, rotation);
                return true;
            }
            result = null;
            return false;
        }
        
        /// <summary>
        /// Pushes an object back into its pool. Nothing fancy, just here for consistent syntax.
        /// </summary>
        /// <param name="obj">The object to push back into the pool.</param>
        public static void Push<T>(T obj) where T : MonoBehaviour, IPoolable<T> {
            ObjectPool<T>.Push(obj);
        }
    }
    
    public interface IPoolable<T> where T : MonoBehaviour, IPoolable<T> {
        public Action<T> ReturnToPool { get; set; }

        string ObjectPoolKey();
        
        void Initialise(Action<T> returnToPoolAction) {
            ReturnToPool = returnToPoolAction;
        }
        
        /// <summary>
        /// Called when the object is spawned from the pool.
        /// </summary>
        void OnPoolPull();

        /// <summary>
        /// Called when the object is pushed back into the pool.
        /// </summary>
        void OnPoolPush();
    }
}