// BEGIN MIT LICENSE BLOCK //
//
// Copyright (c) 2016 dskjal
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
//
// END MIT LICENSE BLOCK   //

using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class CSV {
    public CSV() {
        columns = new List<List<string>>();
    }
    public int GetColumnCount() { return columns.Count; }
    public int GetRowCount() { return columns.Count > 0 ? columns[0].Count : 0; }
    public List<string> GetRow(int i) {
        if (i >= columns.Count) {
            throw new System.Exception("An invalid column was specified.");
        }
        return columns[i];
    }
    public string this[int row, int column] {
        get { return this.columns[row][column]; }
    }
    public IEnumerator<string> GetEnumerator() {
        foreach (var row in columns) {
            foreach (var t in row) {
                yield return t;
            }
        }
    }

    internal List<List<string>> columns;
}

public static class CSVLoader {
    private static string dataFolderName = "PresetBallData";

    static CSVLoader() {
#if UNITY_EDITOR
#else
		string path = GetFilePath();
		if(!Directory.Exists(path))
			Directory.CreateDirectory(path);
#endif
    }

    private static List<string> getColumn(string text, int startPos, out int endPos) {
        var row = new List<string>();

        int elemStart = startPos;
        int i = startPos;
        bool isContinue = true;
        while (isContinue) {
            switch (text[i]) {
                case '"':
                    ++elemStart;
                    // 対応する " まで読み込む
                    while (++i < text.Length) {
                        if (text[i] == '"') {
                            break;
                        }
                    }
                    break;
                case '\n':
                    isContinue = false;
                    goto case ',';
                case ',':
                    // クォート時の文字列の長さの調整 フォールスルーがあるため改行もチェック
                    var offset = 1;
                    while (text[i - offset] == '"' || text[i - offset] == '\r' || text[i - offset] == '\n') {
                        ++offset;
                    }
                    row.Add(text.Substring(elemStart, i - elemStart - offset + 1));
                    elemStart = i + 1;
                    break;
            }
            ++i;
        }

        endPos = i;
        return row;
    }

    private static string GetFilePath(string fileName = null) {
        string dataPath, path;

#if UNITY_EDITOR
        dataPath = Application.dataPath;
#else
		dataPath = Application.persistentDataPath;
#endif

        if (fileName == null)
            path = Path.Combine(dataPath, dataFolderName);
        else
            path = Path.Combine(dataPath, dataFolderName, fileName);

        return path;
    }

    public static string[] GetFileNameInDataFolder() {
        return Directory.GetFiles(GetFilePath(), "*.csv", System.IO.SearchOption.TopDirectoryOnly);
    }

    public static CSV Load(string fileName) {
        var csv = new CSV();

        using (var sr = new StreamReader(GetFilePath(fileName), System.Text.Encoding.GetEncoding("utf-8"))) {
            var text = sr.ReadToEnd();

            for (int i = 0; i < text.Length;) {
                csv.columns.Add(getColumn(text, i, out i));
            }
        }

        return csv;
    }
}
