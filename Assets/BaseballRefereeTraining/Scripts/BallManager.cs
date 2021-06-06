using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum BallType
{
    Fast, Curve, Slider, Screw
}

public class BallInfo
{
    public BallType type;
    public float velocity;
    public int line;
    public int column;
    public bool isStrike;

    public BallInfo(){}
    public BallInfo(BallType _type, float _velo, int _line, int _col, bool _str)
    {
        type = _type;
        velocity = _velo;
        line = _line;
        column = _col;
        isStrike = _str;
    }
    public BallInfo(BallInfo info)
    {
        type = info.type;
        velocity = info.velocity;
        line = info.line;
        column = info.column;
        isStrike = info.isStrike;
    }
}

// 7分割前提
public class BallManager : MonoBehaviour
{
    [SerializeField] bool isSkipWaitTimeInPractice = true;
    [SerializeField] bool isSkipWaitTimeInTest = false;

    [SerializeField] BallSimulator ball;
    [SerializeField] Motion motion;
    [SerializeField] BatterZoneManager batterZone;
    [SerializeField] HistoryMenu historyMenu;
    [SerializeField] Transform head;
    [SerializeField] Transform judgeController;
    [SerializeField] OVRInput.Button strikeJudgeInput = OVRInput.Button.PrimaryHandTrigger;
    // [SerializeField] OVRInput.Button ballJudgeInput = OVRInput.Button.One;
    [SerializeField] OVRInput.Button PauseInput = OVRInput.Button.PrimaryIndexTrigger;
    [SerializeField] OVRInput.Controller controller = OVRInput.Controller.RTouch;
    [SerializeField] OVRInput.Controller subController = OVRInput.Controller.LTouch;
    [SerializeField] float resultTime = 2f;

    [SerializeField] string strikeText = "Strike";
    [SerializeField] string ballText = "Ball";
    [SerializeField] Color defualtTextColor = Color.white;
    [SerializeField] Color pauseTextColor = Color.green;
    [SerializeField] Color correctTextColor = Color.white;
    [SerializeField] Color incorrectTextColor = Color.red;
    [SerializeField] string fileName = "TestBalls";
    [SerializeField] float trailTime = 5f;

    private BallType thisBall;
    private float thisVelocity;
    private int thisTargetLine;
    private int thisTargetcolumn;
    private Vector3 thisZonePos;

    private BallInfoCSV m_csv;
    private BallInfo[] m_order;
    private int m_orderIndex = 0;
    private float m_judgementTime = 0;

    private bool m_initFlag = true;
    private bool m_judging = false;
    private bool m_replay = false;
    private bool m_isPause = false;
    private bool m_isStrike;

    private bool m_isJudge = true;

    public void Init()
    {
        m_initFlag = true;
        m_judging = false;

        m_replay = false;

        m_isPause = false;
        ball.Init();
        motion.Init();
        ball.Pause(m_isPause);
        motion.Pause(m_isPause);
        ball.InitPosition();
    }

    public void Replay(bool isJudge = false)
    {
        m_replay = true;

        m_isJudge = isJudge;
        if(m_isJudge)
        {
            GameManager.instance.ResetPosition();
        }
    }

    public void StopReplay()
    {
        m_replay = false;
        Init();
        GameManager.instance.SetModeBoard("Replay");
    }

    public BallType RandomBalls()
    {
        return (BallType)UnityEngine.Random.Range(0, System.Enum.GetNames(typeof(BallType)).Length);
    }

    private void SetBallParameter(BallType type, float velocity = 0f, int line = -1, int column = -1)
    {
        switch(type)
        {
            case BallType.Fast:
                ball.Fastball(velocity, line, column);
                break;
            case BallType.Curve:
                ball.Curveball(velocity, line, column);
                break;
            case BallType.Slider:
                ball.Sliderball(velocity, line, column);
                break;
            case BallType.Screw:
                ball.Screwball(velocity, line, column);
                break;
        }
    }

    private void StartPitching()
    {
        ball.StartPitching();
        motion.StartPitching();
    }

    public void EndPitching(float velocity, int line, int column, Vector3 zpos)
    {
        // todo ここが7分割前提
        line = 6 - line;
        thisTargetLine = line;
        thisTargetcolumn = column;
        thisVelocity = velocity;
        m_isStrike = 5 > line && line > 1 && 5 > column && column > 1;
        thisZonePos = zpos;
    }

    //strike: true
    //ball  : false
    private void Judge(bool userJudgement, float JudgementTime = 0)
    {
        bool isCorrectAns = m_isStrike == userJudgement;
        BallInfo info = new BallInfo(thisBall, thisVelocity, thisTargetLine, thisTargetcolumn, m_isStrike);

        GameManager.instance.SetMainBoard(m_isStrike ? strikeText : ballText, isCorrectAns ? correctTextColor : incorrectTextColor);
        if(!isCorrectAns) StartCoroutine(GameManager.instance.Vibrate(controller, 0.5f));

        //save his judgement
        //todo
        if(GameManager.instance.GetNowMode() == Mode.Test)
            m_csv.Write(info, isCorrectAns, JudgementTime);

        string ballInfoText = thisBall.ToString() + "\n" + thisVelocity.ToString("00.0") + " km/s";
        GameManager.instance.SetSubBoard(ballInfoText);

        historyMenu.AddHistory(info, isCorrectAns, GameManager.instance.GetNowMode());
    }

    public void Pause()
    {
        m_isPause = !m_isPause;
        ball.Pause(m_isPause);
        motion.Pause(m_isPause);
        GameManager.instance.SetMainBoard("");
    }

    // -------------------------
    // - Practice
    // -------------------------
    private void Practice()
    {
        if(ball.IsPitching()) return;

        if (!m_judging)
        {
            if (!motion.IsAnimating() || isSkipWaitTimeInPractice)
            {
                thisBall = RandomBalls();
                SetBallParameter(thisBall);
                ball.Trail();
                StartPitching();
                m_judging = true;
            }
        }

        if (m_judging && OVRInput.GetDown(strikeJudgeInput, controller))
        {
            //Select Strike
            if (head.localPosition.y - 0.15f < judgeController.localPosition.y)
            {
                Judge(true, m_judgementTime);
                batterZone.ShowBallTrace(thisZonePos);
                StartCoroutine(GameManager.instance.Wait(resultTime, () =>
                {
                    GameManager.instance.SetMainBoard();
                    GameManager.instance.SetSubBoard();
                    batterZone.HideBallTrace();
                }));
                m_judging = false;
            }
            //Select Ball
            else
            {
                Judge(false, m_judgementTime);
                batterZone.ShowBallTrace(thisZonePos);
                StartCoroutine(GameManager.instance.Wait(resultTime, () =>
                {
                    GameManager.instance.SetMainBoard();
                    GameManager.instance.SetSubBoard();
                    batterZone.HideBallTrace();
                }));
                m_judging = false;
            }
        }
    }

    // -------------------------
    // - Test
    // -------------------------
    private void Test()
    {
        if(m_judging)
            m_judgementTime += Time.deltaTime;

        // if (!ball.IsPitching())
        //     m_judgementTime += Time.deltaTime;

        if(ball.IsPitching()) return;

        if (!m_judging)
        {
            if (m_initFlag)
            {
                m_orderIndex = 0;
                m_judgementTime = 0;
                ball.Trail();
                m_csv.NewFile();
                m_initFlag = false;
            }

            if (!motion.IsAnimating() || isSkipWaitTimeInTest)
            {
                if (m_orderIndex == m_order.Length)
                {
                    GameManager.instance.ChangeStateToSelect();
                    m_csv.SaveFile();
                    m_initFlag = true;
                }
                else
                {
                    BallInfo info = m_order[m_orderIndex++];
                    thisBall = info.type;
                    SetBallParameter(thisBall, info.velocity, info.line, info.column);
                    StartPitching();
                    m_judging = true;
                }
            }
        }

        if (m_judging && OVRInput.GetDown(strikeJudgeInput, controller))
        {
            //Select Strike
            if (head.localPosition.y - 0.15f < judgeController.localPosition.y)
            {
                Judge(true, m_judgementTime);
                StartCoroutine(GameManager.instance.Wait(resultTime, () =>
                {
                    GameManager.instance.SetMainBoard();
                    GameManager.instance.SetSubBoard();
                }));
                m_judging = false;
            }
            //Select Ball
            else
            {
                Judge(false, m_judgementTime);
                StartCoroutine(GameManager.instance.Wait(resultTime, () =>
                {
                    GameManager.instance.SetMainBoard();
                    GameManager.instance.SetSubBoard();
                }));
                m_judging = false;
            }
        }
    }

    // -------------------------
    // - Replay
    // -------------------------
    private void Replay()
    {
        if(ball.IsPitching()) return;

        if (!m_judging)
        {
            if (!motion.IsAnimating() && m_replay)
            {
                BallInfo info = historyMenu.GetSelectedBallInfo();
                if(info.type != null)
                {
                    thisBall = info.type;
                    SetBallParameter(thisBall, info.velocity, info.line, info.column);
                    ball.Trail(m_isJudge ? 0 : trailTime);
                    StartPitching();
                    m_judging = m_isJudge;
                }
            }
        }

        if(m_isJudge)
        {
            if (m_judging && OVRInput.GetDown(strikeJudgeInput, controller))
            {
                //Select Strike
                if (head.localPosition.y - 0.15f < judgeController.localPosition.y)
                {
                    Judge(true, m_judgementTime);
                    StartCoroutine(GameManager.instance.Wait(resultTime, () =>
                    {
                        GameManager.instance.SetMainBoard();
                        GameManager.instance.SetSubBoard();
                    }));
                    m_judging = false;
                }
                //Select Ball
                else
                {
                    Judge(false, m_judgementTime);
                    StartCoroutine(GameManager.instance.Wait(resultTime, () =>
                    {
                        GameManager.instance.SetMainBoard();
                        GameManager.instance.SetSubBoard();
                    }));
                    m_judging = false;
                }
            }
        }
        else
        {
            batterZone.ShowBatterZone();
        }
    }

    private void Start()
    {
        m_csv = new BallInfoCSV();
        m_order = m_csv.Load(fileName);
    }

    private void Update()
    {
        if(GameManager.instance.GetNowState() == State.Select) return;

        if(m_isPause)
        {
            GameManager.instance.SetMainBoard("Pause", pauseTextColor);
            return;
        }

        switch(GameManager.instance.GetNowMode())
        {
            case Mode.Practice:
                Practice();
                break;
            case Mode.Test:
                Test();
                break;
            case Mode.Replay:
                Replay();
                break;
        }
    }
}
