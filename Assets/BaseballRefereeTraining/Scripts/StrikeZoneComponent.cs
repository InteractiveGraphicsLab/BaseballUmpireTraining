using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrikeZoneComponent : MonoBehaviour
{
    private bool m_isStrikeZone;
    private StrikeZoneManager m_manager;
    private int m_index;
    private Renderer m_renderer;

    public void Init(StrikeZoneManager manager, int index, bool isStrikeZone, Material mat)
    {
        m_manager = manager;
        m_index = index;
        m_isStrikeZone = isStrikeZone;
        m_renderer = this.GetComponent<Renderer>();
        m_renderer.material = mat;
    }

    public bool isStrike()
    {
        return m_isStrikeZone;
    }
}
