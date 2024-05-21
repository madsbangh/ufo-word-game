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
            private GameObject _ufoBodyPrefab;
            [SerializeField]
            private string _description;
            [SerializeField]
            private int _price;

            public GameObject UfoIconPrefab => _ufoIconPrefab;
            
            public GameObject UfoBodyPrefab => _ufoBodyPrefab;

            public string Description => _description;
            
            public int Price => _price;
        }

        [SerializeField] private List<UfoSkinData> _skins;

        public IReadOnlyList<UfoSkinData> Skins => _skins.AsReadOnly();
    }
}