using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public enum ButtonType
{
    Basic,
    Toggle,
    Checkboxes,
    Radio,
    StepIncrement,
    Slider,
    HoldDown
}

[Serializable]
public class WidgetEvent
{
    public string text;
    public UnityEvent func;

    public WidgetEvent(WidgetEvent w)
    : this(w.text, w.func) {}

    public WidgetEvent(string t, UnityEvent f)
    {
        text = t;
        func = f;
    }
}

public class WidgetMenu
{
    public ButtonType type { get; private set; }
    public int nowEventIndex { get; private set; }
    public Text menuText { get; private set; }
    public WidgetEvent[] events { get; private set; }
    public int eventNum { get; private set; }
    public Transform transform { get; private set; }

    public WidgetMenu(WidgetMenu w)
    : this(w.transform, w.menuText, w.events) {}

    public WidgetMenu(Transform tr, Text te, IReadOnlyList<WidgetEvent> e)
    {
        transform = tr;
        menuText = te;
        events = e.Select(w => new WidgetEvent(w)).ToArray();
        eventNum = e.Count;
        SetMenu(0);
    }

    public void SetMenu(int index)
    {
        nowEventIndex = index;
        menuText.text = events[nowEventIndex].text;
    }

    public void SetMenu(string text)
    {
        for(int i = 0; i < eventNum; i++)
        {
            if(events[i].text == text)
            {
                SetMenu(i);
                break;
            }
        }
    }

    public void SetTextInfo(float z, int fontSize, Color color)
    {
        Vector3 p = transform.localPosition;
        transform.localPosition = new Vector3(p.x, p.y, -z);
        menuText.fontSize = fontSize;
        menuText.color = color;
    }

    public void Invoke()
    {
        events[nowEventIndex++].func.Invoke();
        if(eventNum <= nowEventIndex)
        {
            nowEventIndex = 0;
        }
        menuText.text = events[nowEventIndex].text;
    }
}
