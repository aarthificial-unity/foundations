using Framework;
using System.Collections;
using TMPro;
using UnityEngine;
using Utils;

namespace View.Office {
  public class SaveTapeManager : MonoBehaviour {
    [Inject] [SerializeField] private StoryMode _storyMode;
    [SerializeField] private SaveTape[] _tapes;
    [SerializeField] private Transform _tapePlayer;
    [SerializeField] private GameObject _selectionScreen;
    [SerializeField] private GameObject _saveMenu;
    [SerializeField] private ComputerButton _ejectButton;
    [SerializeField] private GameObject _initialCamera;
    [SerializeField] private Backdrop _backdrop;

    private int _selectedTapeIndex = -1;

    private void Awake() {
      for (var i = 0; i < _tapes.Length; i++) {
        _tapes[i].SetIndex(i);
        _tapes[i].Selected += HandleSelected;
      }
      Render();
      _ejectButton.Clicked += () => HandleSelected(-1);
    }

    private IEnumerator StartGame() {
      _initialCamera.SetActive(true);
      _backdrop.Request();
      yield return new WaitForSecondsRealtime(1f);
      _storyMode.RequestStart();
    }

    private void HandleSelected(int index) {
      if (index < 0) {
        if (_selectedTapeIndex >= 0) {
          _tapes[_selectedTapeIndex].Deselect();
        }
        _selectedTapeIndex = -1;
        Render();
        return;
      }

      if (_selectedTapeIndex == index) {
        _tapes[_selectedTapeIndex].Deselect();
        _selectedTapeIndex = -1;
        Render();
        return;
      }

      if (_selectedTapeIndex >= 0) {
        _tapes[_selectedTapeIndex].Deselect();
      }

      _selectedTapeIndex = index;
      _tapes[_selectedTapeIndex].Select(_tapePlayer);
      Render();
    }

    private void Render() {
      var isAnyTapeSelected = _selectedTapeIndex >= 0;
      _selectionScreen.SetActive(!isAnyTapeSelected);
      _saveMenu.SetActive(isAnyTapeSelected);
    }
  }
}
