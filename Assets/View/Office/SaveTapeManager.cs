using Audio.Events;
using System;
using UnityEngine;

namespace View.Office {
  public class SaveTapeManager : MonoBehaviour {
    public event Action<int> IndexChanged;

    [SerializeField] private FMODEventInstance _selectSound;
    [SerializeField] private FMODEventInstance _deselectSound;
    [SerializeField] private SaveTape[] _tapes;
    [SerializeField] private Transform _tapePlayer;

    private int _selectedTapeIndex = -1;

    public int CurrentIndex => _selectedTapeIndex;

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
      if (isAnyTapeSelected) {
        _selectSound.Play();
      } else {
        _deselectSound.Play();
      }
      IndexChanged?.Invoke(_selectedTapeIndex);
    }
  }
}
