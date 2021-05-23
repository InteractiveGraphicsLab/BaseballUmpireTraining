using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public GameObject m_teleport;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

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

    public void ActiveTeleport()
    {
        m_teleport.SetActive(true);
    }

    public void DisableTeleport()
    {
        m_teleport.SetActive(false);
    }
}
