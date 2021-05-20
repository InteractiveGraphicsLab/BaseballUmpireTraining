using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FixedMenuText : MonoBehaviour
{
    [SerializeField, Range(0, 360)] float rotation = 0;
    [SerializeField] float radius = 35f;

    void OnValidate()
    {
        this.transform.localRotation = Quaternion.Euler(0, 0, rotation);

        Image image = this.GetComponent<Image>();
        foreach(Transform childTrans in this.transform)
        {
            childTrans.rotation = Quaternion.identity;
            childTrans.localPosition = Quaternion.AngleAxis(image.fillAmount * -180f, transform.forward) * Vector3.down * radius;
        }
    }
}
