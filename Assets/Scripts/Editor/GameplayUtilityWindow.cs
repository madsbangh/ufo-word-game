using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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