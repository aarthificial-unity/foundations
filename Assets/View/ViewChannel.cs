using System;
using UnityEngine;

namespace View {
  public class ViewChannel : ScriptableObject {
    [NonSerialized] public HUDView HUD;
    [NonSerialized] public DialogueView Dialogue;
  }
}
