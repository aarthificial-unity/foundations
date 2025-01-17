﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DevTools.CSV {
  public static class CsvParser {
    [Serializable]
    public struct Row {
      public List<string> Cells;
    }

    private static readonly StringBuilder _builder = new();
    private static readonly List<Row> _rows = new();

    public static IReadOnlyList<Row> Parse(string text) {
      var cells = new List<string>();

      var isInsideField = false;
      _builder.Clear();
      _rows.Clear();

      for (var i = 0; i < text.Length; i++) {
        var c = text[i];
        switch (c) {
          case '\r':
            continue;
          case '"': {
            isInsideField = !isInsideField;
            if (!isInsideField
              && i + 1 < text.Length
              && text[i + 1] != ','
              && text[i + 1] != '\n') {
              _builder.Append('"');
            }
            break;
          }
          case ',' when !isInsideField:
            AddCell(cells);
            break;
          case '\n' when !isInsideField:
            AddCell(cells);
            if (cells.Count > 0) {
              _rows.Add(new Row { Cells = cells });
            }
            cells = new List<string>();
            break;
          default:
            _builder.Append(c);
            break;
        }
      }

      return _rows;
    }

    private static void AddCell(List<string> cells) {
      cells.Add(_builder.ToString());
      _builder.Clear();
    }
  }
}
