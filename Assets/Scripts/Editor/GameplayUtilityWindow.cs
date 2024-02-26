using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.Scripts.Components;
using Components;
using SaveGame;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor
{
    public class GameplayUtilityWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private string _tutorialWord;

        [MenuItem("Word Invader/Gameplay Utilities...")]
        public static void ShowWindow()
        {
            GetWindow<GameplayUtilityWindow>("Gameplay Utilities");
        }

        private void OnGUI()
        {
            GUI.enabled = EditorApplication.isPlaying;

            if (GUILayout.Button("Complete One Word"))
            {
                FindObjectOfType<GameController>().DebugCompleteOneWord();
            }

            if (GUILayout.Button("Complete Section"))
            {
                FindObjectOfType<GameController>().DebugCompleteSection();
            }

            if (GUILayout.Button("Give a Hint Point"))
            {
                FindObjectOfType<GameController>().DebugGiveHint();
            }

            if (GUILayout.Button("Hide Tutorial"))
            {
                FindObjectOfType<SpellWordTutorial>().Hide();
            }
            if (GUILayout.Button($"Play Tutorial for \"{_tutorialWord}\""))
            {
                FindObjectOfType<SpellWordTutorial>().Show(_tutorialWord);
            }
            _tutorialWord = EditorGUILayout.TextField(_tutorialWord)?.ToUpper();

            GUI.enabled = true;

            if (GUILayout.Button("Delete Save File"))
            {
                SaveGameUtility.DeleteSaveFile();
            }

            EditorGUILayout.LabelField("Recent Words");
            
            using var scrollView = new GUILayout.ScrollViewScope(_scrollPosition);
            _scrollPosition = scrollView.scrollPosition;

            if (EditorApplication.isPlaying)
            {
                var gameStateField =
                    typeof(GameController).GetField("_gameState", BindingFlags.Instance | BindingFlags.NonPublic);
                var gameState = gameStateField?.GetValue(FindObjectOfType<GameController>());
                var recentlyFoundWordsField = gameState?.GetType().GetField("RecentlyFoundWords");
                var recentlyFoundWords = (Queue<string>) recentlyFoundWordsField?.GetValue(gameState);

                if (recentlyFoundWords != null)
                {
                    var duplicateWords = new HashSet<string>(recentlyFoundWords
                        .GroupBy(word => word)
                        .Where(group => group.Count() > 1)
                        .Select(group => group.Key));

                    foreach (var text in recentlyFoundWords
                        .Select(word =>
                            duplicateWords.Contains(word)
                                ? $"{word} (duplicate)"
                                : word))
                    {
                        EditorGUILayout.LabelField(text);
                    }
                }
            }
        }
    }
}