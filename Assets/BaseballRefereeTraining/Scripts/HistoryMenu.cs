using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;

public class HistoryComponent
{
    public BallInfo ballInfo;
    public bool isCorrect;
    public Panel panel;

    public HistoryComponent(BallInfo _info, bool _isCorrect, Panel _panel)
    {
        ballInfo = _info;
        isCorrect = _isCorrect;
        panel = _panel;
    }
    public HistoryComponent(HistoryComponent com)
    {
        ballInfo = com.ballInfo;
        isCorrect = com.isCorrect;
        panel = com.panel;
    }
}

public class HistoryMenu : MonoBehaviour
{
    [SerializeField] bool isDebug = false;
    [SerializeField] GameObject practiceObject;
    [SerializeField] GameObject testObject;
    [SerializeField] Transform practiceParent;
    [SerializeField] Transform testParent;
    [SerializeField] Scrollbar practiceScrollbar;
    [SerializeField] Scrollbar testScrollbar;
    [SerializeField] Text header;
    // [SerializeField] BallManager ballManager;
    [SerializeField] GameObject panelPrefab;
    [SerializeField] float parentHeight;
    [SerializeField] float scrollSpeed = 3f;

    private List<HistoryComponent> m_practiceHistory;
    private List<HistoryComponent> m_testHistory;
    private List<HistoryComponent> m_nowHistory;
    private Scrollbar m_nowScrollbar;
    private int m_selected = 0;
    private int m_topIndex = 0;
    private int m_maxCapacity = 0;
    private float m_futureHieght = 1f;
    private float m_panelHeight;
    private float m_minHeight;
    private float m_maxHeight;
    private float m_perHieght;
    private float m_t;
    private bool m_scrollUp;
    private bool m_scrollDown;

    public void InactiveUIs()
    {
        practiceObject.SetActive(false);
        testObject.SetActive(false);
        header.gameObject.SetActive(false);
    }

    public void ChangeHistoryToPractice()
    {
        ChangeHistory(Mode.Practice);
    }

    public void ChangeHistoryToTest()
    {
        ChangeHistory(Mode.Test);
    }

    private void ChangeHistory(Mode mode)
    {
        InactiveUIs();
        switch(mode)
        {
            case Mode.Practice:
                m_nowHistory = m_practiceHistory;
                m_nowScrollbar = practiceScrollbar;
                practiceObject.SetActive(true);
                header.text = "Random Pitching";
                break;
            case Mode.Test:
                m_nowHistory = m_testHistory;
                m_nowScrollbar = testScrollbar;
                testObject.SetActive(true);
                header.text = "Preset Pitching";
                break;
        }

        header.gameObject.SetActive(true);
        m_selected = m_nowHistory.Count - 1;
        m_topIndex = m_nowHistory.Count - 1;
        Reflesh();
    }

    public void Next()
    {
        if(m_nowHistory.Count == 0) return;

        m_selected--;
        if(0 > m_selected)
        {
            m_selected = m_nowHistory.Count - 1;
            ScrollTop();
        }
        else
        {
            ScrollDown();
        }

        Reflesh();
    }

    public void Back()
    {
        if(m_nowHistory.Count == 0) return;

        m_selected++;
        if(m_nowHistory.Count - 1 < m_selected)
        {
            m_selected = 0;
            ScrollBottom();
        }
        else
        {
            ScrollUp();
        }

        Reflesh();
    }

    private void ScrollUp()
    {
        if(m_nowHistory.Count * m_panelHeight > parentHeight && m_topIndex + 1 == m_selected)
        {
            m_perHieght = m_panelHeight / ((m_nowHistory.Count * m_panelHeight) - parentHeight);
            m_t = 0;
            m_topIndex++;
            m_minHeight = m_nowScrollbar.value;
            m_maxHeight = GetFutureHeight(m_topIndex);
            m_scrollUp = true;
        }
    }

    private void ScrollDown()
    {
        if(m_nowHistory.Count * m_panelHeight > parentHeight && m_topIndex - m_maxCapacity == m_selected)
        {
            m_perHieght = m_panelHeight / ((m_nowHistory.Count * m_panelHeight) - parentHeight);
            m_t = 1f;
            m_topIndex--;
            m_minHeight = GetFutureHeight(m_topIndex);
            m_maxHeight = m_nowScrollbar.value;
            m_scrollDown = true;
        }
    }

    private void ScrollTop()
    {
        if(m_nowHistory.Count * m_panelHeight > parentHeight)
        {
            m_perHieght = m_panelHeight / ((m_nowHistory.Count * m_panelHeight) - parentHeight);
            m_t = 0;
            m_topIndex = m_nowHistory.Count - 1;
            m_minHeight = m_nowScrollbar.value;
            m_maxHeight = 1f;
            m_scrollUp = true;
        }
    }

    private void ScrollBottom()
    {
        if(m_nowHistory.Count * m_panelHeight > parentHeight)
        {
            m_perHieght = m_panelHeight / ((m_nowHistory.Count * m_panelHeight) - parentHeight);
            m_t = 1f;
            m_topIndex = m_maxCapacity - 1;
            m_minHeight = 0;
            m_maxHeight = m_nowScrollbar.value;
            m_scrollDown = true;
        }
    }

    public float GetFutureHeight(int index)
    {
        int i = m_nowHistory.Count - 1 - index;
        return 1f - m_perHieght * i;
    }

    public BallInfo GetSelectedBallInfo()
    {
        if(m_nowHistory.Count == 0) return new BallInfo();
        else return m_nowHistory[m_selected].ballInfo;
    }

    private Transform GetParent(Mode mode)
    {
        switch(mode)
        {
            case Mode.Practice:
                return practiceParent;
            case Mode.Test:
                return testParent;
            default:
                return new GameObject().transform;
        }
    }

    private void SetComponent(HistoryComponent com, ref List<HistoryComponent> list)
    {
        int falseCount = 0;
        foreach(HistoryComponent h in list)
        {
            if(!h.isCorrect) falseCount++;
        }

        if(com.isCorrect)
        {
            //correct
            com.panel.gameObject.transform.SetSiblingIndex(falseCount);
            list.Insert(list.Count - falseCount, com);
        }
        else
        {
            //incorrect
            com.panel.gameObject.transform.SetAsFirstSibling();
            list.Add(com);
        }
    }

    public void AddHistory(BallInfo info, bool isCorrect, Mode mode)
    {
        GameObject panelObj = Instantiate(panelPrefab);
        panelObj.transform.SetParent(GetParent(mode));
        panelObj.transform.localPosition = Vector3.zero;
        panelObj.transform.localRotation = Quaternion.identity;
        panelObj.transform.localScale = Vector3.one;
        // panelObj.transform.SetAsFirstSibling();
        Panel panel = panelObj.GetComponent<Panel>();
        panel.SetPanel(info.type.ToString(), info.velocity.ToString("00.0") + "km/s", isCorrect, info.isStrike);
        switch(mode)
        {
            case Mode.Practice:
                SetComponent(new HistoryComponent(info, isCorrect, panel), ref m_practiceHistory);
                break;
            case Mode.Test:
                SetComponent(new HistoryComponent(info, isCorrect, panel), ref m_testHistory);
                break;
        }
        Reflesh();
    }

    private void Reflesh()
    {
        for(int i = 0; i < m_nowHistory.Count; i++)
        {
            m_nowHistory[i].panel.Selected(i == m_selected);
        }

    }

    private void AddTestData()
    {
        for(int i = 0; i < 13; ++i)
            AddHistory(new BallInfo(BallType.Fast, 100f + 10f * i, 3, 3, i % 3 == 0), i % 3 == 1, Mode.Practice);
    }

    void Start()
    {
        m_practiceHistory = new List<HistoryComponent>();
        m_testHistory = new List<HistoryComponent>();
        m_nowHistory = new List<HistoryComponent>();
        m_panelHeight = panelPrefab.GetComponent<RectTransform>().sizeDelta.y;
        m_maxCapacity = (int)(parentHeight / m_panelHeight);
        if(isDebug) AddTestData();

        ChangeHistory(Mode.Practice);
    }

    void Update()
    {
        if(m_scrollUp)
        {
            m_nowScrollbar.value = Mathf.Lerp(m_minHeight, m_maxHeight, m_t);
            m_t += scrollSpeed * Time.deltaTime;

            if (m_t > 1)
            {
                m_scrollUp = false;
            }
        }

        if(m_scrollDown)
        {
            m_nowScrollbar.value = Mathf.Lerp(m_minHeight, m_maxHeight, m_t);
            m_t -= scrollSpeed * Time.deltaTime;

            if(m_t < 0)
            {
                m_scrollDown = false;
            }
        }

        if(isDebug)
        {
            if(Input.GetKeyDown(KeyCode.W))
            {
                Back();
            }
            else if(Input.GetKeyDown(KeyCode.S))
            {
                Next();
            }
        }
    }
}