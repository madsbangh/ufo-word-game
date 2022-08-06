using System.Collections;
using System.Reflection;
using Audio;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomPropertyDrawer(typeof(AudioClipsPlayer))]
    public class AudioClipsPlayerDrawer : PropertyDrawer
    {
        private const int Margin = 15;
        private const int Padding = 15;
        
        private AudioClipsPlayer _cachedPlayer;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true) + 
                   (property.isExpanded ? Margin + Padding : 0);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.isExpanded)
            {
                position.yMax -= Margin;
            }

            GUI.Box(position, (string)null);
            
            var buttonPosition = position;
            buttonPosition.xMin = position.xMax - 150;
            buttonPosition.yMax = position.yMin + EditorGUIUtility.singleLineHeight;
            
            if (GUI.Button(buttonPosition, "Play"))
            {
                EnsureCachedPlayerSetup(property);
                _cachedPlayer.Play();
                Event.current.Use();
            }

            var newLabel = new GUIContent($"{label.text} ({nameof(AudioClipsPlayer)})", label.tooltip);
            EditorGUI.PropertyField(position, property, newLabel, true);
        }

        private void EnsureCachedPlayerSetup(SerializedProperty property)
        {
            if (_cachedPlayer == null)
            {
                _cachedPlayer = (AudioClipsPlayer) fieldInfo.GetValue(property.serializedObject.targetObject);
                EditorCoroutineUtility.StartCoroutine(UpdatePlayerEditorCoroutine(_cachedPlayer),
                    property.serializedObject.targetObject);
            }
        }

        private IEnumerator UpdatePlayerEditorCoroutine(AudioClipsPlayer player)
        {
            while (true)
            {
                player.Update();
                yield return null;
            }
        }
    }
}