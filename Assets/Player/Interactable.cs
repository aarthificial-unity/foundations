using UnityEngine;
using UnityEngine.Assertions;

namespace Player {
  public class Interactable : MonoBehaviour {
    private PlayerController _player;
    private MeshRenderer _meshRenderer;
    private Material _defaultMaterial;

    private void Awake() {
      _meshRenderer = GetComponent<MeshRenderer>();
      _defaultMaterial = _meshRenderer.material;
    }

    public virtual Vector3 GetPosition() {
      return transform.position;
    }

    public virtual float GetRadius() {
      return 0.3f;
    }

    public virtual void StopInteraction(PlayerController player) {
      Assert.AreEqual(
        player,
        _player,
        "Stop was called by a different player than Start"
      );
      _player = null;
      _meshRenderer.material = _defaultMaterial;
    }

    public virtual void SetReached(PlayerController player, bool hasReached) {
      Assert.AreEqual(
        player,
        _player,
        "InRange was called by a different player than Start"
      );
      _meshRenderer.material = hasReached ? player.Material : _defaultMaterial;
    }

    public virtual void StartInteraction(PlayerController player) {
      Assert.IsNull(
        _player,
        "Trying to start interaction while already interacting"
      );
      _player = player;
    }
  }
}
