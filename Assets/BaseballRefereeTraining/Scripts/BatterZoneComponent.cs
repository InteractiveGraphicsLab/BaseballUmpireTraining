using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatterZoneComponent : MonoBehaviour
{
    [SerializeField] bool m_isStrikeZone;
    // private boll m_isStrikeZone;
    private BatterZoneManager m_manager;
    private int m_index;

    public void Init(BatterZoneManager manager, int index, bool isStrikeZone)
    {
        m_manager = manager;
        m_index = index;
        m_isStrikeZone = isStrikeZone;

        this.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
        if(isStrikeZone)
        {
            this.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.red);
        }
        else
        {
            this.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.blue);
        }
    }

    private void OnTriggerEnter(Collider ball)
    {
        if(ball.CompareTag("Ball"))
        {
            //親に伝達
            m_manager.WasPitched(m_index, m_isStrikeZone);

            //色を変える？
        }
    }
}
