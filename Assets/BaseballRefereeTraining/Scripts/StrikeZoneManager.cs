using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrikeZoneManager : MonoBehaviour
{
    [SerializeField] Vector3 centerPosition = new Vector3(0, 0.7f, 0);
    [SerializeField] float strikeZoneWidth = 0.432f;
    [SerializeField] float strikeZoneHeight = 0.6f;
    [SerializeField] int verticalDivisionNumber = 4;
    [SerializeField] int horizontalDivisionNumber = 4;
    [SerializeField] int verticalBallZoneOffset = 1;
    [SerializeField] int horizontalBallZoneOffset = 1;
    [SerializeField] float mainLineWidth = 0.01f;
    [SerializeField] float subLineWidth = 0.001f;
    [SerializeField] float zoneZScale = 0.001f;
    [SerializeField] float ballTracerScale = 0.073f;

    private StrikeZoneProperty m_property;
    private float m_posZ;
    private Vector2 m_pos;
    private int m_widthNum;
    private int m_heightNum;
    private Material m_tracerMat;
    private Material m_strikeMat;
    private Material m_ballMat;
    private Renderer m_ballTracer;
    private Transform m_strikeZoneParent;
    private Transform m_StrikeZoneLineParent;

    private StrikeZoneComponent[] m_batterZones;

    public StrikeZoneProperty GetProperty()
    {
        return m_property;
    }

    public void ShowBallTrace(Vector3 pos)
    {
        m_ballTracer.gameObject.transform.localPosition = new Vector3(-pos.x, pos.y, m_posZ);

        m_ballTracer.gameObject.SetActive(true);
    }

    public void ShowStrikeZone()
    {
        m_strikeZoneParent.gameObject.SetActive(true);
        m_StrikeZoneLineParent.gameObject.SetActive(true);
    }

    public void Show(Vector3 pos)
    {
        ShowBallTrace(pos);
        ShowStrikeZone();
    }

    public void HideBallTrace()
    {
        m_ballTracer.gameObject.SetActive(false);
    }

    public void HideStrikeZone()
    {
        m_strikeZoneParent.gameObject.SetActive(false);
        m_StrikeZoneLineParent.gameObject.SetActive(false);
    }

    public void Hide()
    {
        HideBallTrace();
        HideStrikeZone();
    }

    private void CreateBatterZone()
    {
        bool isStrikeZone;
        Vector2 startPos = m_pos + new Vector2( -(m_property.zoneSize.x - m_property.panelSize.x) / 2f, -(m_property.zoneSize.y - m_property.panelSize.y) / 2f);

        m_strikeZoneParent = new GameObject("BatterZoneParent").transform;
        m_strikeZoneParent.SetParent(this.transform);
        m_strikeZoneParent.localPosition = Vector3.zero;

        for(int i = 0; i < m_widthNum; i++)
        {
            for(int j = 0; j < m_heightNum; j++)
            {
                // GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Quad);
                GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                panel.transform.SetParent(m_strikeZoneParent);
                panel.transform.localPosition = new Vector3(startPos.x + i * m_property.panelSize.x, startPos.y + j * m_property.panelSize.y, m_posZ);
                panel.transform.localScale = new Vector3(m_property.panelSize.x, m_property.panelSize.y, zoneZScale);
                isStrikeZone = m_property.IsStrike(i, j);
                m_batterZones[i * m_widthNum + j] = panel.AddComponent<StrikeZoneComponent>();
                m_batterZones[i * m_widthNum + j].Init(this, i * m_widthNum + j, isStrikeZone, isStrikeZone ? m_strikeMat : m_ballMat);
            }
        }
    }

    private void SetUpLineRenderer(ref LineRenderer line)
    {
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = Color.black;
        line.endColor = Color.black;
        line.numCapVertices = 10;
        line.numCornerVertices = 10;
        line.loop = true;
        line.widthMultiplier = 0.2f;
        // line.sortingLayerName = "StrikeZone";
    }

    private void CreateLine(Vector3 start, Vector3 end)
    {
        GameObject strikeInnerFrame = new GameObject("StrikeInnerFrame");
        strikeInnerFrame.transform.SetParent(m_StrikeZoneLineParent);
        strikeInnerFrame.transform.localPosition = Vector3.zero;
        LineRenderer innerLine = strikeInnerFrame.AddComponent<LineRenderer>();
        innerLine.SetPosition(0, start);
        innerLine.SetPosition(1, end);
        SetUpLineRenderer(ref innerLine);
        innerLine.startWidth = subLineWidth;
        innerLine.endWidth = subLineWidth;
    }

    private void CreateBatterZoneLine()
    {
        m_StrikeZoneLineParent = new GameObject("BatterZoneLineParent").transform;
        m_StrikeZoneLineParent.SetParent(this.transform);
        m_StrikeZoneLineParent.localPosition = Vector3.zero;

        GameObject strikeFrame = new GameObject("StrikeZoneFrame");
        strikeFrame.transform.SetParent(m_StrikeZoneLineParent);
        strikeFrame.transform.localPosition = Vector3.zero;
        LineRenderer strikeLine = strikeFrame.AddComponent<LineRenderer>();
        Vector3 startPos = centerPosition + new Vector3(-m_property.size.x / 2f, -m_property.size.y / 2f, 0);

        Vector3[] strikePositions = new Vector3[4]{
            startPos + new Vector3(0, 0, 0),
            startPos + new Vector3(0, m_property.size.y, 0),
            startPos + new Vector3(m_property.size.x, m_property.size.y, 0),
            startPos + new Vector3(m_property.size.x, 0, 0)
        };
        strikeLine.positionCount = strikePositions.Length;
        strikeLine.SetPositions(strikePositions);
        SetUpLineRenderer(ref strikeLine);
        strikeLine.startWidth = mainLineWidth;
        strikeLine.endWidth = mainLineWidth;

        for(int i = 0; i <= m_property.horiDivNum; ++i)
        {
            if(i == 0 || i == m_property.horiDivNum) continue;
            CreateLine(startPos + new Vector3(m_property.panelSize.x * i, 0, 0), startPos + new Vector3(m_property.panelSize.x * i, m_property.size.y, 0));
        }

        for(int i = 0; i <= m_property.vertDivNum; ++i)
        {
            if(i == 0 || i == m_property.vertDivNum) continue;
            CreateLine(startPos + new Vector3(0, m_property.panelSize.y * i, 0), startPos + new Vector3(m_property.size.x, m_property.panelSize.y * i, 0));
        }
    }

    private void CreateBallTracer()
    {
        GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball.name = "BallTracer";
        ball.transform.localScale = ballTracerScale * Vector3.one;
        m_ballTracer = ball.GetComponent<Renderer>();
        m_ballTracer.material = m_tracerMat;
    }

    private void Start()
    {
        m_property = new StrikeZoneProperty(centerPosition, strikeZoneWidth, strikeZoneHeight, verticalDivisionNumber, horizontalDivisionNumber, verticalBallZoneOffset, horizontalBallZoneOffset);
        m_widthNum = m_property.horiDivNum + m_property.horiOffset * 2;
        m_heightNum = m_property.vertDivNum + m_property.vertOffset * 2;
        m_pos = new Vector2(centerPosition.x, centerPosition.y);
        m_posZ = centerPosition.z;
        m_batterZones = new StrikeZoneComponent[m_widthNum * m_heightNum];
        m_tracerMat = Resources.Load<Material>("BallTracer");
        m_strikeMat = Resources.Load<Material>("StrikeZone");
        m_ballMat = Resources.Load<Material>("Transparent");
        CreateBatterZone();
        CreateBatterZoneLine();
        CreateBallTracer();
        Hide();
    }
}
