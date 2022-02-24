using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class PitcherData {
    static string playerDataUrl = "https://raw.githubusercontent.com/chadwickbureau/register/master/data/people.csv";
    static List<string> colsEssential = new List<string>(){
            "key_mlbam"
        };
    static List<string> colsToKeep = new List<string>(){
            "name_last",
            "name_first",
            "key_mlbam"
        };

    public static async UniTask<List<Pitcher>> LoadPithcerData(System.IProgress<float> progressHandler = null) {
        var request = await Request.Get(playerDataUrl, progressHandler);

        var csvText = request.downloadHandler.text;

        // await UniTask.SwitchToThreadPool();
        var list = await MyCSV.Load(csvText);

        var indices = new Dictionary<string, int>();
        for (int i = 0; i < list[0].Count; i++)
            if (colsToKeep.Contains(list[0][i]))
                indices.Add(list[0][i], i);

        List<Pitcher> ret = new List<Pitcher>();

        await UniTask.Run(() => {
            bool b;
            for (int i = 1; i < list.Count; i++) {
                b = true;
                foreach (var d in indices)
                    if (colsEssential.Contains(d.Key) && list[i][d.Value] == "")
                        b = false;

                if (b)
                    ret.Add(new Pitcher(System.Int32.Parse(list[i][indices["key_mlbam"]]), list[i][indices["name_first"]], list[i][indices["name_last"]]));
            }
        });
        UnityEngine.Debug.Log(UnityEngine.Time.time);

        // await UniTask.SwitchToMainThread();

        return ret;
    }
}
