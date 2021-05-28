using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[Serializable]
class MenuEvent
{
    public string[] text;
    public UnityEvent[] func;
}

class MyButton
{
    public GameObject gameObject;
    public Text text;
    public UnityEvent func;
    public int index;
}

public class MenuController : MonoBehaviour
{
    [SerializeField] State state;
    [SerializeField] OVRInput.Controller controller;
    [SerializeField] GameObject parentCanvas;
    [SerializeField] MenuEvent[] menuData;
    private int selected = 0;
    private int selectedBuf = 0;
    [SerializeField] float radius = 35f;
    [SerializeField] float moveTime = 0.2f;
    [SerializeField] Color menuColor;
    [SerializeField] Color selectorColor;
    [SerializeField] Color highlightColor;
    [SerializeField] Color textColor;

    [SerializeField] GameObject m_menuImagePrefab;
    [SerializeField] GameObject m_buttonPrefab;
    [SerializeField] Vector2 sizeDelta;
    [SerializeField] int dynamicPixelsPerUnit = 7;
    [SerializeField] float cooperationRatio = 1.5f;
    [SerializeField] int defualtFontSize = 16;
    [SerializeField] List<MyButton> m_buttons;

    private Image m_menuImage;
    private Image m_selectorImage;
    private Transform m_buttonsParent;

    private Quaternion m_from;
    private Quaternion m_to;
    private float m_startTime;
    private float m_timeStep;

    private void CalcNowMode(Vector2 trackPadPos)
    {
        if(trackPadPos.x == 0 && trackPadPos.y == 0)
        {
            return;
        }

        float angle = (((-1f * Mathf.Atan2(trackPadPos.y, trackPadPos.x) * Mathf.Rad2Deg + 360f - 90f))) % 360f;
        float partAngle = 360f / menuData.Length;
        selected = Mathf.FloorToInt((angle/ partAngle));
    }

    // private void UpdateParamaters()
    // {
    //     m_selectorImage.fillAmount = 1f / menuData.Length;
    // }

    private void FixedButtonText()
    {
        int i = 0;
        foreach(MyButton mybtn in m_buttons)
        {
            float rot = m_selectorImage.fillAmount * -360f;
            mybtn.gameObject.transform.localPosition = Quaternion.AngleAxis(i++ * rot + rot / 2, Vector3.forward) * Vector3.down * radius;
        }
    }

    private void AddMenu(MenuEvent menuEvent)
    {
        GameObject btnObj = Instantiate(m_buttonPrefab);
        MyButton btn = new MyButton();

        FixedButtonText();
        float rot = m_selectorImage.fillAmount * -360f;
        btnObj.transform.SetParent(m_buttonsParent);
        btnObj.transform.localScale = Vector3.one;
        btnObj.transform.localRotation = Quaternion.Euler(Vector3.zero);
        btnObj.transform.localPosition = Quaternion.AngleAxis(m_buttons.Count * rot + rot / 2, Vector3.forward) * Vector3.down * radius;
        btn.gameObject = btnObj;
        btn.index = 0;
        btn.func = menuEvent.func[btn.index];
        btn.text = btnObj.GetComponent<Text>();
        btn.text.text = menuEvent.text[btn.index];
        btn.text.color = textColor;
        btn.text.fontSize = defualtFontSize;
        m_buttons.Add(btn);
    }

    private void DeleteMenu()
    {
        for(int i = 0; i < m_buttons.Count - menuData.Length; ++i)
        {
            Destroy(m_buttons[m_buttons.Count - 1].gameObject);
            m_buttons.RemoveAt(m_buttons.Count - 1);
            FixedButtonText();
        }
    }

    private void CreateMenuUI()
    {
        if(m_menuImage == null)
        {
            GameObject menuImage = Instantiate(m_menuImagePrefab);
            menuImage.name = "MenuImage";
            menuImage.transform.SetParent(parentCanvas.transform);
            menuImage.transform.localPosition = new Vector3(-sizeDelta.x / 2f, -sizeDelta.y / 2f, 0);
            menuImage.transform.localRotation = Quaternion.Euler(Vector3.zero);
            menuImage.transform.localScale = Vector3.one;
            m_menuImage = menuImage.GetComponent<Image>();
            m_menuImage.color = menuColor;
        }

        if(m_selectorImage == null)
        {
            GameObject selectorImage = Instantiate(m_menuImagePrefab);
            selectorImage.name = "Selector";
            selectorImage.transform.SetParent(parentCanvas.transform);
            selectorImage.transform.localPosition = new Vector3(-sizeDelta.x / 2f, -sizeDelta.y / 2f, 0);
            selectorImage.transform.localRotation = Quaternion.Euler(Vector3.zero);
            selectorImage.transform.localScale = Vector3.one;
            m_selectorImage = selectorImage.GetComponent<Image>();
            m_selectorImage.color = selectorColor;
            m_selectorImage.fillAmount = 1f / menuData.Length;
        }

        if(m_buttonsParent == null)
        {
            m_buttonsParent = new GameObject("Buttons").transform;
            m_buttonsParent.SetParent(parentCanvas.transform);
            m_buttonsParent.transform.localPosition = new Vector3(-sizeDelta.x / 2f, -sizeDelta.y / 2f, 0);
            m_buttonsParent.transform.localRotation = Quaternion.Euler(Vector3.zero);
            m_buttonsParent.transform.localScale = Vector3.one;
        }

        foreach(MenuEvent menuEvent in menuData)
        {
            AddMenu(menuEvent);
        }
    }

    void Start()
    {
        if(m_menuImagePrefab == null)
        {
            m_menuImagePrefab = GameObject.Find("DynamicRadialMenu/Resources/MenuImage");
        }

        if(m_buttonPrefab == null)
        {
            m_buttonPrefab = GameObject.Find("DynamicRadialMenu/Resources/Button");
        }

        parentCanvas.GetComponent<CanvasScaler>().dynamicPixelsPerUnit = dynamicPixelsPerUnit;
        parentCanvas.GetComponent<RectTransform>().sizeDelta = sizeDelta;

        m_buttons = new List<MyButton>();
        m_from = Quaternion.identity;
        m_to = Quaternion.identity;

        CreateMenuUI();
        parentCanvas.SetActive(false);
    }

    void Update()
    {
        // ----- Debug -----
        // if(m_buttons.Count < menuData.Length)
        // {
        //     //UpdateParamaters();
        //     m_selectorImage.fillAmount = 1f / menuData.Length;
        //     AddMenu();
        // }
        // else if(m_buttons.Count > menuData.Length)
        // {
        //     // UpdateParamaters();
        //     m_selectorImage.fillAmount = 1f / menuData.Length;
        //     DeleteMenu();
        // }
        // -----

        if(GameManager.instance.GetNowState() != state)
        {
            parentCanvas.SetActive(false);
            return;
        }

        if (OVRInput.GetDown(OVRInput.Touch.PrimaryThumbstick, controller))
        {
            //show UI
            parentCanvas.SetActive(true);
        }
        else if (OVRInput.Get(OVRInput.Touch.PrimaryThumbstick, controller))
        {
            CalcNowMode(OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, controller));
        }
        else if(OVRInput.GetUp(OVRInput.Touch.PrimaryThumbstick, controller))
        {
            //hide UI
            parentCanvas.SetActive(false);
        }

        if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick, controller))
        {
            //execute
            m_buttons[selected].func.Invoke();

            int i = ++m_buttons[selected].index;
            if(m_buttons[selected].index == menuData[selected].func.Length)
            {
                i = m_buttons[selected].index = 0;
            }
            m_buttons[selected].func = menuData[selected].func[i];
            m_buttons[selected].text.text = menuData[selected].text[i];
        }

        if(selectedBuf != selected)
        {
            m_startTime = Time.time;
            m_from = m_selectorImage.gameObject.transform.localRotation;
            m_to = Quaternion.AngleAxis(selected * m_selectorImage.fillAmount * -360f, Vector3.forward);

            for(int i = 0; i < m_buttons.Count; i++)
            {
                if(i == selected)
                {
                    m_buttons[i].gameObject.transform.localPosition += cooperationRatio * 5f * Vector3.back;
                    m_buttons[i].text.fontSize = (int)(defualtFontSize * cooperationRatio);
                    m_buttons[i].text.color = highlightColor;
                }
                else
                {
                    Vector3 pos = m_buttons[i].gameObject.transform.localPosition;
                    pos.z = 0;
                    m_buttons[i].gameObject.transform.localPosition = pos;
                    m_buttons[i].text.fontSize = defualtFontSize;
                    m_buttons[i].text.color = textColor;
                }
            }
            selectedBuf = selected;
        }

        m_timeStep = Mathf.Clamp01((Time.time - m_startTime) / moveTime);
        m_selectorImage.gameObject.transform.localRotation = Quaternion.Lerp(m_from, m_to, m_timeStep);
    }
}
