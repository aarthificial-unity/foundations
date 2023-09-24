using System;
using UnityEngine;
using View.Controls;
using View.Office.States;

namespace View.Office {
  public class MenuManager : MonoBehaviour {
    [SerializeField] private PaperButton _newGameButton;
    [SerializeField] private PaperButton _continueButton;
    [SerializeField] private PaperButton _ejectButton;
    [SerializeField] private Clickable _exitButton;
    [SerializeField] private Clickable _settingsButton;
    [SerializeField] private PaperButton _settingsExitButton;

    [NonSerialized] public IntroState IntroState;
    [NonSerialized] public MainMenuState MainMenuState;
    [NonSerialized] public StartGameState StartGameState;
    [NonSerialized] public ExitState ExitState;
    [NonSerialized] public SettingsState SettingsState;

    private MenuState _currentState;
    private SaveTapeManager _saveTapeManager;

    private void Awake() {
      _saveTapeManager = FindObjectOfType<SaveTapeManager>();
      IntroState = GetComponent<IntroState>();
      MainMenuState = GetComponent<MainMenuState>();
      StartGameState = GetComponent<StartGameState>();
      ExitState = GetComponent<ExitState>();
      SettingsState = GetComponent<SettingsState>();

      _newGameButton.Clicked += StartGameState.NewGame;
      _continueButton.Clicked += StartGameState.ContinueGame;
      _ejectButton.Clicked += _saveTapeManager.Eject;
      _exitButton.Clicked += ExitState.Enter;
      _settingsButton.Clicked += SettingsState.Enter;
      _settingsExitButton.Clicked += MainMenuState.Enter;
    }

    private void Start() {
      SwitchState(IntroState);
    }

    private void Update() {
      _currentState?.OnUpdate();
    }

    public void SwitchState(MenuState state) {
      if (_currentState == state) {
        return;
      }

      _currentState?.OnExit();
      _currentState = state;
      _currentState?.OnEnter();
    }
  }
}
