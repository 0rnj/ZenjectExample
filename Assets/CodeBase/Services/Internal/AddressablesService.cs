using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeBase.Services.Public;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace CodeBase.Services.Internal
{
    public sealed class AddressablesService : IAddressablesService
    {
        private readonly Dictionary<string, IResourceLocation> _resourcesLocations = new();

        private readonly Dictionary<string, AsyncOperationsContainer> _assetContainers = new();

        private readonly Dictionary<GameObject, string> _instanceKeys = new();

        public async void Initialize() => 
            await UnityEngine.AddressableAssets.Addressables.InitializeAsync().Task;

        private bool CheckResourceType(Type resourceType)
        {
            return !resourceType.IsValueType && resourceType.ReflectedType == null &&
                   resourceType.BaseType != typeof(System.Object)
                   && resourceType.BaseType != typeof(UnityEventBase) && resourceType.BaseType != typeof(UnityEvent)
                   && resourceType.BaseType?.BaseType != typeof(UnityEventBase) &&
                   resourceType.BaseType?.BaseType != typeof(UnityEvent);
        }

        public T LoadAssetImmediately<T>(object owner, AssetReferenceT<T> reference) where T : Object
        {
            var aoh = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<T>(reference);
            GetAssetContainer(reference.AssetGUID).Add(owner, aoh);
            aoh.WaitForCompletion();

            return aoh.IsValid() ? aoh.Result : null;
        }

        public T LoadAssetImmediately<T>(object owner, AssetReference reference) where T : Object
        {
            var aoh = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<T>(reference);
            GetAssetContainer(reference.AssetGUID).Add(owner, aoh);
            aoh.WaitForCompletion();

            return aoh.IsValid() ? aoh.Result : null;
        }

        public T LoadAssetImmediately<T>(object owner, string primaryKey) where T : Object
        {
            var aoh = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<T>(primaryKey);
            GetAssetContainer(primaryKey).Add(owner, aoh);
            aoh.WaitForCompletion();

            return aoh.IsValid() ? aoh.Result : null;
        }

        public void LoadAsset<T>(object owner, string primaryKey, Action<AsyncOperationHandle<T>> callback)
            where T : Object
        {
            var resourceLocation =
                GetResourceLocation(primaryKey, typeof(T).Name) ?? GetResourceLocation(primaryKey, null);
            LoadWithCallback(owner, primaryKey, resourceLocation, callback);
        }

        public void LoadAsset<T>(object owner, string primaryKey, IResourceLocation resourceLocation,
            Action<AsyncOperationHandle<T>> callback) where T : Object
        {
            LoadWithCallback(owner, primaryKey, resourceLocation, callback);
        }

        public void LoadAsset<T>(object owner, AssetReference reference, Action<AsyncOperationHandle<T>> callback)
            where T : Object
        {
            LoadWithCallback(owner, reference, callback);
        }

        public async Task<T> LoadAsync<T>(object owner, string primaryKey) where T : Object
        {
            var resourceLocation =
                GetResourceLocation(primaryKey, typeof(T).Name) ?? GetResourceLocation(primaryKey, null);
            var aoh = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<T>(resourceLocation);

            GetAssetContainer(primaryKey).Add(owner, aoh);

            await aoh.Task;

            return aoh.Result;
        }

        public async Task<T> LoadAsync<T>(object owner, AssetReferenceT<T> reference) where T : Object
            => await LoadAsync<T>(owner, reference as AssetReference);


        public async Task<T> LoadAsync<T>(object owner, AssetReference reference) where T : Object
        {
            var aoh = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<T>(reference);

            GetAssetContainer(reference.AssetGUID).Add(owner, aoh);

            await aoh.Task;

            return aoh.Result;
        }
        
        public async Task<T> LoadPrefabAsync<T>(object owner, AssetReference reference) where T : Component
        {
            var aoh = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>(reference);

            GetAssetContainer(reference.AssetGUID).Add(owner, aoh);

            await aoh.Task;

            return aoh.Result.GetComponent<T>();
        }

        public async Task<T> InstantiateAsync<T>(object owner, string primaryKey,
            InstantiationParameters parameters = default)
        {
            var resourceLocation =
                GetResourceLocation(primaryKey, typeof(T).Name) ?? GetResourceLocation(primaryKey, null);
            var aoh = UnityEngine.AddressableAssets.Addressables.InstantiateAsync(resourceLocation, parameters);

            GetAssetContainer(primaryKey).Add(owner, aoh);

            var gameObject = await aoh.Task;

            _instanceKeys[gameObject] = primaryKey;

            return gameObject.GetComponent<T>();
        }

        public async Task<T> InstantiateAsync<T>(object owner, AssetReference reference,
            InstantiationParameters parameters = default)
        {
            var aoh = UnityEngine.AddressableAssets.Addressables.InstantiateAsync(reference, parameters);
            var primaryKey = reference.AssetGUID;

            GetAssetContainer(primaryKey).Add(owner, aoh);

            var gameObject = await aoh.Task;

            _instanceKeys[gameObject] = primaryKey;

            return gameObject.GetComponent<T>();
        }

        public async Task<GameObject> InstantiateAsync(object owner, AssetReference reference,
            InstantiationParameters parameters = default)
        {
            var aoh = UnityEngine.AddressableAssets.Addressables.InstantiateAsync(reference, parameters);
            var primaryKey = reference.AssetGUID;

            GetAssetContainer(primaryKey).Add(owner, aoh);

            var gameObject = await aoh.Task;

            _instanceKeys[gameObject] = primaryKey;

            return gameObject;
        }

        public GameObject Instantiate(object owner, AssetReference reference,
            InstantiationParameters parameters = default)
        {
            var aoh = UnityEngine.AddressableAssets.Addressables.InstantiateAsync(reference, parameters);
            var primaryKey = reference.AssetGUID;

            GetAssetContainer(primaryKey).Add(owner, aoh);

            var gameObject = aoh.WaitForCompletion();

            _instanceKeys[gameObject] = primaryKey;

            return gameObject;
        }

        public async Task<SceneInstance> LoadSceneAsync(object owner, string key, LoadSceneMode loadSceneMode)
        {
            if (owner == null)
            {
                Debug.LogError($"[{GetType().Name}] [{nameof(LoadSceneAsync)}] Argument {nameof(owner)} is null");
                return default;
            }

            var aoh = CreateSceneLoadHandle(key, loadSceneMode);
            GetAssetContainer(key).Add(owner, aoh);
            await aoh.Task;

            return aoh.IsValid() ? aoh.Result : default;
        }
        
        public async Task<SceneInstance> LoadSceneAsync(object owner, AssetReference reference, LoadSceneMode loadSceneMode)
        {
            if (owner == null)
            {
                Debug.LogError($"[{GetType().Name}] [{nameof(LoadSceneAsync)}] Argument {nameof(owner)} is null");
                return default;
            }

            var primaryKey = reference.AssetGUID;
            var aoh = CreateSceneLoadHandle(primaryKey, loadSceneMode);
            GetAssetContainer(primaryKey).Add(owner, aoh);
            await aoh.Task;

            return aoh.IsValid() ? aoh.Result : default;
        }

        public void UnloadScene(object owner, string primaryKey, SceneInstance sceneInstance)
        {
            UnityEngine.AddressableAssets.Addressables.UnloadSceneAsync(sceneInstance);
            Release(owner, primaryKey);
        }
        
        public async Task UnloadScene(object owner, AssetReference reference, SceneInstance sceneInstance)
        {
            var primaryKey = reference.AssetGUID;
            var aoh = UnityEngine.AddressableAssets.Addressables.UnloadSceneAsync(sceneInstance);
            
            await aoh.Task;
            
            Release(owner, primaryKey);
        }

        public void ReleaseInstance(object owner, GameObject instancedObject)
        {
            if (_instanceKeys.TryGetValue(instancedObject, out var primaryKey))
            {
                GetAssetContainer(primaryKey, false)?.Release(owner);

                UnityEngine.AddressableAssets.Addressables.ReleaseInstance(instancedObject);


                _instanceKeys.Remove(instancedObject);
            }
            else
            {
                Debug.LogError($"PrimaryKey not found for {instancedObject}, owner: {owner}");
            }
        }

        public void Release(object owner, string primaryKey)
        {
            GetAssetContainer(primaryKey, false)?.Release(owner);
        }

        public void Release(object owner, AssetReference assetReference)
        {
            GetAssetContainer(assetReference.AssetGUID, false)?.Release(owner);
        }

        public void ReleaseAll()
        {
            foreach (var asyncOperationsContainer in _assetContainers.Values)
            {
                asyncOperationsContainer.ReleaseAll();
            }
        }

        public IResourceLocation GetResourceLocation(string primaryKey, string type)
        {
            var resourceKey = string.IsNullOrEmpty(type) ? primaryKey : $"{primaryKey}_{type}";

            _resourcesLocations.TryGetValue(resourceKey, out var resourceLocation);

            return resourceLocation;
        }

        private AsyncOperationsContainer GetAssetContainer(string primaryKey, bool createIfMissing = true)
        {
            if (!_assetContainers.ContainsKey(primaryKey))
            {
                if (createIfMissing == false)
                {
                    return default;
                }

                _assetContainers.Add(primaryKey, new AsyncOperationsContainer(primaryKey));
            }

            _assetContainers.TryGetValue(primaryKey, out var container);
            return container;
        }


        private AsyncOperationHandle<SceneInstance> CreateSceneLoadHandle(string resourceKey,
            LoadSceneMode loadSceneMode)
        {
            return _resourcesLocations.TryGetValue(resourceKey, out var resourceLocation)
                ? UnityEngine.AddressableAssets.Addressables.LoadSceneAsync(resourceLocation, loadSceneMode)
                : UnityEngine.AddressableAssets.Addressables.LoadSceneAsync(resourceKey, loadSceneMode);
        }
        
        private AsyncOperationHandle<SceneInstance> CreateSceneUnloadHandle(SceneInstance sceneInstance)
        {
            return UnityEngine.AddressableAssets.Addressables.UnloadSceneAsync(sceneInstance);
        }

        private async void LoadWithCallback<T>(object owner, string primaryKey, IResourceLocation resourceLocation,
            Action<AsyncOperationHandle<T>> callback) where T : Object
        {
            var aoh = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<T>(resourceLocation);

            aoh.Completed += callback;

            GetAssetContainer(primaryKey).Add(owner, aoh);

            await aoh.Task;
        }

        private async void LoadWithCallback<T>(object owner, AssetReference reference,
            Action<AsyncOperationHandle<T>> callback) where T : Object
        {
            var aoh = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<T>(reference);
            aoh.Completed += callback;

            GetAssetContainer(reference.AssetGUID).Add(owner, aoh);

            await aoh.Task;
        }

        private class AsyncOperationsContainer
        {
            public string PrimaryKey { get; private set; }

            private readonly List<AsyncOperationHandle> _handlers = new();
            private readonly List<WeakReference<object>> _owners = new();

            public AsyncOperationsContainer(string primaryKey)
            {
                PrimaryKey = primaryKey;
            }

            public void Add(object owner, AsyncOperationHandle operation)
            {
                _owners.Add(new WeakReference<object>(owner));
                _handlers.Add(operation);
            }

            public void Release(object owner)
            {
                _owners.RemoveAll(x => !x.TryGetTarget(out var target) || target == owner);

                if (_owners.Count == 0)
                {
                    ReleaseAll();
                }
            }

            public void ReleaseAll()
            {
                _owners.Clear();

                if (!_handlers.Any()) return;

                foreach (var asyncOperationHandle in _handlers)
                {
                    if (asyncOperationHandle.IsValid())
                    {
                        UnityEngine.AddressableAssets.Addressables.Release(asyncOperationHandle);
                    }
                }

                _handlers.Clear();

                Debug.Log($"Released async handlers for: {PrimaryKey}");
            }
        }
    }
}