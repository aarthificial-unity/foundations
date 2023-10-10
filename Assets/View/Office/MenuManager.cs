using Framework;
using System;
using UnityEngine;
using Utils;
using View.Controls;
using View.Office.States;
using MenuState = View.Office.States.MenuState;

namespace View.Office {
  public class MenuManager : MonoBehaviour {
    private enum Screen {
      Selection,
      SaveMenu,
      ResetConfirmation,
    }

    [SerializeField] private GameObject _selectionScreen;
    [SerializeField] private GameObject _saveMenuScreen;
    [SerializeField] private GameObject _resetConfirmationScreen;

    [SerializeField] private PaperButton _newGameButton;
    [SerializeField] private PaperButton _continueButton;
    [SerializeField] private PaperButton _resetButton;
    [SerializeField] private PaperButton _ejectButton;
    [SerializeField] private PaperButton _resetAcceptButton;
    [SerializeField] private PaperButton _resetCancelButton;
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

      _newGameButton.Clicked += HandleContinue;
      _continueButton.Clicked += HandleContinue;
      _resetButton.Clicked += HandleReset;
      _resetAcceptButton.Clicked += HandleResetAccept;
      _resetCancelButton.Clicked += HandleResetCancel;
      _ejectButton.Clicked += _saveTapeManager.Eject;
      _exitButton.Clicked += ExitState.Enter;
      _settingsButton.Clicked += SettingsState.Enter;
      _settingsExitButton.Clicked += MainMenuState.Enter;
      _saveTapeManager.IndexChanged += HandleIndexChanged;
    }

    private void HandleIndexChanged(int index) {
      SetScreen(index >= 0 ? Screen.SaveMenu : Screen.Selection);
    }

    private void Start() {
      SwitchState(IntroState);
      SetScreen(Screen.Selection);
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

    private void SetScreen(Screen newScreen) {
      if (newScreen == Screen.SaveMenu) {
        var save = App.Save.GetSaveController(_saveTapeManager.CurrentIndex);
        _resetButton.gameObject.SetActive(save.Exists);
        _continueButton.gameObject.SetActive(save.Exists);
        _newGameButton.gameObject.SetActive(!save.Exists);
      }

      _selectionScreen.SetActive(newScreen == Screen.Selection);
      _saveMenuScreen.SetActive(newScreen == Screen.SaveMenu);
      _resetConfirmationScreen.SetActive(newScreen == Screen.ResetConfirmation);
    }

    private void HandleReset() {
      SetScreen(Screen.ResetConfirmation);
    }

    private void HandleContinue() {
      StartGameState.Enter(_saveTapeManager.CurrentIndex);
    }

    private void HandleResetCancel() {
      SetScreen(Screen.SaveMenu);
    }

    private void HandleResetAccept() {
      var save = App.Save.GetSaveController(_saveTapeManager.CurrentIndex);
      save.Delete();
      SetScreen(Screen.SaveMenu);
    }
  }
}
