using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class FFilter : MonoBehaviour {
    List<Ball> pitcherBalls, _filterdBalls;
    public IReadOnlyList<Ball> filterdBalls {
        get {
            if (_filterdBalls == null)
                return new List<Ball>().AsReadOnly();
            else
                return _filterdBalls.AsReadOnly();
        }
    }

    [SerializeField]
    BallListController itemController;

    //speed
    [SerializeField]
    InputField minField, maxField;
    int min, max;
    //type
    [SerializeField]
    Dropdown typeDropdown;
    [SerializeField]
    string none = "None";
    List<string> op;
    string type;

    // async void FilterBalls() {
    //     if (pitcherBalls == null) return;

    //     itemController.OnStartLoading();

    //     _filterdBalls.Clear();

    //     await UniTask.Run(() => {
    //         if (BallData.TryGetType(type, out BallData.Type t)) {
    //             _filterdBalls = pitcherBalls.FindAll(b =>
    //                 min <= b.velocity && b.velocity <= max && b.balltype == t
    //             );
    //         }
    //         // type is none
    //         else {
    //             _filterdBalls = pitcherBalls.FindAll(b =>
    //                 min <= b.velocity && b.velocity <= max
    //             );
    //         }
    //     });

    //     itemController.OnBallListChanged(_filterdBalls);
    // }

    // void OnMinSpeedChanged(string text) {
    //     min = Int32.Parse(minField.text);
    //     FilterBalls();
    // }

    // void OnMaxSpeedChanged(string text) {
    //     max = Int32.Parse(maxField.text);
    //     FilterBalls();
    // }

    // void OnTypeChanged(Dropdown change) {
    //     type = op[change.value];
    //     FilterBalls();
    // }

    async void Start() {
        // _filterdBalls = new List<Ball>();

        // minField.onValueChanged.AddListener(OnMinSpeedChanged);
        // min = Int32.Parse(minField.text);
        // maxField.onValueChanged.AddListener(OnMaxSpeedChanged);
        // max = Int32.Parse(maxField.text);

        // op = new List<string>();
        // op.Add(none);
        // foreach (var t in System.Enum.GetValues(typeof(BallData.Type)))
        //     op.Add(t.ToString());
        // typeDropdown.ClearOptions();
        // typeDropdown.AddOptions(op);
        // typeDropdown.value = 0;
        // typeDropdown.onValueChanged.AddListener(delegate { OnTypeChanged(typeDropdown); });
        // type = op[typeDropdown.value];

        // itemController.UpdateList(await BallData.LoadPitcherData("Ohtani"));

        // pitcherBalls = await BallData.LoadPitcherData("Ohtani");
        // FilterBalls();
    }
}
