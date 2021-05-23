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
    [SerializeField] BallSimulator ball;
    [SerializeField] Text board;
    [SerializeField] OVRInput.Button strikeJudgeInput = OVRInput.Button.PrimaryHandTrigger;
    [SerializeField] OVRInput.Button ballJudgeInput = OVRInput.Button.One;
    [SerializeField] OVRInput.Controller controller = OVRInput.Controller.RTouch;

    private List<BallInfo> m_order;
    private int m_orderIndex = 0;
    private bool m_judgeing = false;
    private bool m_isStrike;

    public void RandomPitching()
    {
       SetBallParameter((BallType)Random.Range(0, System.Enum.GetNames(typeof(BallType)).Length));
       ball.StartPitching();
    }

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

    public void EndPitching(int line, int colunm)
    {
        line = 6 - line;
        m_isStrike = 5 > line && line > 1 && 5 > colunm && colunm > 1;
    }

    // private void InitBoard()
    // {
        
    // }

    void Start()
    {
        
    }

    void Update()
    {
        if(GameManager.instance.GetNowState() == State.Judge && !ball.IsPitching() && !m_judgeing)
        {
            if(GameManager.instance.GetNowMode() == Mode.Practice)
            {
                RandomPitching();
            }
            else if(GameManager.instance.GetNowMode() == Mode.Test)
            {
                SetBallParameter(m_order[m_orderIndex++].type);
            }
            ball.StartPitching();
            m_judgeing = true;
        }

        if(m_judgeing)
        {
            board.text = "";

            if (OVRInput.GetUp(strikeJudgeInput, controller))
            {
                //todo save his judgement as Strike
                if(!m_isStrike)
                {
                    //false
                    StartCoroutine(GameManager.instance.Vibrate(controller, 0.5f));
                    board.text = "Strike";
                    board.color = Color.red;
                    Debug.Log("judge false");
                }
                else
                {
                    //true
                    board.text = "Strike";
                    board.color = Color.white;
                    Debug.Log("judge true");
                }

                m_judgeing = false;
            }
            else if(OVRInput.GetUp(ballJudgeInput, controller))
            {
                //todo save his judgement as Ball
                if(m_isStrike)
                {
                    //false
                    StartCoroutine(GameManager.instance.Vibrate(controller, 0.5f));
                    board.text = "Ball";
                    board.color = Color.red;
                    Debug.Log("judge false");
                }
                else
                {
                    //true
                    board.text = "Ball";
                    board.color = Color.white;
                    Debug.Log("judge true");
                }

                m_judgeing = false;
            }
        }
    }
}
