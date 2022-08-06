using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Components.Menu
{
    public class MenuController : MonoBehaviour
    {
        private const string GitHubURL = "https://github.com/madsbangh/ufo-word-game";
        private const string AppNameTemplate = "{app-name}";
        private const string AppVersionTemplate = "{app-version}";
        
        private static readonly int ShowPropertyId = Animator.StringToHash("Show");

        [Header("In Scene")]
        [SerializeField] private MenuButton _menuButton;
        [SerializeField] private Animator _animator;
        
        [Header("Main Menu")]
        [SerializeField] private GameObject _mainMenu;
        [SerializeField] private Button _aboutButton, _privacyButton, _settingsButton;

        [Header("About Menu")]
        [SerializeField] private GameObject _aboutMenu;
        [SerializeField] private Button _gitHubButton;
        [SerializeField] private TMP_Text _aboutText;

        [Header("Privacy Menu")]
        [SerializeField] private GameObject _privacyMenu;

        [Header("Settings Menu")]
        [SerializeField] private GameObject _settingsMenu;
        [SerializeField] private GameObject _resetGameConfirmationDialog;
        [SerializeField] private Button _resetGameButton, _yesButton, _cancelButton;

        private State _state;

        private enum State
        {
            NoMenu,
            MainMenu,
            About,
            Privacy,
            Settings,
        }

        private void Awake()
        {
            _aboutText.text = _aboutText.text
                .Replace(AppNameTemplate, Application.productName)
                .Replace(AppVersionTemplate, Application.version);
        }

        private void OnEnable()
        {
            _state = State.NoMenu;
            _animator.SetBool(ShowPropertyId, false);
            _menuButton.IsX = false;
            _mainMenu.SetActive(true);
            _aboutMenu.SetActive(false);
            _privacyMenu.SetActive(false);
            _settingsMenu.SetActive(false);
            _menuButton.OnClick.AddListener(MenuButton_Clicked);
            _aboutButton.onClick.AddListener(AboutButton_Clicked);
            _privacyButton.onClick.AddListener(PrivacyButton_Clicked);
            _gitHubButton.onClick.AddListener(GitHubButton_Clicked);
            _settingsButton.onClick.AddListener(SettingsButton_Clicked);
            _resetGameButton.onClick.AddListener(ResetGameButton_Clicked);
            _yesButton.onClick.AddListener(YesButton_Clicked);
            _cancelButton.onClick.AddListener(CancelButton_Clicked);
        }

        private void OnDisable()
        {
            _menuButton.OnClick.RemoveListener(MenuButton_Clicked);
            _aboutButton.onClick.RemoveListener(AboutButton_Clicked);
            _privacyButton.onClick.RemoveListener(PrivacyButton_Clicked);
            _gitHubButton.onClick.RemoveListener(GitHubButton_Clicked);
            _settingsButton.onClick.RemoveListener(SettingsButton_Clicked);
            _resetGameButton.onClick.RemoveListener(ResetGameButton_Clicked);
            _yesButton.onClick.RemoveListener(YesButton_Clicked);
            _cancelButton.onClick.RemoveListener(CancelButton_Clicked);
        }

        private void MenuButton_Clicked()
        {
            switch (_state)
            {
                case State.NoMenu:
                    _state = State.MainMenu;
                    _animator.SetBool(ShowPropertyId, true);
                    _menuButton.IsX = true;
                    break;
                case State.About:
                    _state = State.MainMenu;
                    _aboutMenu.SetActive(false);
                    _mainMenu.SetActive(true);
                    break;
                case State.Privacy:
                    _state = State.MainMenu;
                    _privacyMenu.SetActive(false);
                    _mainMenu.SetActive(true);
                    break;
                case State.Settings:
                    _state = State.MainMenu;
                    _settingsMenu.SetActive(false);
                    _mainMenu.SetActive(true);
                    break;
                case State.MainMenu:
                default:
                    _state = State.NoMenu;
                    _animator.SetBool(ShowPropertyId, false);
                    _menuButton.IsX = false;
                    break;
            }
        }

        private void AboutButton_Clicked()
        {
            _state = State.About;
            _mainMenu.SetActive(false);
            _aboutMenu.SetActive(true);
        }

        private void PrivacyButton_Clicked()
        {
            _state = State.Privacy;
            _mainMenu.SetActive(false);
            _privacyMenu.SetActive(true);
        }

        private static void GitHubButton_Clicked()
        {
            Application.OpenURL(GitHubURL);
        }

        private void SettingsButton_Clicked()
        {
            _state = State.Settings;
            _mainMenu.SetActive(false);
            _settingsMenu.SetActive(true);
            _resetGameConfirmationDialog.SetActive(false);
        }

        private void ResetGameButton_Clicked()
        {
            _resetGameConfirmationDialog.SetActive(true);
        }

        private void YesButton_Clicked()
        {
            SaveGame.SaveGameUtility.DeleteSaveFile();
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
		}

        private void CancelButton_Clicked()
        {
            _resetGameConfirmationDialog.SetActive(false);
        }
    }
}