using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

public class TestAPI : MonoBehaviour {
    public UnityEngine.UI.Text t;
    public UnityEngine.UI.Text p;
    public UnityEngine.UI.Image image;
    public LoadingSlider slider;
    public string playername = "Shohei_Ohtani";
    List<List<string>> playerData;

    string dest = "https://baseballsavant.mlb.com/statcast_search/csv?hfPT=&hfAB=&hfGT=R%7C&hfPR=&hfZ=&stadium=&hfBBL=&hfNewZones=&hfPull=&hfC=&hfSea=2021%7C&hfSit=&player_type=pitcher&hfOuts=&opponent=&pitcher_throws=&batter_stands=&hfSA=&game_date_gt=&game_date_lt=&hfInfield=&team=&position=&hfOutfield=&hfRO=&home_road=&hfFlag=&hfBBT=&metric_1=&hfInn=&min_pitches=0&min_results=0&group_by=name&sort_col=pitches&player_event_sort=api_p_release_speed&sort_order=desc&min_pas=0&type=details&";

    //https://github.com/jldbc/pybaseball/blob/7dd82fc0e058f3a8d48b5032c06b9ad975f6140b/pybaseball/playerid_lookup.py#L22
    string players = "https://raw.githubusercontent.com/chadwickbureau/register/master/data/people.csv";

    string imgUrl = "https://en.wikipedia.org/api/rest_v1/page/summary/";

    List<string> colsEssential = new List<string>(){
            "key_mlbam"
        };
    List<string> colsToKeep = new List<string>(){
            "name_last",
            "name_first",
            "key_mlbam"
        };

    // public async UniTask<string> GetMLBPlayerData() {
    //     var csv = await GetCSVData(players);

    //     // Debug.Log(System.String.Join(", ", csv[0]));

    //     if (csv.Count <= 0)
    //         Debug.LogError("No player CSV data is found.");

    //     var indices = new Dictionary<string, int>();
    //     for (int i = 0; i < csv[0].Count; i++) {
    //         if (colsToKeep.Contains(csv[0][i])) {
    //             indices.Add(csv[0][i], i);
    //         }
    //     }
    //     Debug.Log("Got indices.");

    //     var playerCsv = await CastPlayerData(csv, indices, colsEssential);
    //     Debug.Log("Got player CSV data.");

    //     var debug = new List<string>();
    //     for (int i = 0; i < playerCsv.Count; i++) {
    //         debug.Add(System.String.Join(",", playerCsv[i]));
    //     }
    //     Debug.Log(System.String.Join("\n", debug));

    //     return GetId("Shohei", "Ohtani", playerCsv);
    // }

    // async UniTask<List<List<string>>> GetCSVData(string url) {
    //     var result = await Request(url);
    //     Debug.Log("Got webRequest result.");

    //     return await MyCSV.Load(result);
    // }

    void ParseCSVData(IReadOnlyList<string> list, Dictionary<string, int> dict) {
        var l = new List<string>();

        foreach (var d in dict) {
            if (colsEssential.Contains(d.Key) && list[d.Value] == "") {
                l.Clear();
                break;
            }
            l.Add(list[d.Value]);
        }
        if (l.Count > 0) playerData.Add(l);
    }

    async UniTask ParseCSVDatas(IReadOnlyList<IReadOnlyList<string>> list, int sn, int n, Dictionary<string, int> dict) {
        Debug.Log($"Start {sn}, {n}");
        await UniTask.Run(() => {
            for (int i = sn; i < list.Count && i < sn + n; i++)
                ParseCSVData(list[i], dict);
        });
        Debug.Log($"End {sn}");
    }

    async void CastMBLPlayerData(IReadOnlyList<IReadOnlyList<string>> list) {
        var indices = new Dictionary<string, int>();
        for (int i = 0; i < list[0].Count; i++) {
            if (colsToKeep.Contains(list[0][i])) {
                indices.Add(list[0][i], i);
                // Debug.Log($"{list[0][i]}, {i}");
            }
        }

        playerData = new List<List<string>>();
        // Debug.Log(list[0].Count);
        Debug.Log(list.Count);

        await UniTask.SwitchToTaskPool();
        Debug.Log("SwitchToTaskPool");

        // List<UniTask> tasks = new List<UniTask>();
        // for (int i = 0; i < list.Count; i++) {
        //     var l = list[i];
        //     ParseCSVData(list[i], indices);
        //     tasks.Add(
        //         UniTask.Run(() => {
        //             Debug.Log($"{i}, {list[i][0]}");
        //             ParseCSVData(list[i], indices);
        //         })
        //     );
        // }

        await UniTask.Run(() => {
            for (int i = 0; i < list.Count; i++)
                ParseCSVData(list[i], indices);
        });
        // int n = 1000;
        // int sn = 0;
        // while (sn < list.Count) {
        //     // tasks.Add(
        //     //     ParseCSVDatas(list, sn, n, indices)
        //     // );
        //     await ParseCSVDatas(list, sn, n, indices);
        //     sn += n;
        // }

        Debug.Log($"End loop");

        // await UniTask.WhenAll(tasks);
        // Debug.Log("End Tasks");

        await UniTask.SwitchToMainThread();
        Debug.Log("SwitchToMainThread");

        Debug.Log(playerData.Count);
        Debug.Log(playerData[0].Count);
        Debug.Log(playerData[0][0]);

        // string s = "";
        // foreach (var l in playerData) {
        //     s += System.String.Join(", ", l) + "\n";
        // }
        // t.text = s;
    }

    async UniTask<List<List<string>>> CastPlayerData(IReadOnlyList<IReadOnlyList<string>> list, Dictionary<string, int> indices, IReadOnlyList<string> colsEss) {
        List<List<string>> ret = new List<List<string>>();
        for (int i = 0; i < list.Count; i++) {
            var l = new List<string>();

            foreach (var d in indices) {
                if (colsEss.ToList().Contains(d.Key) && list[i][d.Value] == "") {
                    l.Clear();
                    break;
                }
                l.Add(list[i][d.Value]);
            }
            if (l.Count > 0) ret.Add(l);
        }
        return ret;
    }

    async UniTask<string> GetId(string firstname, string lastname) {
        return await UniTask.Run(() => {
            for (int i = 0; i < playerData.Count; i++) {
                if (playerData[i][1] == lastname && playerData[i][2] == firstname) {
                    return playerData[i][0];
                }
            }
            return "";
        });
    }

    async UniTask<UnityWebRequest> RequestGet(string url, System.Action<float> progressHandler = null) {
        var progress = Progress.Create(progressHandler);

        var request = await UnityWebRequest.Get(url)
            .SendWebRequest()
            .ToUniTask(progress: progress);

        return request;
    }

    async UniTask Test() {
        // var progress = Progress.Create<float>(x => p.text = x.ToString());

        // var request = UnityWebRequest.Get(players);
        // // request.downloadHandler = new CSVDownloadHander();

        // await request.SendWebRequest()
        //   .ToUniTask(progress: progress);

        // await UniTask.SwitchToTaskPool();

        await UniTask.Delay(3000);

        var playerReq = await RequestGet(players, (float x) => slider.OnLoading(x));

        var list = await MyCSV.Load(playerReq.downloadHandler.text);

        var indices = new Dictionary<string, int>();
        for (int i = 0; i < list[0].Count; i++) {
            if (colsToKeep.Contains(list[0][i])) {
                indices.Add(list[0][i], i);
                // Debug.Log($"{list[0][i]}, {i}");
            }
        }

        playerData = new List<List<string>>();
        await UniTask.Run(() => {
            for (int i = 0; i < list.Count; i++)
                ParseCSVData(list[i], indices);
        });

        string s = "";
        for (int i = 0; i < 30; i++)
            s += System.String.Join(",", playerData[i]) + "\n";
        t.text = s;

        // var id = await GetId("Shohei", "Ohtani");
        // Debug.Log(id);
        // var pitchingReq = await RequestGet(dest + $"player_id={id}", (float x) => slider.OnLoading(x));
        // var _list = await  .Load(pitchingReq.downloadHandler.text);

        // s = "";
        // for (int i = 0; i < 30; i++)
        //     s += System.String.Join(",", _list[i]) + "\n";
        // t.text = s;
        // return "l[0][0]";

        // await UniTask.SwitchToMainThread();
    }

    async UniTask<Texture> GetPlayerImage(string keyword) {
        Debug.Log($"Get img URL\n{imgUrl + keyword}");
        var jsonReq = await RequestGet(imgUrl + keyword);
        var json = JsonUtility.FromJson<WikipediaJsonResponse>(jsonReq.downloadHandler.text);

        Debug.Log($"Get img from {json.originalimage.source}");
        var uwr = UnityWebRequestTexture.GetTexture(json.originalimage.source);
        await uwr.SendWebRequest();
        return DownloadHandlerTexture.GetContent(uwr);
    }

    // Start is called before the first frame update
    async void Start() {
        // t.text = await GetMLBPlayerData();
        // await Test();
        var texture = await GetPlayerImage(playername);
        Debug.Log($"fetch texture: {texture}");
        var texture2d = texture.ClippingSquare();
        image.sprite = Sprite.Create(texture2d, new Rect(0, 0, texture2d.width, texture2d.height), Vector2.zero);
    }

    // Update is called once per frame
    void Update() {

    }
}
