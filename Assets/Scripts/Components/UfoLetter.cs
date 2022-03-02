using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Components
{
    public class UfoLetter : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler,
        IPointerUpHandler
    {
        public event Action<UfoLetter, PointerEventData> Pressed;
        public event Action<UfoLetter, PointerEventData> Released;
        public event Action<UfoLetter, PointerEventData> Entered;

        [SerializeField] private TMP_Text _letter;
        [SerializeField] private SpriteRenderer _sprite;
        [SerializeField] private Color _selectedColor;
        [SerializeField] private Color _normalColor;

        public char Letter
        {
            get => _letter.text[0];
            set => _letter.text = value.ToString();
        }

        public bool Selected
        {
            set => _sprite.color = value ? _selectedColor : _normalColor;
        }

        private void Start()
        {
            Selected = false;
        }

        public void OnPointerEnter(PointerEventData eventData) => Entered?.Invoke(this, eventData);

		public void OnPointerDown(PointerEventData eventData) => Pressed?.Invoke(this, eventData);

		public void OnPointerUp(PointerEventData eventData) => Released?.Invoke(this, eventData);
    }
}