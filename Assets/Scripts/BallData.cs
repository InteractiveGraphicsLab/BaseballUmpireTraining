using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.IO;
using Cysharp.Threading.Tasks;

public class BallData {
    static string dest = "https://baseballsavant.mlb.com/statcast_search/csv?hfPT=&hfAB=&hfGT=R%7C&hfPR=&hfZ=&stadium=&hfBBL=&hfNewZones=&hfPull=&hfC=&hfSea=2021%7C&hfSit=&player_type=pitcher&hfOuts=&opponent=&pitcher_throws=&batter_stands=&hfSA=&game_date_gt=&game_date_lt=&hfInfield=&team=&position=&hfOutfield=&hfRO=&home_road=&hfFlag=&hfBBT=&metric_1=&hfInn=&min_pitches=0&min_results=0&group_by=name&sort_col=pitches&player_event_sort=api_p_release_speed&sort_order=desc&min_pas=0&type=details&";

    public enum Type {
        Fastball,
        Fourseam,
        Sinker,
        Cutter,
        Changeup,
        Splitter,
        Slider,
        Curve
    }

    public static bool TryGetType(string s, out Type t) {
        return Enum.TryParse(char.ToUpper(s[0]) + s.Substring(1), out t) && Enum.IsDefined(typeof(Type), t);
    }

    // ###################
    // ### Active Spin ###

    // https://baseballsavant.mlb.com/leaderboard/active-spin
    public static string activeSpinFileName = "active-spin";
    static string activeSpinPrefix = "active_spin_";
    // ActiveSpinのデータのインデックス
    public class ActiveSpinInfo {
        public int index { get; private set; }
        public Type ballType { get; private set; }
        public float sum { get; private set; }
        public int num { get; private set; }
        public float mean { get { return sum / (float)num; } }

        public ActiveSpinInfo(int i, Type t) {
            index = i;
            ballType = t;
            sum = 0;
            num = 0;
        }

        public void Add(float d) {
            sum += d;
            num++;
        }
    }
    static List<ActiveSpinInfo> meanActiveSpins;
    static List<List<string>> activeSpinsData;

    static string activeSpinPath {
        get {
#if UNITY_EDITOR
            return $"BaseballSavant/{activeSpinFileName}";
#else
            return $"BaseballSavant/{activeSpinFileName}";
#endif
        }
    }

    public static bool TryGetTypeOnActiveSpinData(string s, out Type t) {
        t = Type.Fastball;
        int p = s.IndexOf(activeSpinPrefix);
        return p > -1 && TryGetType(s.Substring(p + activeSpinPrefix.Length), out t);
    }

    public static async UniTask LoadActiveSpins() {
        TextAsset asDataText; // TextAsset
        meanActiveSpins = new List<ActiveSpinInfo>();
        activeSpinsData = new List<List<string>>();

#if UNITY_EDITOR
        asDataText = await Resources.LoadAsync(activeSpinPath) as TextAsset;
#else
        asDataText = await Resources.LoadAsync(activeSpinPath) as TextAsset;
#endif
        if (asDataText == null) Debug.LogError("There is no file");

        activeSpinsData = await MyCSV.Load(asDataText);

        for (int i = 0; i < activeSpinsData[0].Count; i++)
            if (TryGetTypeOnActiveSpinData(activeSpinsData[0][i], out Type ty))
                meanActiveSpins.Add(new ActiveSpinInfo(i, ty));

        if (meanActiveSpins.Count == 0) Debug.LogError("Active Spin CSV has no data");

        for (int i = 1; i < activeSpinsData.Count; i++)
            for (int j = 0; j < meanActiveSpins.Count; j++)
                if (Single.TryParse(activeSpinsData[i][meanActiveSpins[j].index], out float v))
                    meanActiveSpins[j].Add(v);

        // for (int i = 0; i < meanActiveSpins.Count; i++) {
        //     Debug.Log($"type: {meanActiveSpins[i].ballType}, mean: {meanActiveSpins[i].mean}\n sum: {meanActiveSpins[i].sum}, num: {meanActiveSpins[i].num}, index: {meanActiveSpins[i].index}");
        // }
    }

    static ActiveSpinInfo GetActiveSpinInfo(Type type) {
        if (meanActiveSpins == null) Debug.LogError("No active spin data");
        return meanActiveSpins.Find(a => a.ballType == type);
    }

    public static float GetActiveSpin(Type type, string lastName, string firstName) {
        string last, first;

        for (int i = 0; i < activeSpinsData.Count; i++) {
            last = activeSpinsData[i][0].Trim();
            first = activeSpinsData[i][1].Trim();
            if (last == lastName && first == firstName && Single.TryParse(activeSpinsData[i][GetActiveSpinInfo(type).index], out float res))
                return res;
            // Debug.Log($"last:{last},{lastName}, {last == lastName}");
            // Debug.Log($"first:{first},{firstName}, {first == firstName}");
            // if (last == lastName && first == firstName) {
            //     if (Single.TryParse(activeSpinsData[i][GetActiveSpinInfo(type).index], out float res)) {
            //         return res;
            //     }
            // }
        }
        return GetActiveSpinInfo(type).mean;
    }

    public static float GetActiveSpin(Type type, string name) {
        string[] names = name.Substring(1, name.Length - 2).Split(' ');
        return GetActiveSpin(type, names[0], names[1]);
    }

    // ####################
    // ### Pitcher Data ###

    static Dictionary<string, Type> types;
    static Dictionary<string, int> ic;
    static List<Ball> _balls;
    public IReadOnlyList<Ball> balls {
        get {
            if (_balls == null)
                return new List<Ball>().AsReadOnly();
            else
                return _balls.AsReadOnly();
        }
    }

    static List<string> colsToKeep = new List<string>(){
            "pitch_type",
            "release_speed",
            "release_pos_x",
            "release_pos_y",
            "release_pos_z",
            "player_name",
            "p_throws",
            "plate_x",
            "plate_z",
            "release_spin_rate",
            "spin_axis"
        };

    public static List<Ball> GetBallsCopy() {
        return new List<Ball>(_balls);
    }

    static void CreateTypeDict() {
        types = new Dictionary<string, Type>();
        types.Add("FF", Type.Fourseam);
        types.Add("SI", Type.Sinker);
        types.Add("FC", Type.Cutter);
        types.Add("CH", Type.Changeup);
        types.Add("FS", Type.Splitter);
        types.Add("SL", Type.Slider);
        types.Add("CU", Type.Curve);
        types.Add("FA", Type.Fastball);
    }

    static string PitchingDataPath(string filename) {
#if UNITY_EDITOR
        return $"BaseballSavant/{filename}";
#else
        return $"BaseballSavant/{activeSpinFileName}.csv";
#endif
    }

    public static async UniTask<List<Ball>> LoadBallData(Pitcher p, IProgress<float> progressHandler = null) {
        // TextAsset tpd; // TextAsset
        List<List<string>> data = new List<List<string>>();
        _balls = new List<Ball>();

        // https://baseballsavant.mlb.com/statcast_search
        // から得られる選手の投球データのみ対応
        var pitchingReq = await Request.Get(dest + $"player_id={p.id}", progressHandler);
        var csvText = pitchingReq.downloadHandler.text;
        if (csvText[0] == 0xFEFF) csvText = csvText.Substring(1); //BOM
        // tpd = await Resources.LoadAsync(PitchingDataPath(filename)) as TextAsset;
        // if (tpd != null) data = await MyCSV.Load(tpd);

        if (csvText != null) data = await MyCSV.Load(csvText);

        // https://baseballsavant.mlb.com/statcast_search
        // 軌道計算に必要なデータのインデックスを取得
        ic = new Dictionary<string, int>();
        for (int i = 0; i < data[0].Count; i++) {
            // var col = data[0][i].Replace("\"", "");
            if (colsToKeep.Contains(data[0][i]))
                ic.Add(data[0][i], i);
        }

        if (activeSpinsData == null) await LoadActiveSpins();

        if (types == null) CreateTypeDict();

        await UniTask.Run(() => {
            for (int i = 1; i < data.Count; i++) {
                if (TryComposeBall(data[i], out Ball b)) {
                    _balls.Add(b);
                }
            }
        });

        return GetBallsCopy();
    }

    static Ball ComposeBall(IReadOnlyList<string> d) {
        if (types == null) CreateTypeDict();
        string n = String.Join(" ", d[ic["player_name"]].Split(','));
        Type t = types[d[ic["pitch_type"]]];
        float v = Single.Parse(d[ic["release_speed"]]);
        float rpx = -Single.Parse(d[ic["release_pos_x"]]);
        float rpy = Single.Parse(d[ic["release_pos_y"]]);
        float rpz = -Single.Parse(d[ic["release_pos_z"]]);
        float sax = d[ic["spin_axis"]] == "" ? 0 : Single.Parse(d[ic["spin_axis"]]);
        float asp = GetActiveSpin(t, d[ic["player_name"]]);
        int spr = d[ic["spin_axis"]] == "" ? 0 : Int32.Parse(d[ic["release_spin_rate"]]);
        float px = Single.Parse(d[ic["plate_x"]]);
        float py = Single.Parse(d[ic["plate_z"]]);

        return Ball.Compose(n, t, d[ic["p_throws"]], v, rpx, rpy, rpz, sax, asp, spr, px, py);
    }

    static bool TryComposeBall(IReadOnlyList<string> d, out Ball ball) {
        // Debug.Log($"d.Count: {d.Count}");
        // Debug.Log($"type: {types.ContainsKey(d[ic["pitch_type"]])}, {d[ic["pitch_type"]]}");
        // Debug.Log($"Speed: {Single.TryParse(d[ic["release_speed"]], out float av)}, {d[ic["release_speed"]]}");
        // Debug.Log($"relPosX: {Single.TryParse(d[ic["release_pos_x"]], out float arpx)}, {d[ic["release_pos_x"]]}");
        // Debug.Log($"relPosY: {Single.TryParse(d[ic["release_pos_y"]], out float arpy)}, {d[ic["release_pos_y"]]}");
        // Debug.Log($"relPosZ: {Single.TryParse(d[ic["release_pos_z"]], out float arpz)}, {d[ic["release_pos_z"]]}");
        // Debug.Log($"spinAxis: {Single.TryParse(d[ic["spin_axis"]], out float asax)}, {d[ic["spin_axis"]]}");
        // Debug.Log($"spenRate: {Int32.TryParse(d[ic["release_spin_rate"]], out int aspr)}, {d[ic["release_spin_rate"]]}");
        // Debug.Log($"plateX: {Single.TryParse(d[ic["plate_x"]], out float apx)}, {d[ic["plate_x"]]}");
        // Debug.Log($"platey: {Single.TryParse(d[ic["plate_z"]], out float apy)}, {d[ic["plate_z"]]}");

        // release_pos_z, release_pos_y はこれで正しい
        if (types.ContainsKey(d[ic["pitch_type"]]) &&
            Single.TryParse(d[ic["release_speed"]], out float v) &&
            Single.TryParse(d[ic["release_pos_x"]], out float rpx) &&
            Single.TryParse(d[ic["release_pos_z"]], out float rpy) &&
            Single.TryParse(d[ic["release_pos_y"]], out float rpz) &&
            Single.TryParse(d[ic["spin_axis"]], out float sax) &&
            Int32.TryParse(d[ic["release_spin_rate"]], out int spr) &&
            Single.TryParse(d[ic["plate_x"]], out float px) &&
            Single.TryParse(d[ic["plate_z"]], out float py)
        ) {
            string n = String.Join(" ", d[ic["player_name"]].Split(','));
            Type t = types[d[ic["pitch_type"]]];
            ball = Ball.Compose(n, t, d[ic["p_throws"]], v, -rpx, rpy, -rpz, sax, GetActiveSpin(t, d[ic["player_name"]]), spr, px, py);
            return true;
        } else {
            ball = new Ball();
            return false;
        }
    }

    public static Ball ComposeBallOfIndex(int i) {
        return _balls[i];
    }
}
