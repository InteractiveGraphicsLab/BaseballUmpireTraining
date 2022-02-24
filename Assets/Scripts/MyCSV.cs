using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class MyCSV {
    public static async UniTask<List<List<string>>> Load(TextAsset csv) {
        return await Load(csv.text);
    }

    public static async UniTask<List<List<string>>> Load(string csvText) {
        List<List<string>> datas = new List<List<string>>();
        string s;
        int a = 0, b, c;

        Debug.Log("csv: " + (Time.time));

        // await UniTask.SwitchToTaskPool();

        StringReader reader = new StringReader(csvText);
        Debug.Log("csv: " + (Time.time));
        while (reader.Peek() != -1) {
            s = await reader.ReadLineAsync();
            while ((a = s.IndexOf('"', a)) > -1) {
                b = s.IndexOf('"', a + 1);
                c = s.IndexOf(',', a + 1);
                if (c < 0 || b < 0 || s.Length - 1 <= c) break;
                // なんか知らんけど動く
                if (c < b && !(s[c - 1] == '"' || s[c + 1] == '"') && !(s[c - 1] == ',' || s[c + 1] == ','))
                    s = s.Substring(0, c) + s.Substring(c + 1);
                a = b;
            }
            datas.Add(new List<string>(s.Replace("\"", "").Split(',')));
            a = 0;
        }
        Debug.Log("csv: " + (Time.time));
        // await UniTask.SwitchToMainThread();

        return datas;
    }
}
