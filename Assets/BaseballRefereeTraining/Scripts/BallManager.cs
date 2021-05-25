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

    public BallInfo(){}
    public BallInfo(BallType _type, float _velo, int _line, int _col)
    {
        type = _type;
        velocity = _velo;
        line = _line;
        column = _col;
    }
}

// 7分割前提
public class BallManager : MonoBehaviour
{
    [SerializeField] bool isSkipWaitTimeInPractice = true;
    [SerializeField] bool isSkipWaitTimeInTest = false;

    [SerializeField] BallSimulator ball;
    [SerializeField] Motion motion;
    [SerializeField] Text board;
    [SerializeField] OVRInput.Button strikeJudgeInput = OVRInput.Button.PrimaryHandTrigger;
    [SerializeField] OVRInput.Button ballJudgeInput = OVRInput.Button.One;
    [SerializeField] OVRInput.Button PauseInput = OVRInput.Button.PrimaryIndexTrigger;
    [SerializeField] OVRInput.Controller controller = OVRInput.Controller.RTouch;
    [SerializeField] float resultTime = 2f;

    [SerializeField] string strikeText = "Strike";
    [SerializeField] string ballText = "Ball";
    [SerializeField] Color defualtTextColor = Color.white;
    [SerializeField] Color pauseTextColor = Color.green;
    [SerializeField] Color correctTextColor = Color.white;
    [SerializeField] Color incorrectTextColor = Color.red;

    private BallType thisBall;
    private float thisVelocity;
    private int thisTargetLine;
    private int thisTargetcolumn;

    private List<BallInfo> m_history;
    private BallInfo[] m_order;
    private int m_orderIndex = 0;
    private bool m_judging = false;
    private bool m_isPause = false;
    private bool m_isStrike;

    private void LoadBallsText(string fileName)
    {
        m_order = BallInfoCSV.Load(fileName);
        m_orderIndex = 0;

        //Debug
        // foreach(BallInfo info in m_order)
        // {
        //     Debug.Log("Type: " + info.type + "\nVelocity: " + info.velocity + "\nLine: " + info.type + "\ncolumn: "  + info.column);
        // }
    }

    public BallType RandomBalls()
    {
        return (BallType)UnityEngine.Random.Range(0, System.Enum.GetNames(typeof(BallType)).Length);
    }

    //todo additional argument: velocity
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

    public void EndPitching(float velocity, int line, int column)
    {
        // todo ここが7分割前提
        line = 6 - line;
        thisTargetLine = line;
        thisTargetcolumn = column;
        thisVelocity = velocity;
        m_isStrike = 5 > line && line > 1 && 5 > column && column > 1;

        m_history.Add(new BallInfo(thisBall, thisVelocity, thisTargetLine, thisTargetcolumn));
    }

    //strike: true
    //ball  : false
    private void Judge(bool userJudgement)
    {
        bool isCorrectAns = m_isStrike == userJudgement;

        board.text = m_isStrike ? strikeText : ballText;
        board.color = isCorrectAns ? correctTextColor : incorrectTextColor;
        if(!isCorrectAns) StartCoroutine(GameManager.instance.Vibrate(controller, 0.5f));

        Debug.Log("thisBall  : " + thisBall);
        Debug.Log("thisVelocity  : " + thisVelocity);
        Debug.Log("thisTarget: (" + thisTargetLine + ", " + thisTargetcolumn + ")");
        Debug.Log("isCorrectAns: " + isCorrectAns);

        //todo save his judgement
    }

    private void Start()
    {
        LoadBallsText("TestBalls");
        m_history = new List<BallInfo>();
    }

    private void Update()
    {
        if(!ball.IsPitching() && OVRInput.GetDown(PauseInput, controller))
        {
            m_isPause = !m_isPause;
            board.text = m_isPause ? "Pause" : "";
            board.color = m_isPause ? pauseTextColor : defualtTextColor;
        }

        if(m_isPause) return;

        if(!m_judging && !ball.IsPitching() && GameManager.instance.GetNowState() == State.Judge)
        {
            if(GameManager.instance.GetNowMode() == Mode.Practice)
            {
                if(!motion.IsAnimating() || isSkipWaitTimeInPractice)
                {
                    thisBall = RandomBalls();
                    SetBallParameter(thisBall);
                    StartPitching();
                    m_judging = true;
                }
            }
            else if(GameManager.instance.GetNowMode() == Mode.Test)
            {
                if(!motion.IsAnimating() || isSkipWaitTimeInTest)
                {
                    if(m_orderIndex == m_order.Length)
                    {
                        GameManager.instance.ChangeStateToSelect();
                        m_orderIndex = 0;
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
        }

        if(m_judging && !ball.IsPitching())
        {
            //Select Strike
            if (OVRInput.GetDown(strikeJudgeInput, controller))
            {
                Judge(true);
                StartCoroutine(GameManager.instance.Wait(resultTime, () => { board.text = ""; }));
                m_judging = false;
            }
            //Select Ball
            else if(OVRInput.GetDown(ballJudgeInput, controller))
            {
                Judge(false);
                StartCoroutine(GameManager.instance.Wait(resultTime, () => { board.text = ""; }));
                m_judging = false;
            }
        }
    }
}
