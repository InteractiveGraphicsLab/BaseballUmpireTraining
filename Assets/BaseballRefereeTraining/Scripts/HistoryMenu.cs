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

    private List<HistoryComponent> m_history;
    private int m_selected = 0;

    public void Next()
    {
        if(m_history.Count == 0) return;

        m_selected++;
        if(m_history.Count == m_selected)
            m_selected = 0;

        Reflesh();
    }

    public void Back()
    {
        if(m_history.Count == 0) return;

        if(0 == m_selected)
            m_selected = m_history.Count;
        m_selected--;

        Reflesh();
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
        string text = info.isStrike ? "<color=yellow>●</color> " : "<color=green>●</color> ";
        text += info.type.ToString().PadRight(8) + " " + info.velocity.ToString("00.0") + "km/s ";
        panel.SetPanel(text, isCorrect);
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
    }

    void Update()
    {
        if(m_history.Count == 0) return;
    }
}
