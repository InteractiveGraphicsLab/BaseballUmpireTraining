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

    public BallInfo()
    : this(BallType.Fast, 150f, 0, 0, true){}

    public BallInfo(BallInfo i)
    : this(i.type, i.velocity, i.line, i.column, i.isStrike){}

    public BallInfo(BallType _type, float _velo, int _line, int _col, bool _str)
    {
        type = _type;
        velocity = _velo;
        line = _line;
        column = _col;
        isStrike = _str;
    }
}

public class BallManager : MonoBehaviour
{
    [SerializeField] bool isSkipWaitTimeInPractice = true;
    [SerializeField] bool isSkipWaitTimeInTest = false;

    [SerializeField] BallSimulator ball;
    [SerializeField] Motion motion;
    [SerializeField] StrikeZoneManager strikeZone;
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

    public StrikeZoneProperty strikeZoneProperty { get; private set; }

    private BallType m_thisBall;
    private float m_thisVelocity;
    private int m_thisTargetLine;
    private int m_thisTargetColumn;
    private Vector3 m_thisZonePos;
    private bool m_isStrike;

    private BallInfoCSV m_csv;
    private BallInfo[] m_order;
    private int m_orderIndex = 0;
    private float m_judgementTime = 0;

    private bool m_initFlag = true;
    private bool m_judging = false;
    private bool m_replay = false;
    private bool m_isPause = false;

    // private bool m_isJudge = true;

    public void SetStrikeZoneProperty(StrikeZoneProperty s)
    {
        strikeZoneProperty = s;
    }

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
        if(GameManager.instance.GetNowMode() == Mode.Replay)
        {
            strikeZone.ShowStrikeZone();
        }
        else
        {
            strikeZone.Hide();
        }
    }

    public void Replay(bool isJudge = false)
    {
        m_replay = true;

        // m_isJudge = isJudge;
        // if(m_isJudge)
        // {
        //     GameManager.instance.ResetPosition();
        // }
    }

    public void StopReplay()
    {
        Init();
        GameManager.instance.SetModeBoard("Review");
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
        m_thisTargetLine = line;
        m_thisTargetColumn = column;
        m_isStrike = strikeZoneProperty.IsStrike(line, column);
        m_thisVelocity = velocity;
        m_thisZonePos = zpos;
    }

    //strike: true
    //ball  : false
    private void Judge(bool userJudgement, float JudgementTime = 0)
    {
        bool isCorrectAns = m_isStrike == userJudgement;
        BallInfo info = new BallInfo(m_thisBall, m_thisVelocity, m_thisTargetLine, m_thisTargetColumn, m_isStrike);

        GameManager.instance.SetMainBoard(m_isStrike ? strikeText : ballText, isCorrectAns ? correctTextColor : incorrectTextColor);
        if(!isCorrectAns) StartCoroutine(GameManager.instance.Vibrate(controller, 0.5f));

        //save his judgement
        //todo
        if(GameManager.instance.GetNowMode() == Mode.Test)
            m_csv.Write(info, isCorrectAns, JudgementTime);

        string ballInfoText = m_thisBall.ToString() + "\n" + m_thisVelocity.ToString("00.0") + " km/s";
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
                m_thisBall = RandomBalls();
                SetBallParameter(m_thisBall);
                ball.Trail();
                StartPitching();
                m_judging = true;
            }
        }

        if (m_judging && OVRInput.GetDown(strikeJudgeInput, controller))
        {
            bool userJudge = head.localPosition.y - 0.15f < judgeController.localPosition.y;
            Judge(userJudge, m_judgementTime);
            strikeZone.Show(m_thisZonePos);
            StartCoroutine(GameManager.instance.Wait(resultTime, () =>
            {
                GameManager.instance.SetMainBoard();
                GameManager.instance.SetSubBoard();
                strikeZone.Hide();
            }));
            m_judging = false;
        }
    }

    // -------------------------
    // - Test
    // -------------------------
    private void Test()
    {
        if(!ball.IsPitching() && m_judging)
            m_judgementTime += Time.deltaTime;

        // if (!ball.IsPitching())
        //     m_judgementTime += Time.deltaTime;

        if(ball.IsPitching()) return;

        if (!m_judging)
        {
            if (m_initFlag)
            {
                m_orderIndex = 0;
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
                    GameManager.instance.SetModeBoard("Preset Pitching:  " + m_orderIndex + " / " + m_order.Length);
                    m_thisBall = info.type;
                    SetBallParameter(m_thisBall, info.velocity, info.line, info.column);
                    StartPitching();
                    m_judgementTime = 0;
                    m_judging = true;
                }
            }
        }

        if (m_judging && OVRInput.GetDown(strikeJudgeInput, controller))
        {
            bool userJudge = head.localPosition.y - 0.15f < judgeController.localPosition.y;
            Judge(userJudge, m_judgementTime);
            StartCoroutine(GameManager.instance.Wait(resultTime, () =>
            {
                GameManager.instance.SetMainBoard();
                GameManager.instance.SetSubBoard();
            }));
            m_judging = false;
        }
    }

    // -------------------------
    // - Replay
    // -------------------------
    private void Replay()
    {
        if(ball.IsPitching()) return;

        // if (!m_judging)
        // {
            if (!motion.IsAnimating() && m_replay)
            {
                BallInfo info = historyMenu.GetSelectedBallInfo();
                if(info.type != null)
                {
                    m_thisBall = info.type;
                    SetBallParameter(m_thisBall, info.velocity, info.line, info.column);
                    ball.Trail(trailTime);
                    StartPitching();
                    // m_judging = m_isJudge;
                }
            }
        // }

        // if(m_isJudge)
        // {
        //     if (m_judging && OVRInput.GetDown(strikeJudgeInput, controller))
        //     {
        //         //Select Strike
        //         if (head.localPosition.y - 0.15f < judgeController.localPosition.y)
        //         {
        //             Judge(true, m_judgementTime);
        //             StartCoroutine(GameManager.instance.Wait(resultTime, () =>
        //             {
        //                 GameManager.instance.SetMainBoard();
        //                 GameManager.instance.SetSubBoard();
        //             }));
        //             m_judging = false;
        //         }
        //         //Select Ball
        //         else
        //         {
        //             Judge(false, m_judgementTime);
        //             StartCoroutine(GameManager.instance.Wait(resultTime, () =>
        //             {
        //                 GameManager.instance.SetMainBoard();
        //                 GameManager.instance.SetSubBoard();
        //             }));
        //             m_judging = false;
        //         }
        //     }
        // }
        // else
        // {
        //     strikeZone.ShowStrikeZone();
        // }
    }

    private void Start()
    {
        m_csv = new BallInfoCSV();
        m_order = m_csv.Load(fileName);
    }

    private void Update()
    {
        if(strikeZoneProperty == null)
        {
            strikeZoneProperty = strikeZone.GetProperty();
        }

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
