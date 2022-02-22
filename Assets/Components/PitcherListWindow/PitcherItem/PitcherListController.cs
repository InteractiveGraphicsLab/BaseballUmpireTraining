using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

public class PitcherListController : FilterableListController<Pitcher> {
    [SerializeField] LoadingSlider loadingSlider;

    [SerializeField] InputField uiNameField;
    string uiName = "Shohei";
    [SerializeField] InputField uiIdField;
    int uiId = 300000;

    void OnNameChanged(InputField input) {
        uiName = input.text;
        Filter();
    }

    void OnIdChanged(InputField input) {
        uiId = Int32.Parse(input.text);
        Filter();
    }

    public override void OnPostSetupItems() {
        base.OnPostSetupItems();

        uiNameField.onEndEdit.AddListener(delegate { OnNameChanged(uiNameField); });
        uiNameField.text = uiName;
        uiIdField.onEndEdit.AddListener(delegate { OnIdChanged(uiIdField); });
        uiIdField.text = uiId.ToString();

        conditions.Add(new FilterRequirement((Pitcher p) => p.name.Contains(uiName)));
        conditions.Add(new FilterRequirement((Pitcher p) => uiId <= p.id));
    }


    // string playerDataUrl = "https://raw.githubusercontent.com/chadwickbureau/register/master/data/people.csv";
    // List<string> colsEssential = new List<string>(){
    //         "key_mlbam"
    //     };
    // List<string> colsToKeep = new List<string>(){
    //         "name_last",
    //         "name_first",
    //         "key_mlbam"
    //     };

    // async void Start() {
    //     var l = await GetPitcherData((float v) => loadingSlider.OnLoading(v));
    //     UpdateList(l);

    //     // await UniTask.Delay(5000);
    //     // uiText = "Shoh";
    //     // Filter();
    // }

    // async UniTask<List<Pitcher>> GetPitcherData(System.Action<float> progressHandler = null) {
    //     UnityWebRequest request;

    //     if (progressHandler == null) {
    //         request = await UnityWebRequest.Get(playerDataUrl).SendWebRequest();
    //     } else {
    //         var progress = Progress.Create(progressHandler);

    //         request = await UnityWebRequest.Get(playerDataUrl)
    //             .SendWebRequest()
    //             .ToUniTask(progress: progress);
    //     }

    //     var csvText = request.downloadHandler.text;

    //     await UniTask.SwitchToThreadPool();
    //     var list = await MyCSV.Load(csvText);

    //     var indices = new Dictionary<string, int>();
    //     for (int i = 0; i < list[0].Count; i++)
    //         if (colsToKeep.Contains(list[0][i]))
    //             indices.Add(list[0][i], i);

    //     List<Pitcher> ret = new List<Pitcher>();

    //     await UniTask.Run(() => {
    //         bool b;
    //         for (int i = 1; i < list.Count; i++) {
    //             b = true;
    //             foreach (var d in indices)
    //                 if (colsEssential.Contains(d.Key) && list[i][d.Value] == "")
    //                     b = false;

    //             if (b)
    //                 ret.Add(new Pitcher(Int32.Parse(list[i][indices["key_mlbam"]]), list[i][indices["name_first"]], list[i][indices["name_last"]]));
    //         }
    //     });

    //     await UniTask.SwitchToMainThread();

    //     return ret;
    // }
}
