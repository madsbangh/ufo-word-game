using System;
using TMPro;
using UnityEngine;

namespace Components
{
    [RequireComponent(typeof(Animator))]
    public class LetterTile : MonoBehaviour
    {
        private static readonly int PingProp = Animator.StringToHash("Ping");
        private static readonly int StateProp = Animator.StringToHash("State");

        [SerializeField]
        private TMP_Text _letter;

        private Animator _animator;

        public char Letter
        {
            get => _letter.text[0];
            set
            {
                _letter.text = value.ToString();
                name = _letter.text;
            }
        }

        public TileState State
        {
            set
            {
                switch (value)
                {
                    case TileState.Locked:
                        _animator.SetInteger(StateProp, 0);
                        break;
                    case TileState.Hidden:
                        _animator.SetInteger(StateProp, 1);
                        _animator.SetTrigger(PingProp);
                        break;
                    case TileState.Revealed:
                        _animator.SetInteger(StateProp, 2);
                        _animator.SetTrigger(PingProp);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(value));
                }
            }
        }

        public Vector2Int Position
        {
            set
            {
                var t = transform;
                t.position = value.ToWorldPosition();
            }
        }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }
    }
}