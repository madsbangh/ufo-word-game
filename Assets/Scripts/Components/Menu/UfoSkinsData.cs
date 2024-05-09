using System;
using System.Collections.Generic;
using UnityEngine;

namespace Components.Menu
{
    [CreateAssetMenu]
    public class UfoSkinsData : ScriptableObject
    {
        [Serializable]
        public class UfoSkinData
        {
            [SerializeField]
            private GameObject _ufoIconPrefab;
            [SerializeField]
            private string _description;

            public GameObject UfoIconPrefab => _ufoIconPrefab;

            public string Description => _description;
        }

        [SerializeField] private List<UfoSkinData> _skins;

        public IReadOnlyList<UfoSkinData> Skins => _skins.AsReadOnly();
    }
}