using Aarthificial.Typewriter.Blackboards;
using System;
using Cinemachine;
using Interactions;
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
    [SerializeField] private CinemachineVirtualCamera _cameraPrefab;

    public PlayerController RT;
    public PlayerController LT;
    [NonSerialized] public PlayerType FocusedPlayer;
    [NonSerialized] public SpringTween CameraWeightTween;
    [NonSerialized] public DialogueState DialogueState;
    [NonSerialized] public ExploreState ExploreState;
    [NonSerialized] public Blackboard GlobalBlackboard = new();
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

      _cameraGroup = GetComponent<CinemachineTargetGroup>();
      CameraWeightTween.ForceSet(0.5f);
      _cameraGroup.DoUpdate();
      _camera = Instantiate(_cameraPrefab);
      _camera.Follow = transform;
    }

    private void Start() {
      SwitchState(ExploreState);
    }

    private void Update() {
      _currentState?.OnUpdate();
      Shader.SetGlobalVector(_ltPosition, LT.transform.position);
      Shader.SetGlobalVector(_rtPosition, RT.transform.position);
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

    public bool TryGetPlayer(int id, out PlayerController player) {
      if (id == InteractionContext.LT) {
        player = LT;
        return true;
      }

      if (id == InteractionContext.RT) {
        player = RT;
        return true;
      }

      player = null;
      return false;
    }

    public PlayerController this[PlayerType type] =>
      type switch {
        PlayerType.LT => LT,
        PlayerType.RT => RT,
        _ => null,
      };
  }
}
