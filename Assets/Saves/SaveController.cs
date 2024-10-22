﻿using Aarthificial.Safekeeper;
using Aarthificial.Safekeeper.Loaders;
using Aarthificial.Typewriter.Blackboards;
using System;
using Typewriter;

namespace Saves {
  [Serializable]
  public class GlobalData {
    public string SceneName;
    public Blackboard Blackboard = new();
  }

  public class SaveController : SaveControllerBase {
    public readonly bool AutoSave;
    public readonly GlobalData GlobalData = new();
    private readonly SaveLocation _location = new("default", "global");
    private readonly string _defaultSceneName;

    public SaveController(
      ISaveLoader loader,
      string defaultSceneName,
      bool autoSave
    ) : base(loader) {
      AutoSave = autoSave;
      _defaultSceneName = defaultSceneName;
    }

    protected override void OnLoad() {
      if (!Data.Read(_location, GlobalData)) {
        GlobalData.Blackboard.Set(Facts.RTItem, Facts.ItemGun);
      }
      GlobalData.SceneName ??= _defaultSceneName;
    }

    protected override void OnSave() {
      Data.Write(_location, GlobalData);
    }
  }
}
