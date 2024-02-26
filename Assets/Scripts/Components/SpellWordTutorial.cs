using Components;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Components
{
    public class SpellWordTutorial : MonoBehaviour
    {
        [SerializeField]
        private Transform _hand;

        [SerializeField]
        private UfoLetterRing _letterRing;

        [SerializeField]
        private float _animationStepDurationSeconds;

        [SerializeField]
        private float _scaleDurationSeconds;

        [SerializeField]
        private float _delayBetweenPlaysSeconds;

        private void Start()
        {
            _hand.localScale = Vector3.zero;
        }

        private void OnEnable()
        {
            GameEvents.BoardWordCompleted += GameEvents_BoardWordCompleted;
        }

        private void OnDisable()
        {
            GameEvents.BoardWordCompleted -= GameEvents_BoardWordCompleted;
        }

        private void GameEvents_BoardWordCompleted(string _)
        {
            Hide();
        }

        /// <summary>
        /// Start animating the hand icon to follow a sequence of letters repeatedly.
        /// </summary>
        public void Show(string word)
        {
            StopAllCoroutines();
            StartCoroutine(SpellWordsRepeatedlyCoroutine(word));
        }

        /// <summary>
        /// Hide the hand icon (gracefully) whereever it is in its sequence.
        /// </summary>
        public void Hide()
        {
            StopAllCoroutines();
            var pos = _hand.position;
            var startScale = _hand.localScale.x;
            StartCoroutine(AnimateHandCoroutline(
                _scaleDurationSeconds,
                pos, pos,
                startScale, 0f));
        }

        private IEnumerator SpellWordsRepeatedlyCoroutine(string word)
        {
            var wait = new WaitForSeconds(_delayBetweenPlaysSeconds);
            while (true)
            {
                yield return SpellWordCoroutine(word);
                yield return wait;
            }
        }

        /// <summary>
        /// Amimation sequence: Scale the hand up from zero,
        /// move it along the letters of the given word in sequence,
        /// then scale it down to zero again.
        /// </summary>
        private IEnumerator SpellWordCoroutine(string word)
        {
            var letters = ToUfoLetters(word).GetEnumerator();

            if (letters.MoveNext())
            {
                var startPos = letters.Current.transform.position;
                yield return AnimateHandCoroutline(_scaleDurationSeconds, startPos, startPos, 0f, 1f);

                while (letters.MoveNext())
                {
                    var fromPos = _hand.position;
                    var toPos = letters.Current.transform.position;
                    yield return AnimateHandCoroutline(_animationStepDurationSeconds, fromPos, toPos, 1f, 1f);
                }

                var endPos = _hand.position;
                yield return AnimateHandCoroutline(_scaleDurationSeconds, endPos, endPos, 1f, 0f);
            }
        }

        /// <summary>
        /// Move the hand icon and scale it over time
        /// </summary>
        private IEnumerator AnimateHandCoroutline(
            float seconds,
            Vector3 fromPos,
            Vector3 toPos,
            float fromScale,
            float toScale)
        {
            if (seconds == 0f)
            {
                _hand.position = toPos;
                _hand.localScale = Vector3.one * toScale;
                yield break;
            }

            for (var elapsed = 0f; elapsed < seconds; elapsed += Time.deltaTime)
            {
                var t = elapsed / seconds;
                _hand.position = Vector3.LerpUnclamped(fromPos, toPos, Mathf.SmoothStep(0f, 1f, t));
                _hand.localScale = Vector3.one * Mathf.SmoothStep(fromScale, toScale, t);
                yield return null;
            }

            _hand.position = toPos;
            _hand.localScale = Vector3.one * toScale;
        }

        /// <summary>
        /// Given a word in string form, return a sequence of letters on the UFO.
        /// It is assumed the given word will be spellable with the current UFO letter ring.
        /// </summary>
        private IEnumerable<UfoLetter> ToUfoLetters(string word)
        {
            var letters = _letterRing.ActiveLetters.ToLookup(l => l.Letter);
            var used = new HashSet<UfoLetter>(word.Length);

            // Let `characters` be the elements in the given word,
            // and `letters` be the letters in the  interactive objects around the UFO
            foreach (var character in word)
            {
                if (letters.Contains(character))
                {
                    // There might be more letters for a given character
                    foreach (var letter in letters[character])
                    {
                        // Skip any used letters
                        if (!used.Contains(letter))
                        {
                            // Mark this letter as used
                            used.Add(letter);
                            // Return the letter
                            yield return letter;
                            // Found a letter in the lookup,
                            // so stop traversing this letter group
                            break;
                        }
                    }
                }
            }
        }
    }
}