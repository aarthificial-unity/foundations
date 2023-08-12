using System;
using UnityEngine;
using View.Office.States;

namespace View.Office {
  public class MenuManager : MonoBehaviour {
    [SerializeField] private ComputerButton _newGameButton;
    [SerializeField] private ComputerButton _continueButton;
    [SerializeField] private Clickable _exitButton;

    [NonSerialized] public IntroState IntroState;
    [NonSerialized] public MainMenuState MainMenuState;
    [NonSerialized] public StartGameState StartGameState;
    [NonSerialized] public ExitState ExitState;

    private MenuState _currentState;

    private void Awake() {
      IntroState = GetComponent<IntroState>();
      MainMenuState = GetComponent<MainMenuState>();
      StartGameState = GetComponent<StartGameState>();
      ExitState = GetComponent<ExitState>();

      _newGameButton.Clicked += StartGameState.NewGame;
      _continueButton.Clicked += StartGameState.ContinueGame;
      _exitButton.Clicked += ExitState.Enter;
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
