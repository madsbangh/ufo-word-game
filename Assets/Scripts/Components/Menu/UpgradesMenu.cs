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
        [SerializeField] private TMP_Text _scoreLabel;
        [SerializeField] private GameController _gameController;
        [SerializeField] private Button _previousButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private RectTransform _ufo;
        [SerializeField] private RectTransform _ufoParent;
        [SerializeField] private UfoSkinsData _ufoSkins;

        [Header("Settings")]
        [SerializeField] private float _animationSpeed;
        [SerializeField] private float _edgeNudgeDistance;

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
            _gameController.ReadOnlyGameState.ScoreChanged += ReadOnlyGameState_BonusHintPointsChanged;
            _previousButton.onClick.AddListener(Previous_Clicked);
            _nextButton.onClick.AddListener(Next_Clicked);

            _previousButton.interactable = true;
            _nextButton.interactable = true;
            _ufo.anchoredPosition = Vector2.zero;
        }

        private void OnDisable()
        {
            _gameController.ReadOnlyGameState.ScoreChanged -= ReadOnlyGameState_BonusHintPointsChanged;
            _previousButton.onClick.RemoveListener(Previous_Clicked);
            _nextButton.onClick.RemoveListener(Next_Clicked);
        }

        private void ReadOnlyGameState_BonusHintPointsChanged(int hintPointCount)
        {
            _scoreLabel.text = hintPointCount.ToString();
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
            
            Destroy(_ufo.gameObject);
            GameObject ufoIconPrefab = _ufoSkins.Skins[_selectedUfoSkinIndex].UfoIconPrefab;
            _ufo = Instantiate(ufoIconPrefab, _ufoParent).GetComponent<RectTransform>();
            
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
    }
}