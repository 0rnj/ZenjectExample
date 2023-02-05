using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace CodeBase.Services.Public
{
    public interface IAddressablesService
    {
        void Initialize();
        T LoadAssetImmediately<T>(object owner, AssetReferenceT<T> reference) where T : Object;
        T LoadAssetImmediately<T>(object owner, AssetReference reference) where T : Object;
        T LoadAssetImmediately<T>(object owner, string primaryKey) where T : Object;

        void LoadAsset<T>(object owner, string primaryKey, Action<AsyncOperationHandle<T>> callback)
            where T : Object;

        void LoadAsset<T>(object owner, string primaryKey, IResourceLocation resourceLocation,
            Action<AsyncOperationHandle<T>> callback) where T : Object;

        void LoadAsset<T>(object owner, AssetReference reference, Action<AsyncOperationHandle<T>> callback)
            where T : Object;

        Task<T> LoadAsync<T>(object owner, string primaryKey) where T : Object;
        Task<T> LoadAsync<T>(object owner, AssetReferenceT<T> reference) where T : Object;
        Task<T> LoadAsync<T>(object owner, AssetReference reference) where T : Object;
        Task<T> LoadPrefabAsync<T>(object owner, AssetReference reference) where T : Component;

        Task<T> InstantiateAsync<T>(object owner, string primaryKey,
            InstantiationParameters parameters = default);

        Task<T> InstantiateAsync<T>(object owner, AssetReference reference,
            InstantiationParameters parameters = default);

        Task<GameObject> InstantiateAsync(object owner, AssetReference reference,
            InstantiationParameters parameters = default);

        GameObject Instantiate(object owner, AssetReference reference,
            InstantiationParameters parameters = default);

        Task<SceneInstance> LoadSceneAsync(object owner, string key, LoadSceneMode loadSceneMode);
        Task<SceneInstance> LoadSceneAsync(object owner, AssetReference reference, LoadSceneMode loadSceneMode);
        void UnloadScene(object owner, string primaryKey, SceneInstance sceneInstance);
        Task UnloadScene(object owner, AssetReference reference, SceneInstance sceneInstance);
        void ReleaseInstance(object owner, GameObject instancedObject);
        void Release(object owner, string primaryKey);
        void Release(object owner, AssetReference assetReference);
        void ReleaseAll();
    }
}