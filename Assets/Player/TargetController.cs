using UnityEngine;
using Utils.Tweening;

namespace Player {
  public class TargetController : MonoBehaviour {
    private static readonly int _playerTypeID =
      Shader.PropertyToID("_PlayerType");

    [SerializeField] private SkinnedMeshRenderer _mesh;
    [SerializeField] private Transform _bone;
    private SpringTween _scaleTween;
    private SpringTween _playerTypeTween;
    private PlayerType _previousPlayer;
    private MaterialPropertyBlock _block;
    private bool _areBothActive;

    public bool Visible {
      get => gameObject.activeSelf;
      set => gameObject.SetActive(value);
    }

    private void Awake() {
      _block = new MaterialPropertyBlock();
    }

    public void DrivenUpdate(
      PlayerManager manager,
      PlayerController player,
      bool areBothActive
    ) {
      var wereBothActive = _areBothActive;
      _areBothActive = areBothActive;

      var forceSet = false;
      if (player != null) {
        if (_previousPlayer != player.Type) {
          Visible = false;
        }

        _previousPlayer = player.Type;
        if (player.NavigateState.IsActive) {
          transform.position = player.TargetPosition;
          if (player.CommandAction.action.IsPressed()) {
            _scaleTween.ForceSet(0.5f);
          } else {
            _scaleTween.Set(1f);
          }

          if (!Visible && !player.Agent.pathPending) {
            Visible = true;
            forceSet = true;
          }
        } else {
          Visible = false;
        }
      }

      var playerTypeValue = _areBothActive
        ? 0
        : _previousPlayer == PlayerType.LT
          ? -1
          : 1;
      if (forceSet && !wereBothActive) {
        _playerTypeTween.ForceSet(playerTypeValue);
      } else {
        _playerTypeTween.Set(playerTypeValue);
      }

      if (_playerTypeTween.Update(SpringConfig.Slow)) {
        _block.SetFloat(_playerTypeID, _playerTypeTween.X);
        _mesh.SetPropertyBlock(_block);
      }

      var direction = manager.LT.transform.position
        - manager.RT.transform.position;
      transform.rotation = Quaternion.LookRotation(
        new Vector3(direction.x, 0, direction.z).normalized
      );

      if (_scaleTween.Update(SpringConfig.Medium)) {
        _bone.localScale = new Vector3(_scaleTween.X, 1, _scaleTween.X);
      }
    }
  }
}
