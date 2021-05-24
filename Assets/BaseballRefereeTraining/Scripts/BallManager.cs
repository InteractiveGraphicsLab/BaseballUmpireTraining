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
    public int colunm;
}

// 7分割前提
public class BallManager : MonoBehaviour
{
    [SerializeField] bool isSkipWaitTimeInPractice = true;
    [SerializeField] bool isSkipWaitTimeInTest = false;

    [SerializeField] TextAsset[] textFiles;

    [SerializeField] BallSimulator ball;
    [SerializeField] Motion motion;
    [SerializeField] Text board;
    [SerializeField] OVRInput.Button strikeJudgeInput = OVRInput.Button.PrimaryHandTrigger;
    [SerializeField] OVRInput.Button ballJudgeInput = OVRInput.Button.One;
    [SerializeField] OVRInput.Controller controller = OVRInput.Controller.RTouch;
    [SerializeField] float resultTime = 2f;

    [SerializeField] string strikeText = "Strike";
    [SerializeField] string ballText = "Ball";
    [SerializeField] Color defualtTextColor = Color.white;
    [SerializeField] Color correctTextColor = Color.white;
    [SerializeField] Color incorrectTextColor = Color.red;

    private BallType thisBall;
    private float thisSpeed;
    private int thisTargetLine;
    private int thisTargetColunm;

    private BallInfo[] m_order;
    private int m_orderIndex = 0;
    private bool m_judging = false;
    private bool m_isStrike;

    private bool TryParseToEnum(string s, out BallType bt)
    {
        return Enum.TryParse(s, out bt) && Enum.IsDefined(typeof(BallType), bt);
    }

    private void LoadBallsText(int index = 0)
    {
        string[] splitText = textFiles[index].text.Split(char.Parse("\n"));
        if(m_order == null)
        {
            m_order = new BallInfo[splitText.Length];
        }
        else
        {
            System.Array.Resize<BallInfo>(ref m_order, splitText.Length);
        }

        //splitText[i] = "<Balltype> <velocity> <line> <colunm>"
        for(int i = 0; i < splitText.Length; i++)
        {
            //空白除去
            string[] info = splitText[i].Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
            m_order[i] = new BallInfo();

            if(info.Length != 4)
                Debug.LogError("Line " + i + ": Not enough arguments");

            if(!TryParseToEnum(info[0], out m_order[i].type))
                Debug.LogError("Line " + i + ": There is no definition in BallType: " + m_order[i].type);

            if(!float.TryParse(info[1], out m_order[i].velocity))
                Debug.LogError("Line " + i + ": Input string could not be converted to Velocity <Float>: " + m_order[i].velocity);

            if(!int.TryParse(info[2], out m_order[i].line))
                Debug.LogError("Line " + i + ": Input string could not be converted to Line <Int>: " + m_order[i].line);

            if(!int.TryParse(info[2], out m_order[i].colunm))
                Debug.LogError("Line " + i + ": Input string could not be converted to Colunm <Int>: " + m_order[i].colunm);
        }

        m_orderIndex = 0;

        //Debug
        // foreach(BallInfo info in m_order)
        // {
        //     Debug.Log("Type: " + info.type + "\nVelocity: " + info.velocity + "\nLine: " + info.type + "\nColunm: "  + info.colunm);
        // }
    }

    public BallType RandomBalls()
    {
        return (BallType)UnityEngine.Random.Range(0, System.Enum.GetNames(typeof(BallType)).Length);
    }

    //todo additional argument: velocity
    private void SetBallParameter(BallType type)
    {
        switch(type)
        {
            case BallType.Fast:
                ball.Fastball();
                break;
            case BallType.Curve:
                ball.Curveball();
                break;
            case BallType.Slider:
                ball.Sliderball();
                break;
            case BallType.Screw:
                ball.Screwball();
                break;
        }
    }

    private void StartPitching()
    {
        ball.StartPitching();
        motion.StartPitching();
    }

    public void EndPitching(int line, int colunm)
    {
        // todo ここが7分割前提
        line = 6 - line;
        thisTargetLine = line;
        thisTargetColunm = colunm;
        m_isStrike = 5 > line && line > 1 && 5 > colunm && colunm > 1;
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
        Debug.Log("thisTarget: (" + thisTargetLine + ", " + thisTargetColunm + ")");
        //todo save his judgement
    }

    private void Start()
    {
        LoadBallsText();
    }

    private void Update()
    {
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
                    }
                    else
                    {
                        thisBall = m_order[m_orderIndex++].type;
                        SetBallParameter(thisBall);
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
