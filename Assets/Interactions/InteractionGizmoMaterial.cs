using TMPro;
using UnityEngine;

namespace Interactions {
  public class InteractionGizmoMaterial : ScriptableObject {
    private static readonly int _atlasTexID = Shader.PropertyToID("_AtlasTex");
    private static readonly int _dialogueRectID =
      Shader.PropertyToID("_DialogueRect");
    private static readonly int _skipRectID = Shader.PropertyToID("_SkipRect");
    private static readonly int _playRectID = Shader.PropertyToID("_PlayRect");
    private static readonly int _cancelRectID =
      Shader.PropertyToID("_CancelRect");

    private const char _dialogueIcon = '\ue0bf';
    private const char _skipIcon = '\ue044';
    private const char _playIcon = '\ue037';
    private const char _cancelIcon = '\ue5cd';

    [SerializeField] private TMP_FontAsset _font;
    [SerializeField] private Material _material;

    [ContextMenu("Update")]
    private void OnValidate() {
      if (_font == null || _material == null) {
        return;
      }

      _material.SetTexture(_atlasTexID, _font.atlasTexture);

      SetRect(_dialogueRectID, _dialogueIcon);
      SetRect(_skipRectID, _skipIcon);
      SetRect(_playRectID, _playIcon);
      SetRect(_cancelRectID, _cancelIcon);
    }

    private void SetRect(int id, char icon) {
      if (!_font.characterLookupTable.TryGetValue(icon, out var character)) {
        return;
      }

      var width = (float)_font.atlasTexture.width;
      var height = (float)_font.atlasTexture.height;
      var glyph = character.glyph;

      _material.SetVector(
        id,
        new Vector4(
          glyph.glyphRect.x / width,
          glyph.glyphRect.y / height,
          glyph.glyphRect.width / width,
          glyph.glyphRect.height / height
        )
      );
    }
  }
}
