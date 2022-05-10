using System;
using System.Collections.Generic;
using UnityEngine;

namespace Components
{
    [RequireComponent(typeof(Animator))]
    public class UFOAnimator : MonoBehaviour
    {
        private static readonly int HappyTriggerId = Animator.StringToHash("Happy");
        private static readonly int SadTriggerId = Animator.StringToHash("Sad");
        private static readonly int WinTriggerId = Animator.StringToHash("Win");
        private static readonly int BonusWordFoundTriggerId = Animator.StringToHash("Bonus Word Found");
        private static readonly int WordAlreadyFoundTriggerId = Animator.StringToHash("Word Already Found");

        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public void PlayHappy()
        {
            _animator.SetTrigger(HappyTriggerId);
        }

        public void PlaySad()
        {
            _animator.SetTrigger(SadTriggerId);
        }

        public void PlayWin()
        {
            _animator.SetTrigger(WinTriggerId);
        }

        public void PlayFoundBonusWord()
        {
            _animator.SetTrigger(BonusWordFoundTriggerId);
        }

        public void PlayAlreadyFoundWord()
        {
            _animator.SetTrigger(WordAlreadyFoundTriggerId);
        }
    }
}