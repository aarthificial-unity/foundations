using UnityEditor;
using UnityEngine;
using Utils;

namespace Editor.Utils
{
  [CustomPropertyDrawer(typeof(ScenePathAttribute))]
  public class ScenePathPropertyDrawer : PropertyDrawer
  {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
      EditorGUI.BeginProperty(position, label, property);

      var oldScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(property.stringValue);
      EditorGUI.BeginChangeCheck();
      var newScene = EditorGUI.ObjectField(position, label, oldScene, typeof(SceneAsset), false) as SceneAsset;
      if (EditorGUI.EndChangeCheck()) {
        var newPath = AssetDatabase.GetAssetPath(newScene);
        property.stringValue = newPath;
      }

      EditorGUI.EndProperty();
    }
  }
}
