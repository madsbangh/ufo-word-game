using TMPro;
using UnityEngine;

namespace Components.Menu
{
    public class UpgradesMenu : MonoBehaviour
    {
        [SerializeField] private TMP_Text _scoreLabel;
        [SerializeField] private GameController _gameController;

        private void OnEnable()
        {
            _scoreLabel.text = _gameController.ReadOnlyGameState.Score.ToString();
            _gameController.ReadOnlyGameState.ScoreChanged += ReadOnlyGameState_BonusHintPointsChanged;
        }

        private void OnDisable()
        {
            _gameController.ReadOnlyGameState.ScoreChanged -= ReadOnlyGameState_BonusHintPointsChanged;
        }

        private void ReadOnlyGameState_BonusHintPointsChanged(int hintPointCount)
        {
            _scoreLabel.text = hintPointCount.ToString();
        }
    }
}