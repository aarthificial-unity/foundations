using System;
using Aarthificial.Typewriter.Attributes;
using Aarthificial.Typewriter.Entries;
using Aarthificial.Typewriter.References;
using UnityEngine;

namespace Player {
  public class PlayerConfig : ScriptableObject {
    public PlayerLookup<Color> Colors;
    public LayerMask InteractionMask;
    public LayerMask PlayerMask;
    public LayerMask GroundMask;
    public float Acceleration = 30;
    public float WalkSpeed = 5;
    public float Smoothing = 5;
    public float RotationSpeed = 400;
    public float CloseDistance = 2;
    public float MinDistance = 4;
    public float MaxDistance = 5;
    public float LimitDistance = 6;
  }
}
