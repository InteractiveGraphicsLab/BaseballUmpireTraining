using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BallListWindow : ListWindow {
    [SerializeField] BallListController ballListCon;
    [SerializeField] GameObject contents;
    [SerializeField] Text noDataText;
    [SerializeField] BallInfoPreview preview;
    [SerializeField] MyBallSimulator simulator;

    public Pitcher pitcher { get; private set; }

    public void Start() {
        BallListItem.AddSelectedListener((Ball ball) => {
            preview.UpdateInfo(pitcher, ball);
            simulator.SetBall(ball);
        });
    }

    public void SetActiveContents(bool isActive) {
        contents.SetActive(isActive);
        noDataText.gameObject.SetActive(!isActive);
    }

    public override async void UpdateList<T>(T param) {
        if (typeof(T) != typeof(Pitcher))
            throw new System.FormatException($"Worng Type. {typeof(T)} is not {typeof(Pitcher)}.");

        pitcher = param as Pitcher;
        SetActiveContents(true);
        UnityAction onStart = () => contents.SetActive(false);
        UnityAction onEnd = () => {
            contents.SetActive(true);
            preview.InitPreview();
        };
        var balls = await BallData.LoadBallData(pitcher, loadingSlider.GetProgressHandler(onStart, onEnd));

        if (balls.Count > 0) {
            SetActiveContents(true);
            ballListCon.UpdateList(balls);
        } else {
            SetActiveContents(false);
            noDataText.text = $"No balls of {pitcher.name}";
        }
    }
}
