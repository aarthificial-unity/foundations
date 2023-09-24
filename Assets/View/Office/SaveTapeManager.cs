using Audio;
using Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using View.Controls;

namespace View.Office {
  public class SaveTapeManager : MonoBehaviour {
    [SerializeField] private FMODEventInstance _selectSound;
    [SerializeField] private FMODEventInstance _deselectSound;
    [SerializeField] private SaveTape[] _tapes;
    [SerializeField] private Selectable _selectOnChoice;
    [SerializeField] private Transform _tapePlayer;
    [SerializeField] private GameObject _selectionScreen;
    [SerializeField] private GameObject _saveMenu;
    [SerializeField] private PaperButton _ejectButton;
    [SerializeField] private GameObject _initialCamera;
    [SerializeField] private Backdrop _backdrop;

    private int _selectedTapeIndex = -1;

    private void Awake() {
      for (var i = 0; i < _tapes.Length; i++) {
        _tapes[i].SetIndex(i);
        _tapes[i].Clicked += HandleClicked;
      }
      Render();
      _selectSound.Setup();
      _deselectSound.Setup();
      _ejectButton.Clicked += () => HandleClicked(-1);
    }

    private void OnDestroy() {
      _selectSound.Release();
      _deselectSound.Release();
    }

    // TODO Move somewhere else
    private IEnumerator StartGame() {
      _initialCamera.SetActive(true);
      _backdrop.Request();
      yield return new WaitForSecondsRealtime(1f);
      App.Game.Story.Enter();
    }

    private void HandleClicked(int index) {
      if (index < 0) {
        if (_selectedTapeIndex >= 0) {
          _tapes[_selectedTapeIndex].Eject();
          _tapes[_selectedTapeIndex].Select();
        }
        _selectedTapeIndex = -1;
        Render();
        return;
      }

      if (_selectedTapeIndex == index) {
        _tapes[_selectedTapeIndex].Eject();
        _selectedTapeIndex = -1;
        Render();
        return;
      }

      if (_selectedTapeIndex >= 0) {
        _tapes[_selectedTapeIndex].Eject();
      }

      _selectedTapeIndex = index;
      _tapes[_selectedTapeIndex].Insert(_tapePlayer);
      Render();
    }

    private void Render() {
      var isAnyTapeSelected = _selectedTapeIndex >= 0;
      _selectionScreen.SetActive(!isAnyTapeSelected);
      _saveMenu.SetActive(isAnyTapeSelected);
      if (isAnyTapeSelected) {
        _selectOnChoice.QuietSelect();
        _selectSound.Play();
      } else {
        _deselectSound.Play();
      }
    }
  }
}
