using System;
using Components.Menu;
using UnityEngine;
using UnityEngine.Serialization;

namespace Components
{
    [RequireComponent(typeof(Animator))]
    public class UfoBodySkinView : MonoBehaviour
    {
        [SerializeField] private GameController _gameController;
        [SerializeField] private GameObject _ufoBodyToReplace;
        [FormerlySerializedAs("_skins")] [SerializeField] private UfoSkinsData _skinsData;
        
        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void Start()
        {
            var selectedUfoIndex = _gameController.ReadOnlyGameState.SelectedUfoIndex;
            selectedUfoIndex.Changed += SelectedUfoIndexOnChanged;
            ChangeSkin(selectedUfoIndex.Value);
        }

        private void OnDestroy()
        {
            _gameController.ReadOnlyGameState.SelectedUfoIndex.Changed -= SelectedUfoIndexOnChanged;
        }

        private void SelectedUfoIndexOnChanged(int _, int value)
        {
            ChangeSkin(value);
        }

        private void ChangeSkin(int index)
        {
            var skinData = _skinsData.Skins[index];
            var localPosition = _ufoBodyToReplace.transform.localPosition;
            var localRotation = _ufoBodyToReplace.transform.localRotation;
            Destroy(_ufoBodyToReplace);
            _ufoBodyToReplace = Instantiate(skinData.UfoBodyPrefab, transform);
            _ufoBodyToReplace.transform.SetLocalPositionAndRotation(localPosition, localRotation);
            _ufoBodyToReplace.name = "Body";
            _animator.Rebind();
        }
    }
}
