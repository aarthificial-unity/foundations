using Audio;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace View.Office {
  public class SaveTapeManager : MonoBehaviour {
    [SerializeField] private FMODEventInstance _selectSound;
    [SerializeField] private FMODEventInstance _deselectSound;
    [SerializeField] private SaveTape[] _tapes;
    [SerializeField] private Selectable _selectOnChoice;
    [SerializeField] private Transform _tapePlayer;
    [SerializeField] private GameObject _selectionScreen;
    [SerializeField] private GameObject _saveMenu;

    private int _selectedTapeIndex = -1;

    public void Eject() {
      HandleClicked(-1);
    }

    private void Awake() {
      for (var i = 0; i < _tapes.Length; i++) {
        _tapes[i].SetIndex(i);
        _tapes[i].Clicked += HandleClicked;
      }
      Render();
      _selectSound.Setup();
      _deselectSound.Setup();
    }

    private void OnDestroy() {
      _selectSound.Release();
      _deselectSound.Release();
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
