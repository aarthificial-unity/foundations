using System;
using UnityEngine;

namespace Player {
  public class PlayerConfig : ScriptableObject {
    [NonSerialized] public int GroundLayer;
    public LayerMask InteractionMask;
    public float Acceleration = 30;
    public float WalkSpeed = 5;
    public float CloseDistance = 2;
    public float MinDistance = 4;
    public float MaxDistance = 5;
    public float LimitDistance = 6;

    private void OnEnable() {
      GroundLayer = LayerMask.NameToLayer("Ground");
    }
  }
}
