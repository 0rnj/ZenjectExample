using System;
using CodeBase.Services.Public;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace CodeBase.UI
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private Button _startButton;
        
        private ISceneLoadingService _sceneLoadingService;

        [Inject]
        private void Construct(ISceneLoadingService sceneLoadingService)
        {
            _sceneLoadingService = sceneLoadingService;
        }

        private void OnEnable()
        {
            _startButton.onClick.AddListener(LoadBattle);
        }

        private void OnDisable()
        {
            _startButton.onClick.RemoveAllListeners();
        }

        private void LoadBattle()
        {
            _sceneLoadingService.LoadScene("Battle");
        }
    }
}