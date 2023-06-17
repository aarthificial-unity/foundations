using System;
using UnityEngine;

namespace Player {
  public class PlayerConfig : ScriptableObject {
    [NonSerialized] public int GroundLayer;
    public LayerMask InteractionMask;
    public float Acceleration = 30;
    public float WalkSpeed = 5;
    public float SprintSpeed = 10;
    public float MinDistance = 4;
    public float MaxDistance = 5;
    public float LimitDistance = 6;
    public float DoubleTapTime = 0.75f;

    private void OnEnable() {
      GroundLayer = LayerMask.NameToLayer("Ground");
    }
  }
}
