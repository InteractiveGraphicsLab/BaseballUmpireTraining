using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Panel : MonoBehaviour
{
    [SerializeField] Text balltypeText;
    [SerializeField] Text velocityText;
    [SerializeField] GameObject correct;
    [SerializeField] GameObject incorrect;
    [SerializeField] Image panelImage;
    [SerializeField] Image panelHeadImage;
    [SerializeField] Image judgeImage;
    [SerializeField] Color correctColor;
    [SerializeField] Color incorrectColor;
    [SerializeField] Color backgroundColor;
    [SerializeField] Color selectedColor;
    [SerializeField] Color strikeColor;
    [SerializeField] Color ballColor;

    private Color defualtColor;

    public void SetPanel(string typeText, string veloText, bool isCorrect, bool isStrike)
    {
        balltypeText.text = typeText;
        velocityText.text = veloText;
        defualtColor = isCorrect ? correctColor : incorrectColor;
        panelHeadImage.color = defualtColor;
        correct.SetActive(isCorrect);
        incorrect.SetActive(!isCorrect);
        panelImage.color = backgroundColor;
        judgeImage.color = isStrike ? strikeColor : ballColor;
    }

    public void Selected(bool isSelected)
    {
        if(isSelected)
        {
            // this.transform.localPosition += new Vector3(0, 0, selectedDelta);
            panelImage.color = selectedColor;
        }
        else
        {
            // this.transform.localPosition = pos;
            panelImage.color = backgroundColor;
        }
    }
}
