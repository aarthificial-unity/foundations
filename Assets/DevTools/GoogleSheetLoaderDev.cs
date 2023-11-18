#if UNITY_EDITOR || DEVELOPMENT_BUILD
using Aarthificial.Typewriter;
using Aarthificial.Typewriter.Entries;
using Aarthificial.Typewriter.References;
using DevTools.CSV;
using Framework;
using Interactions;
using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;

namespace DevTools {
  [ExecuteAlways]
  public partial class GoogleSheetLoader {
    private static readonly Dictionary<string, string> _documentIdOverrides =
      new();
    private static readonly Dictionary<string, string> _sheetNameOverrides =
      new();

    [SerializeField] private DatabaseTable _table;
    [SerializeField] private string _documentId;
    [SerializeField] private string _sheetName;

    private bool _isOpen;
    private string _previousMap;
    private Vector2 _scrollPosition;
    private bool _onlyFacts;
    private bool _hideEmpty = true;
    private string _searchText = "";
    private PlayerManager _players;

    private void Awake() {
      _players = GetComponent<PlayerManager>();
    }

    private void OnEnable() {
      _documentIdOverrides.TryAdd(_documentId, _documentId);
      _sheetNameOverrides.TryAdd(_sheetName, _sheetName);
    }

    private void Update() {
      if (Keyboard.current.tabKey.wasPressedThisFrame) {
        if (_isOpen) {
          Close();
        } else {
          Open();
        }
      }
    }

    private void Open() {
      if (_isOpen) {
        return;
      }

      _isOpen = true;
      _previousMap = App.Input.CurrentMap;
      App.Input.SetMap("None");
      Time.timeScale = 0;
    }

    private void Close() {
      if (!_isOpen) {
        return;
      }

      _isOpen = false;
      App.Input.SetMap(_previousMap);
      Time.timeScale = 1;
    }

    private void OnGUI() {
      if (!_isOpen) {
        return;
      }

      var isInConversation = _players.DialogueState.IsActive
        && _players.DialogueState.Context != null;

      var size = new Vector2(640, 160);
      var rect = new Rect(
        (Screen.width - size.x) / 2,
        (Screen.height - size.y) / 2,
        size.x,
        size.y
      );

      if (isInConversation) {
        rect.y -= size.y - 8;
      }

      LoaderGUI(rect);
      if (isInConversation) {
        rect.y += rect.height + 16;
        rect.height *= 2;
        BlackboardGUI(rect);
      }
    }

    private Rect BoxGUI(Rect rect) {
      GUI.Box(rect, null as string);
      GUI.Box(rect, null as string);
      GUI.Box(rect, null as string);
      GUI.Box(rect, null as string);

      rect.x += 20;
      rect.y += 20;
      rect.width -= 40;
      rect.height -= 40;

      return rect;
    }

    private void LoaderGUI(Rect rect) {
      rect = BoxGUI(rect);

      GUILayout.BeginArea(rect);
      GUILayout.BeginVertical();
      GUILayout.Label("Google Sheet Loader", GUI.skin.label);
      GUILayout.FlexibleSpace();
      GUILayout.BeginHorizontal();

      GUILayout.BeginVertical();
      GUILayout.Label("Document ID");
      GUILayout.Label("Sheet Name");
      GUILayout.EndVertical();

      GUILayout.BeginVertical();
      _documentIdOverrides[_documentId] =
        GUILayout.TextField(_documentIdOverrides[_documentId]);
      _sheetNameOverrides[_sheetName] =
        GUILayout.TextField(_sheetNameOverrides[_sheetName]);
      GUILayout.FlexibleSpace();
      GUILayout.BeginHorizontal();
      if (GUILayout.Button("Cancel")) {
        Close();
      }
      GUI.enabled = !App.Game.Story.IsLoading;
      if (GUILayout.Button(App.Game.Story.IsLoading ? "Saving" : "Save")) {
        StartCoroutine(Save());
      }
      GUI.enabled = true;
      if (GUILayout.Button("Reload")) {
        StartCoroutine(Reload());
      }
      GUILayout.EndHorizontal();
      GUILayout.EndVertical();

      GUILayout.EndHorizontal();
      GUILayout.EndVertical();
      GUILayout.EndArea();
    }

    private void BlackboardGUI(Rect rect) {
      rect = BoxGUI(rect);
      GUILayout.BeginArea(rect);

      GUILayout.Label("Current blackboard", GUI.skin.label);
      GUILayout.BeginHorizontal();
      _searchText = GUILayout.TextField(_searchText, GUILayout.MinWidth(300));
      _onlyFacts = GUILayout.Toggle(_onlyFacts, "Facts only");
      _hideEmpty = GUILayout.Toggle(_hideEmpty, "Hide empty");
      GUILayout.FlexibleSpace();
      GUILayout.EndHorizontal();

      _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

      var context = _players.DialogueState.Context;
      BlackboardEntriesGUI(context, InteractionContext.GlobalScope);
      BlackboardEntriesGUI(context, InteractionContext.InteractionScope);
      BlackboardEntriesGUI(context, InteractionContext.ContextScope);

      GUILayout.EndScrollView();
      GUILayout.EndArea();
    }

    private void BlackboardEntriesGUI(ITypewriterContext context, int scope) {
      if (!context.TryGetBlackboard(scope, out var blackboard)) {
        return;
      }

      foreach (var pair in blackboard) {
        if (!pair.Key.TryGetEntry(out var entry)) {
          continue;
        }

        if ((_onlyFacts && entry is not FactEntry)
          || (_hideEmpty && pair.Value == 0)) {
          continue;
        }

        if (!string.IsNullOrEmpty(_searchText)
          && !entry.Key.Contains(_searchText)) {
          continue;
        }

        var stringValue =
          ((EntryReference)pair.Value).TryGetEntry(out var reference)
            ? $"{reference.Key} ({pair.Value})"
            : pair.Value.ToString();

        GUILayout.BeginHorizontal();
        GUILayout.Label(entry.Key);
        GUILayout.FlexibleSpace();
        GUILayout.Label(stringValue);
        GUILayout.EndHorizontal();
      }
    }

    private IEnumerator Save() {
      App.Game.Story.Save();
      while (!App.Game.Story.IsLoading) {
        yield return null;
      }
      while (App.Game.Story.IsLoading) {
        yield return null;
      }
      Close();
    }

    private IEnumerator Reload() {
      using UnityWebRequest www = UnityWebRequest.Get(
        $"https://docs.google.com/spreadsheets/d/{UnityWebRequest.EscapeURL(_documentIdOverrides[_documentId])}/gviz/tq?tqx=out:csv&sheet={UnityWebRequest.EscapeURL(_sheetNameOverrides[_sheetName])}"
      );

      yield return www.SendWebRequest();
      if (www.result != UnityWebRequest.Result.Success) {
        Debug.LogError($"Connection error: {www.error}", this);
      } else {
        CsvConverter.Convert(_table, www.downloadHandler.text, this);
        TypewriterDatabase.Instance.CreateLookup();
      }

      Close();
      App.Game.Story.Reload();
    }
  }
}
#endif
