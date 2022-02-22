using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class PitcherListWindow : ListWindow {
    [SerializeField] PitcherListController pitcherListCon;
    [SerializeField] GameObject contents;

    public void Start() {
        var blwindow = ListWindow.GetWindow("BallSearchWindow");
        PitcherListItem.AddSelectedListener((Pitcher p) => blwindow.UpdateList(p));

        UniTask.Void(async () => {
            pitcherListCon.UpdateList(await PitcherData.LoadPithcerData(loadingSlider.GetProgressHandler()));
        });
    }

    public override void UpdateList<T>(T param) {
        if (typeof(T) != typeof(List<Pitcher>))
            throw new System.FormatException($"Worng Type. {typeof(T)} is not {typeof(List<Pitcher>)}.");
        pitcherListCon.UpdateList(param as List<Pitcher>);
    }
}
