using TMPro;
using UnityEngine;

namespace Interactions {
  public class InteractionGizmoMaterial : ScriptableObject {
    private static readonly int _atlasTexID = Shader.PropertyToID("_AtlasTex");
    private static readonly int _ltRectID = Shader.PropertyToID("_LTRect");
    private static readonly int _rtRectID = Shader.PropertyToID("_RTRect");
    private static readonly int _bothRectID = Shader.PropertyToID("_BothRect");

    private const char _leftIcon = '\ue0ca';
    private const char _rightIcon = '\ue253';
    private const char _bothIcon = '\ue0bf';

    [SerializeField] private TMP_FontAsset _font;
    [SerializeField] private Material _material;

    [ContextMenu("Update")]
    private void OnValidate() {
      if (_font == null || _material == null) {
        return;
      }

      _material.SetTexture(_atlasTexID, _font.atlasTexture);
      SetRect(_ltRectID, _leftIcon);
      SetRect(_rtRectID, _rightIcon);
      SetRect(_bothRectID, _bothIcon);
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
