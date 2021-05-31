using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] Transform parent;
    [SerializeField] Scrollbar scrollbar;
    [SerializeField] BallManager ballManager;
    [SerializeField] GameObject panelPrefab;
    [SerializeField] float parentHeight;

    private List<HistoryComponent> m_history;
    private int m_selected = 0;
    private int m_topIndex = 0;
    private int m_maxCapacity = 0;
    private float m_panelHeight;
    private bool m_scrollUp;
    private bool m_scrollDown;
    private float m_minHeight;
    private float m_maxHeight;
    private float m_perHieght;
    private float m_t;

    public void Next()
    {
        if(m_history.Count == 0) return;

        m_selected++;
        if(m_history.Count - 1 < m_selected)
        {
            m_selected = 0;
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
        if(m_history.Count == 0) return;

        m_selected--;
        if(0 > m_selected)
        {
            m_selected = m_history.Count - 1;
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
        if(m_history.Count * m_panelHeight > parentHeight && m_topIndex == m_selected && m_selected != 0)
        {
            float deltaHeight = (m_history.Count * m_panelHeight) - parentHeight;
            m_perHieght = m_panelHeight / deltaHeight;

            m_t = 0;
            m_topIndex--;
            m_minHeight = scrollbar.value;
            m_maxHeight = m_minHeight + m_perHieght;
            m_scrollUp = true;
        }
    }

    private void ScrollDown()
    {
        if(m_history.Count * m_panelHeight > parentHeight && m_topIndex + m_maxCapacity == m_selected)
        {
            float deltaHeight = (m_history.Count * m_panelHeight) - parentHeight;
            m_perHieght = m_panelHeight / deltaHeight;

            m_t = 1f;
            m_topIndex++;
            m_maxHeight = scrollbar.value;
            m_minHeight = m_maxHeight - m_perHieght;
            m_scrollDown = true;
        }
    }

    private void ScrollTop()
    {
        if(m_history.Count * m_panelHeight > parentHeight)
        {
            float deltaHeight = (m_history.Count * m_panelHeight) - parentHeight;
            m_perHieght = m_panelHeight / deltaHeight;

            m_t = 0;
            m_topIndex = 0;
            m_maxHeight = 1f;
            m_minHeight = scrollbar.value;
            m_scrollUp = true;
        }
    }

    private void ScrollBottom()
    {
        if(m_history.Count * m_panelHeight > parentHeight)
        {
            float deltaHeight = (m_history.Count * m_panelHeight) - parentHeight;
            m_perHieght = m_panelHeight / deltaHeight;

            m_t = 1f;
            m_topIndex = m_history.Count - m_maxCapacity;
            m_maxHeight = scrollbar.value;
            m_minHeight = 0;
            m_scrollDown = true;
        }
    }

    public BallInfo GetSelectedBallInfo()
    {
        if(m_history.Count == 0) return new BallInfo();
        else return m_history[m_selected].ballInfo;
    }

    public void AddHistory(BallInfo info, bool isCorrect)
    {
        GameObject panelObj = Instantiate(panelPrefab);
        panelObj.transform.SetParent(parent);
        panelObj.transform.localPosition = Vector3.zero;
        panelObj.transform.localRotation = Quaternion.identity;
        panelObj.transform.localScale = Vector3.one;
        Panel panel = panelObj.GetComponent<Panel>();
        panel.SetPanel(info.type.ToString(), info.velocity.ToString("00.0") + "km/s", isCorrect, info.isStrike);
        m_history.Add(new HistoryComponent(info, isCorrect, panel));
        Reflesh();
    }

    private void Reflesh()
    {
        for(int i = 0; i < m_history.Count; i++)
        {
            m_history[i].panel.Selected(i == m_selected);
        }
    }

    void Start()
    {
        m_history = new List<HistoryComponent>();
        m_panelHeight = panelPrefab.GetComponent<RectTransform>().sizeDelta.y;
        m_maxCapacity = (int)(parentHeight / m_panelHeight);
    }

    void Update()
    {
        if(m_scrollDown)
        {
            scrollbar.value = Mathf.Lerp(m_minHeight, m_maxHeight, m_t);
            m_t -= 2.5f * Time.deltaTime;

            if(m_t < 0)
            {
                m_scrollDown = false;
            }
        }

        if(m_scrollUp)
        {
            scrollbar.value = Mathf.Lerp(m_minHeight, m_maxHeight, m_t);
            m_t += 2.5f * Time.deltaTime;

            if(m_t > 1f)
            {
                m_scrollUp = false;
            }
        }
    }
}
