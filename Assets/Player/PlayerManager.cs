using System;
using Cinemachine;
using Player.ManagerStates;
using Settings.Bundles;
using UnityEngine;
using Utils;
using Utils.Tweening;

namespace Player {
  [RequireComponent(typeof(DialogueState))]
  [RequireComponent(typeof(ExploreState))]
  public class PlayerManager : MonoBehaviour {
    private readonly int _rtPosition = Shader.PropertyToID("_RTPosition");
    private readonly int _ltPosition = Shader.PropertyToID("_LTPosition");

    [Inject] [SerializeField] private GameplaySettingsBundle _bundle;
    [Inject] [SerializeField] private PlayerChannel _players;
    [SerializeField] private PlayerController _rtPrefab;
    [SerializeField] private PlayerController _ltPrefab;
    [SerializeField] private Vector3 _rtStartPosition;
    [SerializeField] private Vector3 _ltStartPosition;
    [SerializeField] private CinemachineVirtualCamera _cameraPrefab;

    [NonSerialized] public PlayerType FocusedPlayer;
    [NonSerialized] public SpringTween CameraWeightTween;
    [NonSerialized] public DialogueState DialogueState;
    [NonSerialized] public ExploreState ExploreState;
    private CinemachineTargetGroup _cameraGroup;
    private ManagerState _currentState;
    private CinemachineVirtualCamera _camera;

    public void SwitchState(ManagerState state) {
      if (_currentState == state) {
        return;
      }

      _currentState?.OnExit();
      _currentState = state;
      _currentState?.OnEnter();
    }

    private void Awake() {
      DialogueState = GetComponent<DialogueState>();
      ExploreState = GetComponent<ExploreState>();

      var rt = Instantiate(_rtPrefab, _rtStartPosition, Quaternion.identity);
      var lt = Instantiate(_ltPrefab, _ltStartPosition, Quaternion.identity);
      _players.Manager = this;
      _players.LT = lt;
      _players.RT = rt;
      rt.Other = lt;
      lt.Other = rt;

      _cameraGroup = GetComponent<CinemachineTargetGroup>();
      CameraWeightTween.ForceSet(0.5f);
      _cameraGroup.m_Targets[0].target = rt.transform;
      _cameraGroup.m_Targets[0].weight = 0.5f;
      _cameraGroup.m_Targets[1].target = lt.transform;
      _cameraGroup.m_Targets[1].weight = 0.5f;
      _cameraGroup.DoUpdate();
      _camera = Instantiate(_cameraPrefab);
      _camera.Follow = transform;
    }

    private void OnDestroy() {
      _players.Manager = null;
      _players.LT = null;
      _players.RT = null;
    }

    private void Start() {
      SwitchState(ExploreState);
    }

    private void Update() {
      _currentState?.OnUpdate();
      var lt = _players.LT.transform.position;
      var rt = _players.RT.transform.position;
      Shader.SetGlobalVector(_ltPosition, lt);
      Shader.SetGlobalVector(_rtPosition, rt);
    }

    private void FixedUpdate() {
      var offset = _bundle.CameraWeight.Get() / 100f;
      CameraWeightTween.Set(
        FocusedPlayer switch {
          PlayerType.LT => 0.5f - offset,
          PlayerType.RT => 0.5f + offset,
          _ => 0.5f,
        }
      );

      if (CameraWeightTween.Update(SpringConfig.Slow)) {
        var weight = CameraWeightTween.X;
        _cameraGroup.m_Targets[0].weight = weight;
        _cameraGroup.m_Targets[1].weight = 1 - weight;
      }
    }

#if UNITY_EDITOR
    [ContextMenu("Save State")]
    private void SaveState() {
      _rtStartPosition = _players.RT.transform.position;
      _ltStartPosition = _players.LT.transform.position;
    }
#endif
  }
}
