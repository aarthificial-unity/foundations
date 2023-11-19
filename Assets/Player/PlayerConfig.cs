using UnityEngine;

namespace Player {
  public class PlayerConfig : ScriptableObject {
    public LayerMask InteractionMask;
    public LayerMask GroundMask;
    public float Acceleration = 30;
    public float WalkSpeed = 5;
    public float Smoothing = 5;
    public float RotationSpeed = 400;
    public float CloseDistance = 2;
    public float MinDistance = 4;
    public float MaxDistance = 5;
    public float LimitDistance = 6;
    public float MinTightDistance = 0.5f;
    public float MaxTightDistance = 1.5f;
  }
}
