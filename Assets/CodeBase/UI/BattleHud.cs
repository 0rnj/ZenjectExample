using CodeBase.Services.Public;
using CodeBase.StaticData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace CodeBase.UI
{
    public class BattleHud : MonoBehaviour
    {
        [SerializeField] private Button _exitButton;
        [SerializeField] private TextMeshProUGUI _hpText;

        private ISceneLoadingService _sceneLoadingService;
        private CommonSettings _commonSettings;

        [Inject]
        private void Construct(CommonSettings commonSettings, ISceneLoadingService sceneLoadingService)
        {
            _sceneLoadingService = sceneLoadingService;
            _commonSettings = commonSettings;
        }

        private void OnEnable()
        {
            _exitButton.onClick.AddListener(GoToMainMenu);
        }

        private void OnDisable()
        {
            _exitButton.onClick.RemoveAllListeners();
        }

        private void Update()
        {
            UpdateHealth();
        }

        private void GoToMainMenu()
        {
            _sceneLoadingService.LoadScene("MainMenu");
        }

        private void UpdateHealth()
        {
            var current = 100;

            _hpText.text = $"{current}/{_commonSettings.MainHeroSettings.MaxHP}";
        }
    }
}