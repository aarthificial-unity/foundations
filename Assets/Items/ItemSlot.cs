using Player;
using Typewriter;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using View;
using View.Dialogue;

namespace Items {
  public class ItemSlot : MonoBehaviour {
    [Inject] [SerializeField] private ButtonStyle _styles;
    [SerializeField] private DialogueEntry.BubbleStyle _style;
    [SerializeField] private PlayerType _player;
    [SerializeField] private Image _icon;
    [SerializeField] private BoxSDF _fill;
    [SerializeField] private BoxSDF _stroke;

    private void Start() {
      var style = _styles[_style];
      var color = style.BackgroundColors[_player];

      _fill.Color = color;
      _stroke.Color = color;
      _fill.TextureStrength = style.TextureStrength;
      _stroke.TextureStrength = style.TextureStrength;

      gameObject.SetActive(false);
    }

    public void SetItem(ItemEntry item) {
      _icon.sprite = item?.Icon;
      gameObject.SetActive(item != null);
    }
  }
}
