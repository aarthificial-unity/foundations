using System;
using UnityEngine;

namespace Player {
  public class PlayerAnimator : MonoBehaviour {
    public event Action Stepped;
    public Animator Animator;

    private void Awake() {
      Animator = GetComponent<Animator>();
    }

    public void OnPlayerStep(AnimationEvent evt) {
      if (evt.animatorClipInfo.weight > 0.5f) {
        Stepped?.Invoke();
      }
    }
  }
}
