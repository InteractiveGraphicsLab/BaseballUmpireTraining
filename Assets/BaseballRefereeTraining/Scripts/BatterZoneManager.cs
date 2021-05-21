using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatterZoneManager : MonoBehaviour
{
    [SerializeField] float lineSize = 0.1433333f;
    [SerializeField] float colunmSize = 0.2f;

    private BatterZoneComponent[] m_batterZones;

    private void CreateBatterZone()
    {
        bool isStrikeZone;
        float startLine = -6f * lineSize / 2f;

        for(int i = 0; i < 7; i++)
        {
            for(int j = 0; j < 7; j++)
            {
                GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Quad);
                panel.transform.parent = this.transform;
                panel.transform.localPosition = new Vector3(startLine + j * lineSize, i * colunmSize, 0.43f);
                panel.transform.localScale = new Vector3(lineSize, colunmSize, 0);
                isStrikeZone = 2 <= i && i <= 4 && 2 <= j && j <= 4;
                m_batterZones[i * 7 + j] = panel.AddComponent<BatterZoneComponent>();
                m_batterZones[i * 7 + j].Init(this, isStrikeZone);
            }
        }
    }

    void Start()
    {
        m_batterZones = new BatterZoneComponent[49];
        CreateBatterZone();
    }

    void Update()
    {
        
    }
}
