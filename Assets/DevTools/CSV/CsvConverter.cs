using Aarthificial.Typewriter;
using Aarthificial.Typewriter.Blackboards;
using Aarthificial.Typewriter.Entries;
using Aarthificial.Typewriter.References;
using Interactions;
using System;
using System.Collections.Generic;
using System.Text;
using Typewriter;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DevTools.CSV {
  public static class CsvConverter {
    private enum ScriptState {
      Initial,
      Fact,
      Operation,
      FirstValue,
      Range,
      SecondValue,
    }

    private struct ParsingContext {
      public int Line;
      public int Column;
      public string Code;
    }

    private struct ScriptResult {
      public string Fact;
      public string Operation;
      public string FirstValue;
      public string SecondValue;
      public bool Range;
    }

    private class ParsingException : Exception {
      public ParsingException(string message, ParsingContext context) : base(
        $"{message} ({context.Line + 1}:{context.Column + 1})\n{GetErrorFrame(context)}"
      ) { }
    }

    private static readonly Dictionary<string, EntryReference> _lookup = new();
    private static readonly List<EntryReference> _triggers = new();
    private static readonly List<RuleEntry.Dispatcher> _events = new();
    private static readonly List<BlackboardCriterion> _criteria = new();
    private static readonly List<BlackboardModification> _modifications = new();
    private static readonly HashSet<int> _convertedIds = new();

    public static void Convert(
      DatabaseTable table,
      string data,
      Object context
    ) {
      var rows = CsvParser.Parse(data);

      _convertedIds.Clear();
      _lookup.Clear();

      foreach (var entry in table.Facts) {
        _lookup.Add(entry.Key, (EntryReference)entry);
      }
      foreach (var entry in table.Rules) {
        _lookup.Add(entry.Key, (EntryReference)entry);
      }
      foreach (var entry in table.Events) {
        _lookup.Add(entry.Key, (EntryReference)entry);
      }

      // _collection =
      // LocalizationEditorSettings.GetStringTableCollection("Dialogue");

      _lookup.Add("LT", InteractionContext.LT);
      _lookup.Add("RT", InteractionContext.RT);
      _lookup.Add("initiator", InteractionContext.Initiator);
      _lookup.Add("listener", InteractionContext.Listener);
      _lookup.Add("current_speaker", InteractionContext.CurrentSpeaker);
      _lookup.Add("is_LT_present", InteractionContext.IsLTPresent);
      _lookup.Add("is_RT_present", InteractionContext.IsRTPresent);
      _lookup.Add("call_other", InteractionContext.CallOther);
      _lookup.Add("pick_up", InteractionContext.PickUp);

      BaseEntry previous = null;
      for (var i = 1; i < rows.Count; i++) {
        var cells = rows[i].Cells;
        if (cells.IsEmpty()) {
          continue;
        }

        Type type;
        switch (cells.Type()) {
          case "event":
            type = typeof(EventEntry);
            break;
          case "choice":
            type = cells.Text() == ""
              ? typeof(ChoiceEntry)
              : typeof(DialogueEntry);
            break;
          case "dialogue":
          case "option":
            type = typeof(DialogueEntry);
            break;
          case "fact":
            type = typeof(FactEntry);
            break;
          default:
            Debug.LogError(
              $"Row {i + 1} ({cells.Key()}): Unknown type {cells.Type()}",
              context
            );
            continue;
        }

        if (_lookup.TryGetValue(cells.Key(), out var reference)) {
          if (_convertedIds.Contains(reference)) {
            Debug.Log($"Duplicated entry key {cells.Key()}");
            continue;
          }

          _convertedIds.Add(reference);
          if (!reference.TryGetEntry(out var entry)
            || entry.GetType() == type) {
            continue;
          }

          Debug.Log($"Recreating {cells.Key()}");
          var id = entry.ID;
          TypewriterDatabase.Instance.RemoveEntry(entry.ID);
          var newEntry = (BaseEntry)Activator.CreateInstance(type);
          newEntry.ID = id;
          TypewriterDatabase.Instance.AddEntry(table, newEntry);
        } else {
          var entry = TypewriterDatabase.Instance.CreateEntry(table, type);
          entry.Key = cells.Key();
          _lookup.Add(cells.Key(), entry.ID);
          _convertedIds.Add(entry.ID);
        }
      }

      for (var i = 1; i < rows.Count; i++) {
        var cells = rows[i].Cells;
        if (cells.IsEmpty()) {
          previous = null;
          continue;
        }

        try {
          switch (cells.Type()) {
            case "fact":
              CreateFactEntry(cells, previous);
              break;
            case "event":
              previous = CreateEventEntry(cells, previous);
              break;
            case "choice":
              previous = CreateChoiceEntry(cells, previous);
              break;
            case "dialogue":
            case "option":
              previous = CreateDialogueEntry(cells, previous);
              break;
          }
        } catch (Exception e) {
          Debug.LogError($"Row {i + 1} ({cells.Key()}): {e.Message}", context);
          Debug.LogException(e);
        }
      }

      for (var i = table.Events.Count - 1; i >= 0; i--) {
        var entry = table.Events[i];
        if (!_convertedIds.Contains(entry.ID)) {
          Debug.Log($"Removing {entry.Key}");
          TypewriterDatabase.Instance.RemoveEntry(entry);
        }
      }
      for (var i = table.Facts.Count - 1; i >= 0; i--) {
        var entry = table.Facts[i];
        if (!_convertedIds.Contains(entry.ID)) {
          Debug.Log($"Removing {entry.Key}");
          TypewriterDatabase.Instance.RemoveEntry(entry);
        }
      }
      for (var i = table.Rules.Count - 1; i >= 0; i--) {
        var entry = table.Rules[i];
        if (!_convertedIds.Contains(entry.ID)) {
          Debug.Log($"Removing {entry.Key}");
          TypewriterDatabase.Instance.RemoveEntry(entry);
        }
      }
    }

    private static BaseEntry CreateFactEntry(
      List<string> cells,
      BaseEntry previous
    ) {
      var entry = _lookup[cells.Key()].GetEntry();
      BeginConversion(entry, cells, previous);
      EndConversion(entry);

      return entry;
    }

    private static BaseEntry CreateEventEntry(
      List<string> cells,
      BaseEntry previous
    ) {
      var entry = _lookup[cells.Key()].GetEntry();
      BeginConversion(entry, cells, previous);
      EndConversion(entry);

      return entry;
    }

    private static BaseEntry CreateChoiceEntry(
      List<string> cells,
      BaseEntry previous
    ) {
      var entry = _lookup[cells.Key()].GetEntry() as ChoiceEntry;
      BeginConversion(entry, cells, previous);
      entry.IsCancellable = cells.Cancel();
      EndConversion(entry);

      return entry;
    }

    private static BaseEntry CreateDialogueEntry(
      List<string> cells,
      BaseEntry previous
    ) {
      var entry = _lookup[cells.Key()].GetEntry() as DialogueEntry;
      BeginConversion(entry, cells, previous);

      if (_lookup.TryGetValue(cells.Speaker(), out var speaker)) {
        entry.Speaker = speaker;
        if (speaker != InteractionContext.CurrentSpeaker) {
          _criteria.Add(
            new BlackboardCriterion {
              Min = 1,
              Max = 1,
              FactReference = speaker == InteractionContext.LT
                ? InteractionContext.IsLTPresent
                : InteractionContext.IsRTPresent,
            }
          );
        }
      } else {
        throw new Exception($"Missing speaker {cells.Speaker()}.");
      }

      /* TODO: Switch to localized strings later
      var textEntry = _collection.StringTables[0].GetEntry(entry.Key);
      long textId;
      if (textEntry == null) {
        Debug.Log($"Missing text entry for {entry.Key}");
        var sharedEntry = _collection.SharedData.AddKey(entry.Key);
        textId = sharedEntry.Id;
        _collection.StringTables[0].AddEntry(textId, cells.Text());
        LocalizationEvents.RaiseTableEntryAdded(_collection, sharedEntry);
      } else {
        textId = textEntry.KeyId;
        textEntry.Value = cells.Text();
        LocalizationEvents.RaiseTableEntryModified(
          _collection.SharedData.GetEntry(textId)
        );
      }

      entry.Text = new LocalizedString(
        _collection.SharedData.TableCollectionName,
        textId
      );
      */
      entry.Content = cells.Text();
      entry.Style = cells.Style() switch {
        "speech" => DialogueEntry.BubbleStyle.Speech,
        "thought" => DialogueEntry.BubbleStyle.Thought,
        "action" => DialogueEntry.BubbleStyle.Action,
        _ => throw new Exception($"Unknown style: {cells.Style()}."),
      };

      if (cells.Actions() != "") {
        _events.Clear();
        foreach (var fact in ParseReferences(cells.Actions())) {
          _events.Add(new RuleEntry.Dispatcher { Reference = fact });
        }
        entry.OnApply = _events.ToArray();
      }

      EndConversion(entry);
      return entry;
    }

    private static void BeginConversion(
      BaseEntry entry,
      List<string> cells,
      BaseEntry previous
    ) {
      _triggers.Clear();
      _criteria.Clear();
      _modifications.Clear();

      entry.Key = cells.Key();
      entry.Scope = InteractionContext.InteractionScope;

      if (cells.Triggers() != "") {
        _triggers.AddRange(ParseReferences(cells.Triggers()));
      } else if (previous != null) {
        _triggers.Add(previous.ID);
      }

      if (cells.Criteria() != "") {
        foreach (var result in ParseScript(cells.Criteria())) {
          _criteria.Add(ParseCriterion(result));
        }
      }

      if (cells.Modifications() != "") {
        foreach (var result in ParseScript(cells.Modifications())) {
          _modifications.Add(ParseModification(result));
        }
      }

      entry.Padding =
        int.TryParse(cells.Padding(), out var padding) ? padding : 0;
      entry.Once = cells.Once();
    }

    private static void EndConversion(BaseEntry entry) {
      entry.Triggers.List = _triggers.ToArray();
      entry.Criteria = _criteria.ToArray();
      entry.Modifications = _modifications.ToArray();
    }

    private static BlackboardModification ParseModification(
      ScriptResult result
    ) {
      if (!_lookup.TryGetValue(result.Fact, out var fact)) {
        throw new Exception($"Missing modification fact {result.Fact}");
      }

      var modification = new BlackboardModification {
        FactReference = fact,
        Value = 0,
      };

      switch (result.Operation) {
        case "=":
          if (_lookup.TryGetValue(result.FirstValue, out var entry)) {
            modification.Value = entry.ID;
            modification.Operation =
              BlackboardModification.OperationType.SetReference;
          } else {
            modification.Value = int.Parse(result.FirstValue);
            modification.Operation = BlackboardModification.OperationType.Set;
          }
          break;
        case "+=":
          modification.Value = int.Parse(result.FirstValue);
          modification.Operation = BlackboardModification.OperationType.Add;
          break;
        case "-=":
          modification.Value = -int.Parse(result.FirstValue);
          modification.Operation = BlackboardModification.OperationType.Set;
          break;
        case "++":
          modification.Value = 1;
          modification.Operation = BlackboardModification.OperationType.Set;
          break;
        case "--":
          modification.Value = -1;
          modification.Operation = BlackboardModification.OperationType.Set;
          break;
        default:
          throw new Exception(
            $"Unknown modification operator {result.Operation}"
          );
      }

      return modification;
    }

    private static BlackboardCriterion ParseCriterion(ScriptResult result) {
      if (!_lookup.TryGetValue(result.Fact, out var fact)) {
        throw new Exception($"Missing criteria fact {result.Fact}");
      }

      var criterion = new BlackboardCriterion {
        FactReference = fact,
        Min = 0,
        Max = 0,
      };
      switch (result.Operation) {
        case "=":
          if (result.Range) {
            criterion.Min = int.Parse(result.FirstValue);
            criterion.Max = int.Parse(result.SecondValue);
            criterion.Operation =
              BlackboardCriterion.OperationType.ClosedInterval;
          } else if (_lookup.TryGetValue(result.FirstValue, out var entry)) {
            criterion.Min = criterion.Max = entry.ID;
            criterion.Operation =
              BlackboardCriterion.OperationType.EqualReference;
          } else {
            criterion.Min = int.Parse(result.FirstValue);
            criterion.Max = criterion.Min;
            criterion.Operation = BlackboardCriterion.OperationType.Equal;
          }
          break;
        case ">":
          criterion.Min = int.Parse(result.FirstValue) + 1;
          criterion.Max = int.MaxValue;
          criterion.Operation = BlackboardCriterion.OperationType.GreaterEqual;
          break;
        case ">=":
          criterion.Min = int.Parse(result.FirstValue);
          criterion.Max = int.MaxValue;
          criterion.Operation = BlackboardCriterion.OperationType.GreaterEqual;
          break;
        case "<":
          criterion.Min = int.MinValue;
          criterion.Max = int.Parse(result.FirstValue) - 1;
          criterion.Operation = BlackboardCriterion.OperationType.LessEqual;
          break;
        case "<=":
          criterion.Min = int.MinValue;
          criterion.Max = int.Parse(result.FirstValue);
          criterion.Operation = BlackboardCriterion.OperationType.LessEqual;
          break;
        default:
          throw new Exception($"Unknown criteria operator {result.Operation}");
      }

      return criterion;
    }

    private static IEnumerable<ScriptResult> ParseScript(string text) {
      var state = ScriptState.Initial;
      var builder = new StringBuilder();
      var result = new ScriptResult();

      if (text[^1] != '\n') {
        text += '\n';
      }

      var isComment = false;
      var context = new ParsingContext {
        Code = text,
        Line = 0,
        Column = 0,
      };

      foreach (var c in text) {
        if (isComment) {
          if (c == '\n') {
            isComment = false;
          }
          continue;
        }

        switch (c) {
          case '\r':
          case ' ':
            continue;
          case '=':
          case '>':
          case '<':
          case '+':
            if (state == ScriptState.Fact) {
              result.Fact = builder.ToString();
              builder.Clear();
            }

            state = ScriptState.Operation;
            builder.Append(c);
            break;
          case '.':
            if (state == ScriptState.FirstValue) {
              result.FirstValue = builder.ToString();
              result.Range = true;
              builder.Clear();
              state = ScriptState.Range;
            } else if (state != ScriptState.Range) {
              throw new ParsingException("Unexpected '.'", context);
            }

            break;
          case '#' when state == ScriptState.Initial:
            isComment = true;
            break;
          case '\n':
            switch (state) {
              case ScriptState.FirstValue:
                result.FirstValue = builder.ToString();
                builder.Clear();
                break;
              case ScriptState.SecondValue:
                result.SecondValue = builder.ToString();
                builder.Clear();
                break;
              case ScriptState.Range:
                throw new ParsingException(
                  "Missing second value in the range.",
                  context
                );
              case ScriptState.Operation:
                result.Operation = builder.ToString();
                builder.Clear();
                if (result.Operation != "++" && result.Operation != "--") {
                  throw new ParsingException(
                    "Missing value on the right side of the operator.",
                    context
                  );
                }
                break;
              case ScriptState.Fact:
                throw new ParsingException("Missing operator.", context);
              case ScriptState.Initial:
                throw new ParsingException("Empty criteria.", context);
              default:
                throw new ArgumentOutOfRangeException();
            }

            yield return result;
            result = new ScriptResult();
            state = ScriptState.Initial;
            context.Column = 0;
            context.Line++;

            break;
          default:
            if (state == ScriptState.Initial) {
              state = ScriptState.Fact;
            } else if (state == ScriptState.Operation) {
              result.Operation = builder.ToString();
              builder.Clear();
              state = ScriptState.FirstValue;
            } else if (state == ScriptState.Range) {
              state = ScriptState.SecondValue;
            }

            builder.Append(c);
            break;
        }
        context.Column++;
      }
    }

    private static IEnumerable<EntryReference> ParseReferences(string text) {
      if (text[^1] != '\n') {
        text += '\n';
      }

      var isComment = false;
      var builder = new StringBuilder();
      foreach (var c in text) {
        switch (c) {
          case ' ':
          case '\r':
            continue;
          case '#' when builder.Length == 0:
            isComment = true;
            break;
          case ',':
          case '\n':
            if (isComment) {
              isComment = false;
              builder.Clear();
              break;
            }

            var value = builder.ToString();
            builder.Clear();
            if (value.Length > 0) {
              if (_lookup.TryGetValue(value, out var entry)) {
                yield return entry;
              } else {
                throw new Exception($"Unknown entry reference: {value}.");
              }
            }
            break;
          default:
            builder.Append(c);
            break;
        }
      }
    }

    private static string GetErrorFrame(ParsingContext context) {
      var lines = context.Code.Split('\n');
      var start = Math.Max(0, context.Line - 3);
      var end = Math.Min(lines.Length, context.Line + 3);
      var builder = new StringBuilder();
      for (var i = start; i < end; i++) {
        var l = lines[i];
        builder.Append(l);
        builder.Append('\n');
        if (i == context.Line) {
          builder.Append(' ', context.Column);
          builder.Append('^');
          builder.Append('\n');
        }
      }

      return builder.ToString();
    }

    private static bool IsEmpty(this List<string> list) =>
      string.IsNullOrEmpty(list[0]) || list[0].StartsWith('#');

    private static string Key(this List<string> list) => list[0];
    private static string Style(this List<string> list) => list[1];
    private static string Type(this List<string> list) => list[2];
    private static string Speaker(this List<string> list) => list[3];
    private static bool Once(this List<string> list) => list[4] == "TRUE";
    private static bool Cancel(this List<string> list) => list[5] == "TRUE";
    private static string Text(this List<string> list) => list[6];
    private static string Triggers(this List<string> list) => list[7];
    private static string Criteria(this List<string> list) => list[8];
    private static string Modifications(this List<string> list) => list[9];
    private static string Padding(this List<string> list) => list[10];
    private static string Actions(this List<string> list) => list[12];
  }
}
