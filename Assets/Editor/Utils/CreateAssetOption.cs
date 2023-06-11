using System.IO;
using UnityEditor;
using UnityEngine;

namespace Editor.Utils {
  public static class CreateAssetOption {
    [MenuItem("Assets/Create Asset")]
    private static void CustomOption() {
      var selectedObject = Selection.activeObject;
      if (selectedObject is not MonoScript script) {
        return;
      }

      var scriptPath = AssetDatabase.GetAssetPath(script);
      var scriptFolderPath = Path.GetDirectoryName(scriptPath);
      var scriptName = Path.GetFileNameWithoutExtension(scriptPath);
      var scriptableObjectPath = $"{scriptFolderPath}/{scriptName}.asset";

      var scriptableObject = ScriptableObject.CreateInstance(script.GetClass());
      AssetDatabase.CreateAsset(scriptableObject, scriptableObjectPath);
      AssetDatabase.SaveAssets();

      EditorUtility.FocusProjectWindow();
      Selection.activeObject = scriptableObject;
    }

    [MenuItem("Assets/Create Asset", true)]
    private static bool CustomOptionValidate() {
      var selectedObject = Selection.activeObject;
      if (selectedObject is not MonoScript script) {
        return false;
      }

      return script.GetClass().IsSubclassOf(typeof(ScriptableObject));
    }
  }
}
