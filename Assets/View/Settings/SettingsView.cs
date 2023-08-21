using System.Collections.Generic;
using UnityEngine;
using View.Controls;
using View.Office;

namespace View.Settings {
  public class SettingsView : MonoBehaviour {
    private static readonly int StateParam = Animator.StringToHash("State");

    public PaperButton ExitButton;
    [SerializeField] private Camera _worldCamera;
    [SerializeField] private Animator _animator;
    [SerializeField] private InteractiveGroup _menu;
    [SerializeField] private SettingsPage[] _pages;
    [SerializeField] private Tab[] _tabs;
    [SerializeField] private float _spacing = 0.001f;

    private int _currentIndex;
    private readonly List<SettingsPage> _pageList = new(3);

    private void Awake() {
      _menu.DrivenAwake(_worldCamera);
      for (var i = 0; i < _pages.Length; i++) {
        var page = _pages[i];
        var tab = _tabs[i];
        _pageList.Insert(0, page);
        tab.DrivenAwake(i);
        page.DrivenAwake(
          _worldCamera,
          _spacing,
          _pages.Length - i,
          _pages.Length
        );
        tab.Clicked += HandleTabClicked;
      }
      _tabs[0].Toggle(true);
      SetInteractive(false);
    }

    private void HandleTabClicked(int index) {
      if (_currentIndex == index) {
        return;
      }

      _tabs[_currentIndex].Toggle(false);
      _currentIndex = index;
      _tabs[_currentIndex].Toggle(true);

      var currentPage = _pages[_currentIndex];
      _pageList.Remove(currentPage);
      _pageList.Add(currentPage);
      for (var i = 0; i < _pageList.Count; i++) {
        var page = _pageList[i];
        page.SetOrder(i + 1);
        page.SetInteractive(false);
      }
      currentPage.Nudge();
      currentPage.SetInteractive(true);
    }

    public void SetInteractive(bool value) {
      _menu.SetInteractive(value);
      _pages[_currentIndex].SetInteractive(value);
    }

    public void SetAnimationFactor(float factor) {
      _animator.SetFloat(StateParam, factor);
    }
  }
}
