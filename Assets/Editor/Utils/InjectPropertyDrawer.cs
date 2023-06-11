using UnityEditor;
using UnityEngine;
using Utils;

namespace Editor.Utils {
  [CustomPropertyDrawer(typeof(InjectAttribute))]
  public class InjectPropertyDrawer : PropertyDrawer {
    private bool _hasSearched;

    public override void OnGUI(
      Rect position,
      SerializedProperty property,
      GUIContent label
    ) {
      if (!_hasSearched && property.objectReferenceValue == null) {
        _hasSearched = true;

        if (typeof(MonoBehaviour).IsAssignableFrom(fieldInfo.FieldType)
          && property.serializedObject
            .targetObject is MonoBehaviour component) {
          var suitableComponents =
            component.GetComponentsInChildren(fieldInfo.FieldType);
          if (suitableComponents.Length > 0) {
            property.objectReferenceValue = suitableComponents[0];
          }
        } else if (typeof(ScriptableObject).IsAssignableFrom(
            fieldInfo.FieldType
          )) {
          string[] suitableAssets =
            AssetDatabase.FindAssets("t:" + fieldInfo.FieldType);
          if (suitableAssets.Length > 0) {
            property.objectReferenceValue = AssetDatabase.LoadMainAssetAtPath(
              AssetDatabase.GUIDToAssetPath(suitableAssets[0])
            );
          }
        }
      }

      EditorGUI.BeginProperty(position, label, property);
      EditorGUI.PropertyField(position, property, label);
      EditorGUI.EndProperty();
    }
  }
}
