using Aarthificial.Typewriter;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Editor.Typewriter {
  [ScriptedImporter(4, "tw")]
  public class CsvImporter : ScriptedImporter {
    [SerializeField] internal DatabaseTable Table;

    public override void OnImportAsset(AssetImportContext ctx) { }
  }
}
