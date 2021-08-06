using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[Serializable]
public class WidgetMenus
{
    public string name;
    public WidgetEvent[] contents;
}

enum ButtonOption
{
    Defualt,
    Nested
}

public class WidgetMenuController : MonoBehaviour
{
    [SerializeField] GameObject parentCanvas;
    [SerializeField] GameObject menuImagePrefab;
    [SerializeField] GameObject subMenuImagePrefab;
    [SerializeField] GameObject buttonPrefab;
    [SerializeField] float agility = 0.05f;
    [SerializeField, Range(0, 360)] float startAxis;
    [SerializeField] int dynamicPixelsPerUnit = 7;
    [SerializeField] Vector2 sizeDelta;
    [SerializeField] Color menuColor;
    [SerializeField] Color selectorColor;
    [SerializeField] Color highlightColor;
    [SerializeField] Color textColor;
    [SerializeField] WidgetMenus[] widgetMenuEvents;
    [SerializeField] float radius = 35f;
    [SerializeField] float EmphasisRatio = 1.5f;
    [SerializeField, Range(0, 1)] float selectionThreshold = 0.25f;
    [SerializeField] int defualtFontSize = 14;

    private List<WidgetMenu> m_widgetMenus;
    private int m_selected
    {
        get { return _selected; }
        set
        {
            if(_selected != value)
            {
                _selected = value;
                ResetAppearance();
            }
        }
    }
    private int _selected = 0;
    private float m_fillAmount
    {
        get { return _fillAmount; }
        set
        {
            if(_fillAmount != value)
            {
                _fillAmount = value;
                m_transitionAxis = _fillAmount == 0 ? -180f / widgetMenuEvents.Length : 0;
            }
        }
    }
    private float _fillAmount;
    private float m_subFillAmount
    {
        get { return _subFillAmount; }
        set
        {
            if(_subFillAmount != value)
            {
                _subFillAmount = value;
                // m_transitionAxis = _subFillAmount == 0 ? -180f / widgetMenuEvents.Length : 0;
            }
        }
    }
    private float _subFillAmount;
    private Quaternion m_from, m_to;
    private Image m_menuImage, m_selectorImage;
    private Image m_subMenuImage, m_subSelectorImage;
    private Transform m_widgetMenusParent;
    private float m_menuDegree;
    private float m_transitionAxis;
    // private bool m_isShown = false;

    private void OnValidate()
    {
        if(m_widgetMenus.Count > 0)
            FixTextRotation();
    }

    private void Emphasis(float magnitude)
    {
        if (selectionThreshold <= magnitude)
            m_widgetMenus[m_selected].SetTextInfo(EmphasisRatio, defualtFontSize, highlightColor);
        else
            m_widgetMenus[m_selected].SetTextInfo(0, defualtFontSize, textColor);
    }

    private void ResetAppearance()
    {
        if(m_widgetMenus.Count < 0) return;

        for (int i = 0; i < m_widgetMenus.Count; i++)
        {
            m_widgetMenus[i].SetTextInfo(0, defualtFontSize, textColor);
        }
    }

    private int CalcNowMode(Vector2 trackPadPos)
    {
        float angle = ((-Mathf.Atan2(trackPadPos.x, trackPadPos.y) * Mathf.Rad2Deg + 360f + 180f - startAxis)) % 360f;
        float partAngle = 360f / m_widgetMenus.Count;
        return Mathf.FloorToInt((angle / partAngle));
    }

    private void FixTextRotation()
    {
        for(int i = 0; i < m_widgetMenus.Count; i++)
        {
            float degree = m_menuDegree * (i + 0.5f) + startAxis;
            m_widgetMenus[i].transform.localPosition = Quaternion.AngleAxis(degree, Vector3.forward) * (radius * Vector3.down);
        }
    }

    private void AddMenu(IReadOnlyList<WidgetEvent> menuEvent)
    {
        GameObject btnObj = Instantiate(buttonPrefab, m_widgetMenusParent);
        btnObj.transform.localScale = 0.1f * Vector3.one;
        btnObj.transform.localRotation = Quaternion.Euler(Vector3.zero);

        Text text = btnObj.GetComponent<Text>();
        text.color = textColor;
        text.fontSize = defualtFontSize;

        m_widgetMenus.Add(new WidgetMenu(btnObj.transform, text, menuEvent));
        FixTextRotation();
    }

    private void SetDefualtProparty(Transform img)
    {
        img.localPosition = Vector3.zero;
        img.localRotation = Quaternion.identity;
        img.localScale = Vector3.one;
    }

    private void CreateMenuUI()
    {
        if(m_menuImage == null)
        {
            GameObject menuImage = Instantiate(menuImagePrefab, parentCanvas.transform);
            menuImage.name = "MenuImage";
            SetDefualtProparty(menuImage.transform);
            m_menuImage = menuImage.GetComponent<Image>();
            m_menuImage.color = menuColor;
        }

        if(m_selectorImage == null)
        {
            GameObject selectorImage = Instantiate(menuImagePrefab, parentCanvas.transform);
            selectorImage.name = "Selector";
            SetDefualtProparty(selectorImage.transform);
            m_selectorImage = selectorImage.GetComponent<Image>();
            m_selectorImage.color = selectorColor;
            m_selectorImage.fillAmount = m_fillAmount = 1f / widgetMenuEvents.Length;
            m_menuDegree = m_selectorImage.fillAmount * 360f;
        }

        if(m_subMenuImage == null)
        {
            GameObject subMenuImage = Instantiate(subMenuImagePrefab, parentCanvas.transform);
            subMenuImage.name = "SubMenuImage";
            SetDefualtProparty(subMenuImage.transform);
            m_subMenuImage = subMenuImage.GetComponent<Image>();
            m_subMenuImage.color = menuColor;
        }

        if(m_subSelectorImage == null)
        {
            GameObject subSelectorImage = Instantiate(subMenuImagePrefab, parentCanvas.transform);
            subSelectorImage.name = "SubSelector";
            SetDefualtProparty(subSelectorImage.transform);
            m_subSelectorImage = subSelectorImage.GetComponent<Image>();
            m_subSelectorImage.color = selectorColor;

            m_subSelectorImage.fillAmount = m_subFillAmount = 1f / widgetMenuEvents.Length;
            // m_menuDegree = m_subSelectorImage.fillAmount * 360f;
        }

        if(m_widgetMenusParent == null)
        {
            m_widgetMenusParent = new GameObject("Buttons").transform;
            m_widgetMenusParent.SetParent(parentCanvas.transform);
            SetDefualtProparty(m_widgetMenusParent.transform);
        }

        foreach(WidgetMenus menu in widgetMenuEvents)
        {
            AddMenu(menu.contents);
        }
    }

    // private IEnumerator Wait(float time)
    // {
    //     // if(!m_isInvoked) yield break;

    //     yield return new WaitForSeconds(time);
    //     m_isInvoked = false;
    // }

    private void Start()
    {
        parentCanvas.GetComponent<CanvasScaler>().dynamicPixelsPerUnit = dynamicPixelsPerUnit;

        m_widgetMenus = new List<WidgetMenu>();
        m_from = Quaternion.identity;
        m_to = Quaternion.identity;

        CreateMenuUI();
        // parentCanvas.SetActive(false);
    }

    private void Update()
    {
        // m_isShown = true;
        Vector2 touchScreenPosition = new Vector2((Input.mousePosition.x - Screen.width / 2f) / (Screen.width / 2f), (Input.mousePosition.y - Screen.height / 2f) / (Screen.height / 2f));

        m_selected = CalcNowMode(touchScreenPosition);
        Emphasis(touchScreenPosition.magnitude);

        if(selectionThreshold <= touchScreenPosition.magnitude)
        {
            //appear
            m_fillAmount = 1f / widgetMenuEvents.Length;
        }
        else
        {
            //disappear
            m_fillAmount = 0;
        }
        // m_fillAmount = selectionThreshold <= touchScreenPosition.magnitude ? 1f / widgetMenuEvents.Length : 0;
        m_selectorImage.fillAmount = Mathf.Lerp(m_selectorImage.fillAmount, m_fillAmount, agility);

        m_to = Quaternion.AngleAxis(((m_selected + 1) * m_menuDegree + startAxis + m_transitionAxis), Vector3.forward);
        m_selectorImage.gameObject.transform.localRotation = Quaternion.Lerp(m_selectorImage.gameObject.transform.localRotation, m_to, agility);
  }
}
