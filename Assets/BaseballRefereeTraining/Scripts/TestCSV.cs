using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCSV : MonoBehaviour
{
	public UnityEngine.UI.Text text;
	string dataFolderName = "PresetBallData";
    // Start is called before the first frame update
    void Start()
    {
// #if UNITY_EDITOR
// #else
// 		string path = Path.Combine(Application.persistentDataPath, dataFolderName);
// 		if (!Directory.Exists(path))
// 			Directory.CreateDirectory(path);
// #endif
		string s = "";
		string[] aa = CSVLoader.GetFileNameInDataFolder();
		foreach(string a in aa)
			s += $"[data] {a}\n";

		CSV data = CSVLoader.Load("TestBalls.csv");
		var d = data.GetEnumerator();
		while(d.MoveNext()) {
			s += d.Current + ", ";
		}
		text.text = s;
	}

    // Update is called once per frame
    void Update()
    {
        
    }
}
