using System;
using UnityEngine;

namespace Settings.Bundles {
  public struct SettingProperty {
    private event Action<int> _changed;
    public event Action<int> Changed {
      add {
        _changed += value;
        value(_current);
      }
      remove => _changed -= value;
    }

    private readonly string _key;
    private int _current;

    public SettingProperty(string key, int initial = 0) {
      _key = key;
      _current = PlayerPrefs.GetInt(_key, initial);
      _changed = null;
    }

    public void Set(int value) {
      _current = value;
      PlayerPrefs.SetInt(_key, value);
      _changed?.Invoke(value);
    }

    public void Set(bool value) {
      Set(value ? 1 : 0);
    }

    public readonly int Get() {
      return _current;
    }

    public readonly bool GetBool() {
      return _current != 0;
    }
  }
}
