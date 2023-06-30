using System;
using Cinemachine;
using Items;
using Player.ManagerStates;
using UnityEngine;
using UnityEngine.Assertions;
using Utils;
using View.Dialogue;

namespace Player {
  [RequireComponent(typeof(DialogueState))]
  [RequireComponent(typeof(ExploreState))]
  public class PlayerManager : MonoBehaviour {
    [Inject] [SerializeField] private PlayerChannel _players;
    [SerializeField] private PlayerController _rtPrefab;
    [SerializeField] private PlayerController _ltPrefab;
    [SerializeField] private Vector3 _rtStartPosition;
    [SerializeField] private Vector3 _ltStartPosition;
    [SerializeField] private ItemSlot _rtItemSlot;
    [SerializeField] private ItemSlot _ltItemSlot;
    [SerializeField] private DialogueButton[] _dialogueButtons;
    private int _currentDialogueButtonIndex;

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

    public DialogueButton BorrowButton() {
      Assert.IsTrue(_currentDialogueButtonIndex < _dialogueButtons.Length);
      return _dialogueButtons[_currentDialogueButtonIndex++];
    }

    public void ReleaseButton(DialogueButton button) {
      Assert.IsTrue(_currentDialogueButtonIndex > 0);
      _dialogueButtons[--_currentDialogueButtonIndex] = button;
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
      rt.Slot = _rtItemSlot;
      lt.Slot = _ltItemSlot;

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
    }
  }
}
