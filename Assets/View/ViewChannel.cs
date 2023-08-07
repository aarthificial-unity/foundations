using System;
using UnityEngine;
using UnityEngine.Assertions;
using View.Dialogue;

namespace View {
  public class ViewChannel : ScriptableObject {
    [NonSerialized] public HUDView HUD;
    [NonSerialized] public DialogueView Dialogue;
  }
}
