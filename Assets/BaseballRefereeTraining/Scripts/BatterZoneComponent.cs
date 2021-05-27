using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatterZoneComponent : MonoBehaviour
{
    private bool m_isStrikeZone;
    private BatterZoneManager m_manager;
    private int m_index;
    private Material m_mat;
    private Renderer m_renderer;

    public void Init(BatterZoneManager manager, int index, bool isStrikeZone, Material mat)
    {
        m_manager = manager;
        m_index = index;
        m_isStrikeZone = isStrikeZone;
        m_mat = mat;
        m_renderer = this.GetComponent<Renderer>();
        m_renderer.material = m_mat;
    }
}
