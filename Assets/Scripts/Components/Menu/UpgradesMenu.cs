using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Components.Menu
{

    public class UpgradesMenu : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameController _gameController;
        [SerializeField] private TMP_Text _scoreLabel;
        [SerializeField] private Button _previousButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _selectButton;
        [SerializeField] private Button _unlockButton;
        [SerializeField] private RectTransform _ufo;
        [SerializeField] private RectTransform _ufoParent;
        [SerializeField] private GameObject _ufoPriceTagPrefab;
        [SerializeField] private TMP_Text _description;
        [SerializeField] private UfoSkinsData _ufoSkins;

        [Header("Settings")]
        [SerializeField] private float _animationSpeed;
        [SerializeField] private float _edgeNudgeDistance;
        [SerializeField] private float _selectedScaleFactor;

        private int _viewedUfoSkinIndex;

        private void OnEnable()
        {
            Assert.IsNotNull(_scoreLabel);
            Assert.IsNotNull(_gameController);
            Assert.IsNotNull(_previousButton);
            Assert.IsNotNull(_nextButton);
            Assert.IsNotNull(_ufo);
            Assert.IsNotNull(_ufoParent);
            Assert.IsNotNull(_ufoPriceTagPrefab);
            Assert.IsNotNull(_ufoSkins);
            Assert.AreNotEqual(_animationSpeed, 0f);
            
            _scoreLabel.text = _gameController.ReadOnlyGameState.Score.ToString();
            _gameController.ReadOnlyGameState.Score.Changed += ReadOnlyGameState_ScoreChanged;
            _previousButton.onClick.AddListener(Previous_Clicked);
            _nextButton.onClick.AddListener(Next_Clicked);
            _selectButton.onClick.AddListener(Select_Clicked);
            _unlockButton.onClick.AddListener(Unlock_Clicked);
            
            _previousButton.interactable = true;
            _nextButton.interactable = true;
            _ufo.anchoredPosition = Vector2.zero;

            _viewedUfoSkinIndex = _gameController.ReadOnlyGameState.SelectedUfoIndex.Value;
            SetUfoIconButtonsAndDescription();
        }

        private void OnDisable()
        {
            _gameController.ReadOnlyGameState.Score.Changed -= ReadOnlyGameState_ScoreChanged;
            _previousButton.onClick.RemoveListener(Previous_Clicked);
            _nextButton.onClick.RemoveListener(Next_Clicked);
            _selectButton.onClick.RemoveListener(Select_Clicked);
            _unlockButton.onClick.RemoveListener(Unlock_Clicked);
        }

        private void ReadOnlyGameState_ScoreChanged(int previousScore, int newScore)
        {
            _scoreLabel.text = newScore.ToString();
        }

        private void Previous_Clicked()
        {
            if (_viewedUfoSkinIndex == 0)
            {
                StartCoroutine(AnimateEdgeNudgeCoroutine(_edgeNudgeDistance));
            }
            else
            {
                _viewedUfoSkinIndex--;
                StartCoroutine(AnimateToAdjacentUfoCoroutine(GetComponent<RectTransform>().rect.width));
            }
        }

        private void Next_Clicked()
        {
            if (_viewedUfoSkinIndex == _ufoSkins.Skins.Count - 1)
            {
                StartCoroutine(AnimateEdgeNudgeCoroutine(-_edgeNudgeDistance));
            }
            else
            {
                _viewedUfoSkinIndex++;
                StartCoroutine(AnimateToAdjacentUfoCoroutine(-GetComponent<RectTransform>().rect.width));
            }
        }

        private void Unlock_Clicked()
        {
            _gameController.UnlockUfoSkin(_viewedUfoSkinIndex, _ufoSkins.Skins[_viewedUfoSkinIndex].Price);
            _selectButton.gameObject.SetActive(true);
            _unlockButton.gameObject.SetActive(false);
            SetUfoIconButtonsAndDescription();
        }

        private void Select_Clicked()
        {
            _gameController.SelectUfoSkin(_viewedUfoSkinIndex);
            _selectButton.interactable = false;
        }

        private IEnumerator AnimateEdgeNudgeCoroutine(float horizontalDistance)
        {
            _previousButton.interactable = false;
            _nextButton.interactable = false;
            for (float t = 0f; t < Mathf.PI * 2f; t += Time.deltaTime * _animationSpeed)
            {
                var offset = (0.5f - 0.5f * Mathf.Cos(t)) * horizontalDistance;
                _ufo.anchoredPosition = Vector2.right * offset;
                yield return null;
            }
            _ufo.anchoredPosition = Vector2.zero;

            _previousButton.interactable = true;
            _nextButton.interactable = true;
        }

        private IEnumerator AnimateToAdjacentUfoCoroutine(float horizontalDistance)
        {
            _previousButton.interactable = false;
            _nextButton.interactable = false;

            for (float t = 0f; t < Mathf.PI * 2f; t += Time.deltaTime * _animationSpeed)
            {
                var offset = (0.5f - 0.5f * Mathf.Cos(t * 0.5f)) * horizontalDistance;
                _ufo.anchoredPosition = Vector2.right * offset;
                yield return null;
            }

            SetUfoIconButtonsAndDescription();

            for (float t = 0f; t < Mathf.PI * 2f; t += Time.deltaTime * _animationSpeed)
            {
                var offset = (-0.5f + -0.5f * Mathf.Cos(t * 0.5f)) * horizontalDistance;
                _ufo.anchoredPosition = Vector2.right * offset;
                yield return null;
            }

            _ufo.anchoredPosition = Vector2.zero;

            _previousButton.interactable = true;
            _nextButton.interactable = true;
        }

        private void SetUfoIconButtonsAndDescription()
        {
            Destroy(_ufo.gameObject);
            var skin = _ufoSkins.Skins[_viewedUfoSkinIndex];
            var ufoIconPrefab = skin.UfoIconPrefab;
            _ufo = Instantiate(ufoIconPrefab, _ufoParent).GetComponent<RectTransform>();
            _description.text = skin.Description;

            var score = _gameController.ReadOnlyGameState.Score.Value;
            var isSelectedUfoUnlocked = _gameController.ReadOnlyGameState.UnlockedUfoIndices.Contains(_viewedUfoSkinIndex);
            _unlockButton.gameObject.SetActive(!isSelectedUfoUnlocked);
            _unlockButton.interactable = score >= skin.Price;
            _selectButton.gameObject.SetActive(isSelectedUfoUnlocked);
            _selectButton.interactable = _viewedUfoSkinIndex != _gameController.ReadOnlyGameState.SelectedUfoIndex.Value;
            
            if (!isSelectedUfoUnlocked)
            {
                var priceTag = Instantiate(_ufoPriceTagPrefab, _ufo);
                var priceText = priceTag.GetComponentInChildren<TMP_Text>();
                priceText.text = skin.Price.ToString();
            }
        }

        public void Refresh()
        {
            SetUfoIconButtonsAndDescription();
        }
    }
}