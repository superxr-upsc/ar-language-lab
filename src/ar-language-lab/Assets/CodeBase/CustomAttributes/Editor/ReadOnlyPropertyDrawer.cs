using CodeBase.CustomAttributes;
using UnityEditor;
using UnityEngine;

namespace Codebase.Systems.CustomAttributes.Editor
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var defaultColor = GUI.color;
            GUI.color = new Color(0.45f, 1f, 0.51f);
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label);
            GUI.enabled = true;
            GUI.color = defaultColor;
        }
    }
}