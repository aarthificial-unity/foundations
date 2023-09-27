using UnityEditor;
using UnityEngine;
using Utils;

namespace Editor.Utils {
  public abstract class WrapperPropertyDrawer : PropertyDrawer {
    protected virtual string WrappedName { get; }

    public override void OnGUI(
      Rect position,
      SerializedProperty property,
      GUIContent label
    ) {
      var wrappedProperty = property.FindPropertyRelative(WrappedName);
      EditorGUI.PropertyField(position, wrappedProperty, label, true);
    }
  }
}
