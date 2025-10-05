using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.Scripts.Components;
using Components;
using GameStateAndData;
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
                FindAnyObjectByType<GameController>().DebugCompleteOneWord();
            }

            if (GUILayout.Button("Complete Section"))
            {
                FindAnyObjectByType<GameController>().DebugCompleteSection();
            }

            if (GUILayout.Button("Give a Hint Point"))
            {
                FindAnyObjectByType<GameController>().DebugGiveHint();
            }
            
            if (GUILayout.Button("Give a Score Point"))
            {
                FindAnyObjectByType<GameController>().DebugGiveScore();
            }

            if (GUILayout.Button("Hide Tutorial"))
            {
                FindAnyObjectByType<SpellWordTutorial>().Hide();
            }
            if (GUILayout.Button($"Play Tutorial for \"{_tutorialWord}\""))
            {
                FindAnyObjectByType<SpellWordTutorial>().Show(_tutorialWord);
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
                var gameState = gameStateField?.GetValue(FindAnyObjectByType<GameController>());
                var recentlyFoundWordsField = gameState?.GetType().GetField("RecentlyFoundWords");
                var recentlyFoundWords = (StringQueueGameDataField) recentlyFoundWordsField?.GetValue(gameState);

                if (recentlyFoundWords != null)
                {
                    var duplicateWords = new HashSet<string>(recentlyFoundWords.Items
                        .GroupBy(word => word)
                        .Where(group => group.Count() > 1)
                        .Select(group => group.Key));

                    foreach (var text in recentlyFoundWords.Items
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