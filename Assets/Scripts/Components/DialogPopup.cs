using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Components
{
    public class DialogPopup : MonoBehaviour
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _message;
        [SerializeField] private Button _buttonLeft;
        [SerializeField] private Button _buttonMid;
        [SerializeField] private Button _buttonRight;
        [SerializeField] private TMP_Text _buttonLeftText;
        [SerializeField] private TMP_Text _buttonMidText;
        [SerializeField] private TMP_Text _buttonRightText;
        [SerializeField] private GameObject _background;
        [SerializeField] private GameObject _dialogPanel;

        private UnityAction _buttonLeftHandler;
        private UnityAction _buttonMidHandler;
        private UnityAction _buttonRightHandler;

        private void Awake()
        {
            _buttonLeft.onClick.AddListener(ButtonLeftClicked);
            _buttonMid.onClick.AddListener(ButtonMidClicked);
            _buttonRight.onClick.AddListener(ButtonRightClicked);
            _background.SetActive(false);
            _dialogPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            _buttonLeft.onClick.RemoveListener(ButtonLeftClicked);
            _buttonMid.onClick.RemoveListener(ButtonMidClicked);
            _buttonRight.onClick.RemoveListener(ButtonRightClicked);
        }

        private void ButtonLeftClicked()
        {
            _background.SetActive(false);
            _dialogPanel.SetActive(false);
            _buttonLeftHandler?.Invoke();
        }
        
        private void ButtonMidClicked()
        {
            _background.SetActive(false);
            _dialogPanel.SetActive(false);
            _buttonMidHandler?.Invoke();
        }

        private void ButtonRightClicked()
        {
            _background.SetActive(false);
            _dialogPanel.SetActive(false);
            _buttonRightHandler?.Invoke();
        }

        public void Show(
            string title,
            string message,
            (string Label, UnityAction Action) primaryButton,
            (string Label, UnityAction Action) secondaryButton)
        {
            _title.text = title;
            _message.text = message;
            
            _buttonLeft.gameObject.SetActive(true);
            _buttonLeftText.text = primaryButton.Label;
            _buttonLeftHandler = primaryButton.Action;
                
            _buttonMid.gameObject.SetActive(false);
            _buttonMidHandler = null;
                
            _buttonRight.gameObject.SetActive(true);
            _buttonRightText.text = secondaryButton.Label;
            _buttonRightHandler = secondaryButton.Action;
            
            _background.SetActive(true);
            _dialogPanel.SetActive(true);
        }
        
        public void Show(
            string title,
            string message,
            (string Label, UnityAction Action) button)
        {
            _title.text = title;
            _message.text = message;
            
            _buttonLeft.gameObject.SetActive(false);
            _buttonLeftHandler = null;
                
            _buttonMid.gameObject.SetActive(true);
            _buttonMidText.text = button.Label;
            _buttonMidHandler = button.Action;
                
            _buttonRight.gameObject.SetActive(false);
            _buttonRightHandler = null;
            
            _background.SetActive(true);
            _dialogPanel.SetActive(true);
        }
    }
}