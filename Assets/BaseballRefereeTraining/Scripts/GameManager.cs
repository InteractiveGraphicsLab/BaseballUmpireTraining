using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Mode
{
    Practice, Test
}

public enum State
{
    Select, Judge
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
    [SerializeField] Vector3 refereePos;
    [SerializeField] GameObject teleportation;
    [SerializeField] BallManager ballManager;
    [SerializeField] Text mainBoard;
    [SerializeField] Text subBoard;
    [SerializeField] Text b;
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

    public void SetB(string text = "", Color? color = null)
    {
        b.text = text;
        b.color = color ?? Color.white;
    }

    public void ChangeState(State state)
    {
        m_nowState = state;
        if(state == State.Select)
            InitForSelect();
        else if(state == State.Judge)
            InitForJudge();
    }

    public void ChangeStateToJudge()
    {
        ChangeState(State.Judge);
    }

    public void ChangeStateToSelect()
    {
        ChangeState(State.Select);
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

    public void SetTeleportState(bool isActive)
    {
        teleportation.SetActive(isActive);
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
        avater.position = new Vector3(refereePos.x, avater.position.y, refereePos.z);
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
        SetMainBoard("Select");
        SetSubBoard();
        SetActiveTeleport();
    }

    private void InitForJudge()
    {
        SetMainBoard();
        SetSubBoard();
        SetInactiveTeleport();
        ResetPosition();
    }

    private void Start()
    {
        ChangeStateToSelect();
    }

    private void Update()
    {
        SetB(m_nowMode.ToString() + "\n" + m_nowState.ToString());

        if(m_nowState == State.Select)
        {

        }
        else if(m_nowState == State.Judge)
        {
            ballManager.UpdateFunction();
        }
    }
}
