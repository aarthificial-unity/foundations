using Player;
using System;
using Typewriter;
using UnityEngine;

namespace View.Dialogue {
  public class ButtonStyle : ScriptableObject {
    [Serializable]
    public struct Settings {
      public PlayerLookup<Color> BackgroundColors;
      public PlayerLookup<Color> TextColors;
      public bool Stroke;
      public bool HideArrow;
      public float TextureStrength;
    }

    [SerializeField] private Settings _speechSettings;
    [SerializeField] private Settings _thoughtSettings;
    [SerializeField] private Settings _actionSettings;

    public Settings this[DialogueEntry.BubbleStyle style] =>
      style switch {
        DialogueEntry.BubbleStyle.Thought => _thoughtSettings,
        DialogueEntry.BubbleStyle.Action => _actionSettings,
        _ => _speechSettings,
      };
  }
}
