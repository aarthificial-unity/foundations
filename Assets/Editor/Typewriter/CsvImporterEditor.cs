using Aarthificial.Typewriter;
using System.IO;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Editor.Typewriter {
  [CustomEditor(typeof(CsvImporter))]
  public class CsvImporterEditor : ScriptedImporterEditor {
    public override void OnInspectorGUI() {
      serializedObject.Update();
      var property = serializedObject.FindProperty(nameof(CsvImporter.Table));

      EditorGUILayout.PropertyField(property);
      EditorGUI.BeginDisabledGroup(property.objectReferenceValue == null);
      EditorGUILayout.BeginHorizontal();
      GUILayout.FlexibleSpace();
      if (GUILayout.Button("Convert")) {
        CsvConverter.Convert(
          (DatabaseTable)property.objectReferenceValue,
          File.ReadAllText(((CsvImporter)target).assetPath),
          target
        );
      }
      EditorGUILayout.EndHorizontal();
      EditorGUI.EndDisabledGroup();

      serializedObject.ApplyModifiedProperties();
      ApplyRevertGUI();
    }
  }
}
