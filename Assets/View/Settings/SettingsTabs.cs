using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace View.Settings {
  public class SettingsTabs : MonoBehaviour {
    [SerializeField] private CanvasGroup _tabContainer;
    [SerializeField] private SettingsPage[] _pages;
    [SerializeField] private Tab[] _tabs;
    [SerializeField] private Image _paper;
    [SerializeField] private float _spacing = 0.001f;

    private int _currentIndex;
    private readonly List<SettingsPage> _pageList = new(3);

    private void Awake() {
      for (var i = 0; i < _pages.Length; i++) {
        var page = _pages[i];
        var tab = _tabs[i];
        _pageList.Insert(0, page);
        tab.DrivenAwake(i);
        page.DrivenAwake(_spacing, _pages.Length - i, _pages.Length);
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
      _tabContainer.interactable = value;
      _tabContainer.blocksRaycasts = value;
      _pages[_currentIndex].SetInteractive(value);
    }
  }
}
