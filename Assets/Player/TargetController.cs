using UnityEngine;
using Utils.Tweening;

namespace Player {
  public class TargetController : MonoBehaviour {
    [SerializeField] private SkinnedMeshRenderer _mesh;
    [SerializeField] private Transform _bone;
    private SpringTween _scaleTween;
    private PlayerType _previousPlayer;

    public bool Visible {
      set => gameObject.SetActive(value);
    }

    public void DrivenUpdate(PlayerController player) {
      if (player != null) {
        if (_previousPlayer != player.Type) {
          Visible = false;
        }

        _mesh.sharedMaterial = player.Material;

        if (player.NavigateState.IsActive) {
          transform.position = player.TargetPosition;
          if (player.CommandAction.action.IsPressed()) {
            _scaleTween.ForceSet(0.5f);
          } else {
            _scaleTween.Set(1f);
          }

          if (!player.Agent.pathPending) {
            Visible = true;
          }
        } else {
          Visible = false;
        }
      }

      if (_scaleTween.Update(SpringConfig.Medium)) {
        _bone.localScale = new Vector3(_scaleTween.X, 1, _scaleTween.X);
      }
    }
  }
}
