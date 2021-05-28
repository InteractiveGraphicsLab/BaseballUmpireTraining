using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Panel : MonoBehaviour
{
    [SerializeField] Text panelText;
    [SerializeField] Image image;
    [SerializeField] Color selectedColor;
    // [SerializeField] Color correctColor;
    // [SerializeField] Color incorrectColor;
    [SerializeField] Color defualtColor;
    // [SerializeField] float selectedDelta;

    // private float z;

    public void SetPanel(string text, bool isCorrect)
    {
        panelText.text = text;
        // defualtColor = isCorrect ? correctColor : incorrectColor;
        image.color = defualtColor;
    }

    public void Selected(bool isSelected)
    {
        image.color = isSelected ? selectedColor : defualtColor;
        // if(isSelected)
        // {
        //     this.transform.localPositon.z = z + selectedDelta;
        // }
        // else
        // {
        //     this.transform.localPositon.z = z;
        // }
    }

    private void Start()
    {
        // z = this.tranform.localPositon.z;
    }
}
