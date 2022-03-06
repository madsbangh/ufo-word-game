using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Components.Menu
{
    public class MenuController : MonoBehaviour
    {
        private static readonly int ShowPropertyId = Animator.StringToHash("Show");
        
        [SerializeField]
        private MenuButton _menuButton;

        [SerializeField]
        private Animator _animator;

        private bool _isMenuCurrentlyShown;

        private void OnEnable()
        {
            _menuButton.OnClick.AddListener(MenuButton_Clicked);
        }

        private void OnDisable()
        {
            _menuButton.OnClick.RemoveListener(MenuButton_Clicked);
        }

        private void MenuButton_Clicked()
        {
            _isMenuCurrentlyShown = !_isMenuCurrentlyShown;
            _animator.SetBool(ShowPropertyId, _isMenuCurrentlyShown);
            _menuButton.IsX = _isMenuCurrentlyShown;
        }
    }
}