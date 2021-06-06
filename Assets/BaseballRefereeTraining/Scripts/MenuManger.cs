using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManger : MonoBehaviour
{
    [SerializeField] OVRInput.Button subExecute;
    [SerializeField] OVRInput.Controller controller;
    [SerializeField] MenuController[] m_menuCons;

    private State m_nowState;

    private void InactiveUIs()
    {
        foreach(MenuController mc in m_menuCons)
        {
            mc.SetActive(false);
        }
    }

    public void ActiveUI(State state)
    {
        InactiveUIs();
        switch(state)
        {
            case State.Select:
                m_menuCons[0].SetActive(true);
                break;
            case State.Judge:
                m_menuCons[1].SetActive(true);
                break;
            case State.Replay:
                m_menuCons[2].SetActive(true);
                break;
        }

        m_nowState = state;
    }

    void Start()
    {
        
    }

    void Update()
    {
        //todo
        if(m_nowState == State.Replay)
        {
            if(OVRInput.Get(subExecute, controller))
            {
                m_menuCons[2].SetActive(false);
                m_menuCons[3].SetActive(true);
            }
            else
            {
                m_menuCons[3].SetActive(false);
                m_menuCons[2].SetActive(true);
            }
        }
    }
}
