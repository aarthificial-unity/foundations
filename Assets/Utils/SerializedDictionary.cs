using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utils {
  [Serializable]
  public class SerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>,
    ISerializationCallbackReceiver {
    [Serializable]
    public struct Pair {
      public TKey Key;
      public TValue Value;
    }

    [SerializeField] private List<Pair> _entries = new();

    public SerializedDictionary() { }

    public SerializedDictionary(IDictionary<TKey, TValue> dictionary) : base(
      dictionary
    ) { }

    public void OnBeforeSerialize() {
      _entries.Clear();
      foreach (var pair in this) {
        _entries.Add(
          new Pair {
            Key = pair.Key,
            Value = pair.Value,
          }
        );
      }
    }

    public void OnAfterDeserialize() {
      Clear();
      foreach (var entry in _entries) {
        var key = entry.Key;
        if (ContainsKey(key)) {
          key = default;
        }
        this[key] = entry.Value;
      }
    }
  }
}
