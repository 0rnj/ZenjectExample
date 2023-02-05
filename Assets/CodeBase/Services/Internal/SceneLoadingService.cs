using System;
using CodeBase.Services.Public;
using UnityEngine.SceneManagement;

namespace CodeBase.Services.Internal
{
    public class SceneLoadingService : ISceneLoadingService, IDisposable
    {
        public SceneLoadingService()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        public void Dispose()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        public void LoadScene(string name)
        {
            SceneManager.LoadScene(name, LoadSceneMode.Single);
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            //
        }
    }
}