using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [SerializeField, Range(1, 6)] int menuNum = 2;
    // [SerializeField] int[] menu = {0, 1, 2};
    [SerializeField, Range(0, 5)] int selected = 0;
    private int selectedBuf = 0;
    [SerializeField] float radius = 35f;
    [SerializeField] float moveTime = 1f;
    [SerializeField] Color menuColor;
    [SerializeField] Color selectorColor;
    [SerializeField] Color highlightColor;
    [SerializeField] Color textColor;

    [SerializeField] GameObject m_menuImagePrefab;
    [SerializeField] GameObject m_buttonPrefab;
    [SerializeField] int dynamicPixelsPerUnit = 7;
    [SerializeField] float cooperationRatio = 1.5f;
    [SerializeField] int defualtFontSize = 16;

    private Image m_menuImage;
    private Image m_selectorImage;
    private Transform m_buttonsParent;
    class MyButton {
        public GameObject gameObject;
        public Button button;
        public Text text;
    }
    private List<MyButton> m_buttons;

    private Quaternion m_from;
    private Quaternion m_to;
    private float m_startTime;
    private float m_timeStep;

    private void CreateMenuUI()
    {
        if(m_menuImage == null)
        {
            GameObject　menuImage = Instantiate(m_menuImagePrefab);
            menuImage.name = "MenuImage";
            menuImage.transform.parent = this.transform;
            m_menuImage = menuImage.GetComponent<Image>();
            m_menuImage.color = menuColor;
        }

        if(m_selectorImage == null)
        {
            GameObject　selectorImage = Instantiate(m_menuImagePrefab);
            selectorImage.name = "Selector";
            selectorImage.transform.parent = this.transform;
            m_selectorImage = selectorImage.GetComponent<Image>();
            m_selectorImage.color = selectorColor;
            m_selectorImage.fillAmount = 1f / menuNum;
        }

        if(m_buttonsParent == null)
        {
            m_buttonsParent = new GameObject("Buttons").transform;
            m_buttonsParent.parent = this.transform;
        }
    }

    // private void UpdateParamaters()
    // {
    //     m_selectorImage.fillAmount = 1f / menuNum;
    // }

    private void FixedButtonText()
    {
        int i = 0;
        foreach(MyButton mybtn in m_buttons)
        {
            float rot = m_selectorImage.fillAmount * -360f;
            mybtn.gameObject.transform.localPosition = Quaternion.AngleAxis(i++ * rot + rot / 2, transform.forward) * Vector3.down * radius;
        }
    }

    private void AddMenu()
    {
        for(int i = 0; i < menuNum - m_buttons.Count; ++i)
        {
            GameObject btnObj = Instantiate(m_buttonPrefab);
            MyButton btn = new MyButton();

            FixedButtonText();
            float rot = m_selectorImage.fillAmount * -360f;
            btnObj.transform.localPosition = Quaternion.AngleAxis(m_buttons.Count * rot + rot / 2, transform.forward) * Vector3.down * radius;
            btnObj.transform.parent = m_buttonsParent;
            btn.gameObject = btnObj;
            btn.button = btnObj.GetComponent<Button>();
            btn.text = btnObj.GetComponent<Text>();
            btn.text.color = textColor;
            btn.text.fontSize = defualtFontSize;
            m_buttons.Add(btn);
        }
    }

    private void DeleteMenu()
    {
        for(int i = 0; i < m_buttons.Count - menuNum; ++i)
        {
            Destroy(m_buttons[m_buttons.Count - 1].gameObject);
            m_buttons.RemoveAt(m_buttons.Count - 1);
            FixedButtonText();
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

        this.GetComponent<CanvasScaler>().dynamicPixelsPerUnit = dynamicPixelsPerUnit;

        m_buttons = new List<MyButton>();
        m_from = Quaternion.identity;
        m_to = Quaternion.identity;

        CreateMenuUI();
    }

    void Update()
    {
        // ----- Debug -----
        if(m_buttons.Count < menuNum)
        {
            //UpdateParamaters();
            m_selectorImage.fillAmount = 1f / menuNum;
            AddMenu();
        }
        else if(m_buttons.Count > menuNum)
        {
            // UpdateParamaters();
            m_selectorImage.fillAmount = 1f / menuNum;
            DeleteMenu();
        }
        // -----

        if(selectedBuf != selected)
        {
            m_startTime = Time.time;
            m_from = m_selectorImage.gameObject.transform.localRotation;
            m_to = Quaternion.AngleAxis(selected * m_selectorImage.fillAmount * -360f, transform.forward);

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
