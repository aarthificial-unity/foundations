#if UNITY_EDITOR || DEVELOPMENT_BUILD
using Aarthificial.Typewriter;
using Aarthificial.Typewriter.Entries;
using Aarthificial.Typewriter.References;
using DevTools.CSV;
using Framework;
using Input;
using Interactions;
using Player;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using Utils;

namespace DevTools {
  [ExecuteAlways]
  public partial class GoogleSheetLoader {
    private static string _customDocumentId;
    private static string _customSheetName;

    [SerializeField] [Inject] private PlayerChannel _players;
    [SerializeField] [Inject] private InputChannel _input;
    [SerializeField] [Inject] private StoryMode _story;
    [SerializeField] private DatabaseTable _table;
    [SerializeField] private string _documentId;
    [SerializeField] private string _sheetName;

    private bool _isOpen;
    private string _previousMap;
    private Vector2 _scrollPosition;
    private bool _onlyFacts;
    private bool _hideEmpty = true;
    private string _searchText = "";

    private void OnEnable() {
      if (string.IsNullOrEmpty(_customDocumentId)) {
        _customDocumentId = _documentId;
      }
      if (string.IsNullOrEmpty(_customSheetName)) {
        _customSheetName = _sheetName;
      }
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
      _previousMap = _input.CurrentMap;
      _input.SetMap("None");
      Time.timeScale = 0;
    }

    private void Close() {
      if (!_isOpen) {
        return;
      }

      _isOpen = false;
      _input.SetMap(_previousMap);
      Time.timeScale = 1;
    }

    private void OnGUI() {
      if (!_isOpen) {
        return;
      }

      var isInConversation = _players.Manager.DialogueState.IsActive
        && _players.Manager.DialogueState.Context != null;

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
      _customDocumentId = GUILayout.TextField(_customDocumentId);
      _customSheetName = GUILayout.TextField(_customSheetName);
      GUILayout.FlexibleSpace();
      GUILayout.BeginHorizontal();
      if (GUILayout.Button("Cancel")) {
        Close();
      }
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

      var context = _players.Manager.DialogueState.Context;
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

    private IEnumerator Reload() {
      using UnityWebRequest www = UnityWebRequest.Get(
        $"https://docs.google.com/spreadsheets/d/{UnityWebRequest.EscapeURL(_customDocumentId)}/gviz/tq?tqx=out:csv&sheet={UnityWebRequest.EscapeURL(_customSheetName)}"
      );

      yield return www.SendWebRequest();
      if (www.result != UnityWebRequest.Result.Success) {
        Debug.LogError($"Connection error: {www.error}", this);
      } else {
        CsvConverter.Convert(_table, www.downloadHandler.text, this);
        TypewriterDatabase.Instance.CreateLookup();
      }

      Close();
      _story.Reload();
    }
  }
}
#endif
