using System;
using Cinemachine;
using Items;
using Player.ManagerStates;
using UnityEngine;
using Utils;

namespace Player {
  [RequireComponent(typeof(DialogueState))]
  [RequireComponent(typeof(ExploreState))]
  public class PlayerManager : MonoBehaviour {
    private readonly int _rtPosition = Shader.PropertyToID("_RTPosition");
    private readonly int _ltPosition = Shader.PropertyToID("_LTPosition");

    [Inject] [SerializeField] private PlayerChannel _players;
    [SerializeField] private PlayerController _rtPrefab;
    [SerializeField] private PlayerController _ltPrefab;
    [SerializeField] private Vector3 _rtStartPosition;
    [SerializeField] private Vector3 _ltStartPosition;

    [NonSerialized] public DialogueState DialogueState;
    [NonSerialized] public ExploreState ExploreState;
    private ManagerState _currentState;

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

      var group = GetComponent<CinemachineTargetGroup>();
      group.m_Targets[0].target = rt.transform;
      group.m_Targets[1].target = lt.transform;
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
      Shader.SetGlobalVector(_rtPosition, _players.RT.transform.position);
      Shader.SetGlobalVector(_ltPosition, _players.LT.transform.position);
    }
  }
}
