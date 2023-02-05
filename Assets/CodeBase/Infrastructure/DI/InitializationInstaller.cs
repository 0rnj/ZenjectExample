using CodeBase.Services.Internal;
using CodeBase.Services.Internal.State;
using CodeBase.Services.Public;
using CodeBase.Services.Public.State;
using CodeBase.StaticData;
using UnityEngine;
using Zenject;

namespace CodeBase.Infrastructure.DI
{
    public class InitializationInstaller : MonoInstaller
    {
        [SerializeField] private CommonSettings _commonSettings;

        public override void InstallBindings()
        {
            BindPlayerStateService();
            BindAddressablesService();
            BindSceneLoadingService();
            BindCommonSettings();
        }

        private void BindPlayerStateService() =>
            Container
                .Bind<IPlayerStateService>()
                .To<PlayerStateService>()
                .AsSingle();

        private void BindAddressablesService() =>
            Container
                .Bind<IAddressablesService>()
                .To<AddressablesService>()
                .AsSingle()
                .OnInstantiated<AddressablesService>((_, o) => o.Initialize());

        private void BindSceneLoadingService() =>
            Container
                .Bind<ISceneLoadingService>()
                .To<SceneLoadingService>()
                .AsSingle();

        private void BindCommonSettings() =>
            Container
                .Bind<CommonSettings>()
                .FromScriptableObject(_commonSettings)
                .AsSingle();
    }
}