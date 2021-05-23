using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatterZoneManager : MonoBehaviour
{
    [SerializeField] float lineLength = 0.43f / 3f * 7f;
    [SerializeField] float colunmLength = 1.4f;
    [SerializeField] float z = 0.43f;
    [SerializeField] int divNum = 7;

    private Transform m_batterZoneParent;
    private Transform m_batterZoneLineParent;

    private BatterZoneComponent[] m_batterZones;

    public void WasPitched(int componentNum, bool isStrike)
    {
        //save data: component num, isStrike ...
        
    }

    private void CreateBatterZone()
    {
        bool isStrikeZone;
        float lineSize = lineLength / divNum;
        float colunmSize = colunmLength / divNum;
        float startLine = -6f * lineSize / 2f;

        m_batterZoneParent = new GameObject("BatterZoneParent").transform;
        m_batterZoneParent.SetParent(this.transform);
        m_batterZoneParent.localPosition = Vector3.zero;

        for(int i = 0; i < divNum; i++)
        {
            for(int j = 0; j < divNum; j++)
            {
                GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Quad);
                panel.transform.SetParent(m_batterZoneParent);
                panel.transform.localPosition = new Vector3(startLine + j * lineSize, i * colunmSize, z);
                panel.transform.localScale = new Vector3(lineSize, colunmSize, 0);
                isStrikeZone = 2 <= i && i <= 4 && 2 <= j && j <= 4;
                m_batterZones[i * divNum + j] = panel.AddComponent<BatterZoneComponent>();
                m_batterZones[i * divNum + j].Init(this, i * divNum + j, isStrikeZone);
            }
        }
    }

    private void CreateBatterZoneLine()
    {
        m_batterZoneLineParent = new GameObject("BatterZoneLineParent").transform;
        m_batterZoneLineParent.SetParent(this.transform);
        m_batterZoneLineParent.localPosition = Vector3.zero;

        GameObject frame = new GameObject("ZoneFrame");
        frame.transform.SetParent(m_batterZoneLineParent);
        frame.transform.localPosition = Vector3.zero;
        LineRenderer line = frame.AddComponent<LineRenderer>();
        Vector3[] positions = new Vector3[4]{
            new Vector3(lineLength / 2f, 0, z),
            new Vector3(lineLength / -2f, 0, z),
            new Vector3(lineLength / -2f, colunmLength, z),
            new Vector3(lineLength / 2f, colunmLength, z)
        };
        line.positionCount = positions.Length;
        line.SetPositions(positions);
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = Color.black;
        line.endColor = Color.black;
        line.startWidth = 0.01f;
        line.endWidth = 0.01f;
        line.loop = true;

        GameObject strikeFrame = new GameObject("StrikeZoneFrame");
        strikeFrame.transform.SetParent(m_batterZoneLineParent);
        strikeFrame.transform.localPosition = Vector3.zero;
        LineRenderer strikeLine = strikeFrame.AddComponent<LineRenderer>();
        Vector3[] strikePositions = new Vector3[4]{
            new Vector3(lineLength / 2f - lineLength / divNum * 2f, colunmLength / divNum * 2, z),
            new Vector3(lineLength / 2f - lineLength / divNum * 2f, colunmLength - colunmLength / divNum * 2, z),
            new Vector3(lineLength / -2f + lineLength / divNum * 2f, colunmLength - colunmLength / divNum * 2, z),
            new Vector3(lineLength / -2f + lineLength / divNum * 2f, colunmLength / divNum * 2, z)
        };
        strikeLine.material = new Material(Shader.Find("Sprites/Default"));
        strikeLine.startColor = Color.black;
        strikeLine.endColor = Color.black;
        strikeLine.numCapVertices = 1;
        strikeLine.numCornerVertices = 1;
        strikeLine.loop = true;
        strikeLine.positionCount = strikePositions.Length;
        strikeLine.SetPositions(strikePositions);

        strikeLine.startWidth = 0.01f;
        strikeLine.endWidth = 0.01f;

        // for(int i = 0; i < 4; i++)
        // {
        //     GameObject strikeFrame = new GameObject("StrikeZoneFrame" + i);
        //     strikeFrame.transform.SetParent(m_batterZoneLineParent);
        //     strikeFrame.transform.localPosition = Vector3.zero;
        //     LineRenderer strikeLine = strikeFrame.AddComponent<LineRenderer>();
        //     strikeLine.material = new Material(Shader.Find("Sprites/Default"));
        //     strikeLine.startColor = Color.black;
        //     strikeLine.endColor = Color.black;
        //     strikeLine.startWidth = 0.01f;
        //     strikeLine.endWidth = 0.01f;
        //     if(i == 3)
        //     {
        //         strikeLine.SetPosition(0, strikePositions[i]);
        //         strikeLine.SetPosition(1, strikePositions[0]);
        //     }
        //     else
        //     {
        //         strikeLine.SetPosition(0, strikePositions[i]);
        //         strikeLine.SetPosition(1, strikePositions[i + 1]);
        //     }
        // }
    }

    void Start()
    {
        m_batterZones = new BatterZoneComponent[49];
        CreateBatterZone();
        CreateBatterZoneLine();
    }

    void Update()
    {

    }
}
