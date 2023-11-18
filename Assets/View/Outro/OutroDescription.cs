using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils;

namespace View.Outro {
  public class OutroDescription : MonoBehaviour,
    IPointerMoveHandler,
    IPointerClickHandler {
    [Serializable]
    private struct Link {
      public Color Color;
      public string Url;
    }

    [SerializeField] private SerializedDictionary<string, Link> _links = new();
    private TextMeshProUGUI _text;
    private Cached<string> _hoveredLink;

    private void Awake() {
      _text = GetComponent<TextMeshProUGUI>();
      _text.OnPreRenderText += UpdateMesh;
    }

    private void UpdateMesh(TMP_TextInfo textInfo) {
      foreach (var link in textInfo.linkInfo) {
        var id = link.GetLinkID();
        var hovered = id == _hoveredLink;
        var color = _links[id].Color;
        for (var i = link.linkTextfirstCharacterIndex;
          i < link.linkTextfirstCharacterIndex + link.linkTextLength;
          i++) {
          var charInfo = textInfo.characterInfo[i];
          var materialIndex = charInfo.materialReferenceIndex;
          var newColors = textInfo.meshInfo[materialIndex].colors32;

          for (var j = 0; j < 12; j++) {
            if (charInfo.character == ' ') {
              continue;
            }
            var vertexIndex = charInfo.underlineVertexIndex + j;
            newColors[vertexIndex] = hovered ? color : Color.clear;
          }

          for (var j = 0; j < 4; j++) {
            if (charInfo.character == ' ') {
              continue;
            }
            var vertexIndex = charInfo.vertexIndex + j;
            newColors[vertexIndex] = color;
          }
        }
      }
    }

    public void OnPointerMove(PointerEventData eventData) {
      var linkIndex = TMP_TextUtilities.FindIntersectingLink(
        _text,
        eventData.position,
        Camera.main
      );

      if (_hoveredLink.HasChanged(
          linkIndex == -1
            ? null
            : _text.textInfo.linkInfo[linkIndex].GetLinkID()
        )) {
        _text.ForceMeshUpdate();
      }
    }

    public void OnPointerClick(PointerEventData eventData) {
      if (_hoveredLink.Value != null) {
        Application.OpenURL(_links[_hoveredLink].Url);
      }
    }
  }
}
