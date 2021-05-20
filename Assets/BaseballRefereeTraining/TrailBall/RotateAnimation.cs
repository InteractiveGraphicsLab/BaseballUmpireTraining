using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAnimation : MonoBehaviour
{
    [SerializeField] float rot = 360f;

    void Start()
    {

    }

    void Update()
    {
        this.transform.position = Quaternion.AngleAxis(rot * Time.deltaTime, transform.forward) * this.transform.position;
    }
}
