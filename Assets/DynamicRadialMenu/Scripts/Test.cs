using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public void Callback_One(int i = -1)
    {
        Debug.Log("Callback_One(): " + i);
    }

    public void Callback_Two()
    {
        Debug.Log("Callback_Two()");
    }

    public void Callback_Three()
    {
        Debug.Log("Callback_Three()");
    }
}
