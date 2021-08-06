using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Mode
{
    Practice, Test, Replay
}

public enum State
{
    Select, Judge, Replay
}

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = GameObject.FindObjectOfType<GameManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject(typeof(GameManager).Name);
                    _instance = obj.AddComponent<GameManager>();
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if(_instance == null)
        {
            _instance = this as GameManager;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    [SerializeField] Transform avater;
    [SerializeField] GameObject teleportation;
    [SerializeField] GameObject teleportPoint;
    [SerializeField] GameObject hitter;
    [SerializeField] GameObject selector;
    [SerializeField] BallManager ballManager;
    [SerializeField] MenuManger menuManger;
    [SerializeField] HistoryMenu historyMenu;
    [SerializeField] Text mainBoard;
    [SerializeField] Text subBoard;
    [SerializeField] Text modeBoard;
    private Vector3 m_refereePos;
    private Quaternion m_refereeRot;
    private State m_nowState = State.Select;
    private Mode m_nowMode = Mode.Practice;

    public State GetNowState()
    {
        return m_nowState;
    }

    public Mode GetNowMode()
    {
        return m_nowMode;
    }

    // public Text GetMainBoardInstance()
    // {
    //     return mainBoard;
    // }

    // public Text GetSubBoardInstance()
    // {
    //     return subBoard;
    // }

    public void SetMainBoard(string text = "", Color? color = null)
    {
        mainBoard.text = text;
        mainBoard.color =  color ?? Color.white;
    }

    public void SetSubBoard(string text = "", Color? color = null)
    {
        subBoard.text = text;
        subBoard.color = color ?? Color.white;
    }

    public void SetModeBoard(string text = "", Color? color = null)
    {
        modeBoard.text = text;
        modeBoard.color = color ?? Color.white;
    }

    public void Play()
    {
        if(m_nowMode == Mode.Replay)
        {
            ChangeStateToReplay();
        }
        else
        {
            ChangeStateToJudge();
        }
    }

    public void ChangeState(State state)
    {
        if(state == State.Select)
        {
            InitForSelect();
        }
        else if(state == State.Judge)
        {
            InitForJudge();
        }
        else if(state == State.Replay)
        {
            InitForReplay();
        }

        m_nowState = state;
    }

    public void ChangeStateToJudge()
    {
        ChangeState(State.Judge);
    }

    public void ChangeStateToSelect()
    {
        ChangeState(State.Select);
    }

    public void ChangeStateToReplay()
    {
        ChangeState(State.Replay);
    }

    public void ChangeMode(Mode mode)
    {
        m_nowMode = mode;
    }

    public void ChangeModeToPractice()
    {
        ChangeMode(Mode.Practice);
    }

    public void ChangeModeToTest()
    {
        ChangeMode(Mode.Test);
    }

    public void ChangeModeToReplay()
    {
        ChangeMode(Mode.Replay);
    }

    public void SetTeleportState(bool isActive)
    {
        teleportation.SetActive(isActive);
        // hitter.SetActive(!isActive);
        hitter.SetActive(true);
    }

    public void SetActiveTeleport()
    {
        SetTeleportState(true);
    }

    public void SetInactiveTeleport()
    {
        SetTeleportState(false);
    }

    public void ResetPosition()
    {
        if(avater.position != m_refereePos)
        {
            avater.position = m_refereePos;
            avater.rotation = m_refereeRot;
        }
    }

    public IEnumerator Vibrate(OVRInput.Controller controller = OVRInput.Controller.Active, float duration = 0.1f, float frequency = 0.3f, float amplitude = 0.3f)
    {
        OVRInput.SetControllerVibration(frequency, amplitude, controller);
        yield return new WaitForSeconds(duration);
        OVRInput.SetControllerVibration(0, 0, controller);
    }

    public IEnumerator Wait(float time, System.Action action)
    {
        yield return new WaitForSeconds(time);
        action();
    }

    private void InitForSelect()
    {
        SetMainBoard("Referee Training");
        SetSubBoard();
        SetModeBoard();
        SetActiveTeleport();
        //todo
        ChangeModeToPractice();
        selector.SetActive(true);
        menuManger.ActiveUI(State.Select);
        historyMenu.InactiveUIs();
    }

    private void InitForJudge()
    {
        SetMainBoard();
        SetSubBoard();
        SetModeBoard(m_nowMode.ToString());
        SetInactiveTeleport();
        ResetPosition();
        ballManager.Init();
        selector.SetActive(false);
        menuManger.ActiveUI(State.Judge);
        historyMenu.InactiveUIs();
    }

    private void InitForReplay()
    {
        SetMainBoard();
        SetSubBoard();
        SetModeBoard("Replay");
        SetActiveTeleport();
        ballManager.Init();
        selector.SetActive(false);
        menuManger.ActiveUI(State.Replay);
        historyMenu.ChangeHistoryToPractice();
    }

    private void Start()
    {
        m_refereePos = avater.position;
        m_refereeRot = avater.rotation;
        ChangeStateToSelect();
    }

    private void Update()
    {
        //todo
        Vector3 pos = avater.transform.localPosition;
        float y = pos.y - 0.3f;
        if(y < 0) y = 0;
        avater.transform.localPosition = new Vector3(avater.transform.localPosition.x, y, avater.transform.localPosition.z);

        if(m_nowState == State.Select)
        {
            SetMainBoard("Referee Training");
            SetModeBoard();
        }
        // else if(m_nowState == State.Judge)
        // {
        //     SetModeBoard("Replay");
        // }
    //     else if(m_nowState == State.Replay)
    //     {

    //     }
    }
}
