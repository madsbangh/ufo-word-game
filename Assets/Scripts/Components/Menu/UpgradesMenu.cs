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
        [SerializeField] private TMP_Text _description;
        [SerializeField] private UfoSkinsData _ufoSkins;

        [Header("Settings")]
        [SerializeField] private float _animationSpeed;
        [SerializeField] private float _edgeNudgeDistance;
        [SerializeField] private float _selectedScaleFactor;

        private int _selectedUfoSkinIndex;

        private void OnEnable()
        {
            Assert.IsNotNull(_scoreLabel);
            Assert.IsNotNull(_gameController);
            Assert.IsNotNull(_previousButton);
            Assert.IsNotNull(_nextButton);
            Assert.IsNotNull(_ufo);
            Assert.IsNotNull(_ufoParent);
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
            if (_selectedUfoSkinIndex == 0)
            {
                StartCoroutine(AnimateEdgeNudgeCoroutine(_edgeNudgeDistance));
            }
            else
            {
                _selectedUfoSkinIndex--;
                StartCoroutine(AnimateToAdjacentUfoCoroutine(GetComponent<RectTransform>().rect.width));
            }
        }

        private void Next_Clicked()
        {
            if (_selectedUfoSkinIndex == _ufoSkins.Skins.Count - 1)
            {
                StartCoroutine(AnimateEdgeNudgeCoroutine(-_edgeNudgeDistance));
            }
            else
            {
                _selectedUfoSkinIndex++;
                StartCoroutine(AnimateToAdjacentUfoCoroutine(-GetComponent<RectTransform>().rect.width));
            }
        }

        private void Unlock_Clicked()
        {
            _gameController.UnlockUfoSkin(_selectedUfoSkinIndex);
            _selectButton.gameObject.SetActive(true);
            _unlockButton.gameObject.SetActive(false);
        }

        private void Select_Clicked()
        {
            _gameController.SelectUfoSkin(_selectedUfoSkinIndex);
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
            var ufoIconPrefab = _ufoSkins.Skins[_selectedUfoSkinIndex].UfoIconPrefab;
            _ufo = Instantiate(ufoIconPrefab, _ufoParent).GetComponent<RectTransform>();
            _description.text = _ufoSkins.Skins[_selectedUfoSkinIndex].Description;
            var isSelectedUfoUnlocked = _gameController.ReadOnlyGameState.UnlockedUfoIndices.Contains(_selectedUfoSkinIndex);
            _unlockButton.gameObject.SetActive(!isSelectedUfoUnlocked);
            _selectButton.gameObject.SetActive(isSelectedUfoUnlocked);
            _selectButton.interactable = _selectedUfoSkinIndex != _gameController.ReadOnlyGameState.SelectedUfoIndex.Value;
        }
    }
}