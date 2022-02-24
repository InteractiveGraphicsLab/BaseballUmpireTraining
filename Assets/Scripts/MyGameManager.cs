using System.Collections;
using UnityEngine;
using Cysharp.Threading.Tasks;

public enum MyMode {
    Home, Random, Search, Review
}

public class MyGameManager : MonoBehaviour {
    static MyGameManager _instance;
    public static MyGameManager inst {
        get {
            if (_instance == null) {
                _instance = GameObject.FindObjectOfType<MyGameManager>();
                if (_instance == null) {
                    GameObject obj = new GameObject(typeof(MyGameManager).Name);
                    _instance = obj.AddComponent<MyGameManager>();
                }
            }
            return _instance;
        }
    }

    void Awake() {
        if (_instance == null) {
            _instance = this as MyGameManager;
            DontDestroyOnLoad(this.gameObject);
        } else {
            Destroy(this.gameObject);
        }
    }

    [SerializeField] Transform cameraRig;
    [SerializeField] Transform centerEye;
    [SerializeField] OVRInput.Controller controller = OVRInput.Controller.RTouch;
    [SerializeField] OVRInput.Controller subController = OVRInput.Controller.LTouch;
    [SerializeField] Transform judgeController;
    [SerializeField] OVRInput.Button judgeInput = OVRInput.Button.PrimaryHandTrigger;
    [SerializeField] OVRInput.Button pauseInput = OVRInput.Button.PrimaryIndexTrigger;

    [SerializeField] bool isSkipAnimeAfterJudging = true;
    [SerializeField] string strikeText = "Strike";
    [SerializeField] string ballText = "Ball";
    [SerializeField] int resultTime = 2000;//ms
    [SerializeField] Color defualtTextColor = Color.white;
    [SerializeField] Color pauseTextColor = Color.green;
    [SerializeField] Color correctTextColor = Color.white;
    [SerializeField] Color incorrectTextColor = Color.red;
    [SerializeField] float trailTime = 3f;

    [SerializeField] TrailBall trailBall;
    [SerializeField] MyBallSimulator simulator;
    [SerializeField] Motion motion;
    [SerializeField] StrikeZone strikeZone;
    [SerializeField] HistoryMenu historyMenu;
    [SerializeField] ScreenBoard screen;
    [SerializeField] GameObject teleportation;
    [SerializeField] GameObject teleportPoint;
    [SerializeField] GameObject searchWindows;
    [SerializeField] HomeWindow homeWindow;
    [SerializeField] GameObject hitter;
    [SerializeField] MenuManger menuManger;
    [SerializeField] Vector3 initPos;
    [SerializeField] Vector3 initRot;

    MyMode _nowMode = MyMode.Home;
    public MyMode nowMode {
        get => _nowMode;
        set {
            _nowMode = value;
            switch (value) {
                case MyMode.Home:
                    InitForHome();
                    break;
                case MyMode.Random:
                    InitForRandom();
                    break;
                case MyMode.Search:
                    InitForSearch();
                    break;
                case MyMode.Review:
                    InitForReview();
                    break;
            }
        }
    }

    bool isJudging, isStrike, isPause;

    public void SetTeleportState(bool isActive) {
        // teleportation.SetActive(isActive);
        // hitter.SetActive(!isActive);
        hitter.SetActive(true);
    }

    public void ActiveTeleport() {
        SetTeleportState(true);
    }

    public void InactiveTeleport() {
        SetTeleportState(false);
    }

    public void Vibrate(OVRInput.Controller controller = OVRInput.Controller.Active, int duration = 100, float frequency = 0.3f, float amplitude = 0.3f) {
        UniTask.Void(async () => {
            OVRInput.SetControllerVibration(frequency, amplitude, controller);
            await UniTask.Delay(duration);
            OVRInput.SetControllerVibration(0, 0, controller);
        });
    }

    // public void Pause() {
    //     isPause = !isPause;
    //     ball.Pause(isPause);
    //     motion.Pause(isPause);
    //     GameManager.instance.SetMainBoard("");
    // }


    public void GoHome() {
        nowMode = MyMode.Home;
    }

    public void ResetPosition() {
        if (cameraRig.position != initPos) {
            cameraRig.position = initPos;
            cameraRig.rotation = Quaternion.Euler(initRot);
        }
    }

    void InitForHome() {
        screen.Text(main: "Umpire Training");
        // ActiveTeleport();
        searchWindows.SetActive(false);
        homeWindow.gameObject.SetActive(true);

        menuManger.ActiveUI(MyMode.Home);
        // historyMenu.InactiveUIs();
    }

    void UpdateForHome() {

    }

    void InitForRandom() {
        screen.Text(sub: "Random Pithcing");
        // InactiveTeleport();
        // ResetPosition();
        searchWindows.SetActive(false);
        homeWindow.gameObject.SetActive(false);
        trailBall.trailRenderer.enabled = false;

        menuManger.ActiveUI(MyMode.Random);
        // historyMenu.InactiveUIs();
    }

    void UpdateForRandom() {
        if (!isJudging && (!motion.IsAnimating() || isSkipAnimeAfterJudging)) {
            screen.Text(sub: "Random Pithcing");
            strikeZone.Invisualize();
            simulator.ThrowRandom(trailBall.obj);
            motion.StartPitching();
            isJudging = true;
        }

        if (isJudging && OVRInput.GetDown(judgeInput, controller)) {
            bool userJudge = centerEye.localPosition.y - 0.15f < judgeController.localPosition.y;
            bool isStrike = strikeZone.IsStrike(simulator.posOnBase);
            strikeZone.Visualize(simulator.posOnBase);
            if (isStrike)
                screen.MainText(strikeText, userJudge == isStrike ? correctTextColor : incorrectTextColor);
            else
                screen.MainText(ballText, userJudge == isStrike ? correctTextColor : incorrectTextColor);
            screen.InfoText($"{simulator.thrownBall.balltype}\n{simulator.thrownBall.velocity.ToString("F1")} km/h");

            UniTask.Void(async () => {
                await UniTask.Delay(resultTime);
                isJudging = false;
            });
        }
    }

    void InitForSearch() {
        screen.Text(sub: "MLB Search");
        // ActiveTeleport();
        searchWindows.SetActive(true);
        homeWindow.gameObject.SetActive(false);
        trailBall.trailRenderer.enabled = true;

        menuManger.ActiveUI(MyMode.Search);
        // historyMenu.ChangeHistoryToPractice();
    }

    void UpdateForSearch() {
        if (isJudging) {
            bool isStrike = strikeZone.IsStrike(simulator.posOnBase);
            strikeZone.Visualize(simulator.posOnBase);
            screen.MainText(isStrike ? strikeText : ballText, correctTextColor);
            screen.InfoText($"{simulator.thrownBall.balltype}\n{simulator.thrownBall.velocity.ToString("F1")} km/h");

            UniTask.Void(async () => {
                await UniTask.Delay(resultTime);
                isJudging = false;
            });
        }

        if (!isJudging) {
            strikeZone.Invisualize();
            screen.Text(sub: "MLB Search");
            if (OVRInput.GetDown(judgeInput, controller)) {
                trailBall.trailRenderer.Clear();
                motion.StartPitching();
                simulator.Throw(trailBall.obj);
                isJudging = true;
            }
        }
    }

    void InitForReview() {
        screen.Text(sub: "Review");
        // ActiveTeleport();
        searchWindows.SetActive(false);
        homeWindow.gameObject.SetActive(false);

        menuManger.ActiveUI(MyMode.Review);
        // historyMenu.ChangeHistoryToPractice();
    }

    void UpdateForReview() {
        // if (!motion.IsAnimating() && m_replay) {
        //     BallInfo info = historyMenu.GetSelectedBallInfo();
        //     if (info.type != null) {
        //         m_thisBall = info.type;
        //         SetBallParameter(m_thisBall, info.velocity, info.line, info.column);
        //         ball.Trail(trailTime);
        //         StartPitching();
        //         // m_judging = m_isJudge;
        //     }
        // }
    }

    void Start() {
        InitForHome();
    }

    void Update() {
        if (simulator.isPitching) return;

        switch (nowMode) {
            case MyMode.Home:
                UpdateForHome();
                break;
            case MyMode.Random:
                UpdateForRandom();
                break;
            case MyMode.Search:
                UpdateForSearch();
                break;
            case MyMode.Review:
                UpdateForReview();
                break;
        }
    }
}
