﻿using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Framework {
  public class MenuState : GameState {
    private const int _sceneIndex = 0;

    public void Enter() {
      Manager.SwitchState(this);
    }

    public override IEnumerator OnEnter() {
      yield return base.OnEnter();
      Time.timeScale = 1;
      if (SceneManager.GetActiveScene().buildIndex != _sceneIndex) {
        SceneManager.LoadScene(_sceneIndex);
      }
      App.Input.SwitchToUI();
    }
  }
}
