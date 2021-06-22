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

    private int m_vertDiv, m_horiDiv, m_vertOffset, m_horiOffset;
    private float m_posZ;
    private Vector2 m_pos;
    private int m_widthNum;
    private int m_heightNum;
    private Material m_tracerMat;
    private Material m_strikeMat;
    private Material m_ballMat;
    private Renderer m_ballTracer;
    private Transform m_batterZoneParent;
    private Transform m_batterZoneLineParent;

    private StrikeZoneComponent[] m_batterZones;

    public void Show(Vector3 pos)
    {
        ShowBallTrace(pos);
        ShowBatterZone();
    }

    public void ShowBallTrace(Vector3 pos)
    {
        m_ballTracer.gameObject.transform.localPosition = new Vector3(-pos.x, pos.y, m_posZ);

        ShowBatterZone();
        m_ballTracer.gameObject.SetActive(true);
    }

    public void ShowBatterZone()
    {
        m_batterZoneParent.gameObject.SetActive(true);
        m_batterZoneLineParent.gameObject.SetActive(true);
    }

    public void Hide()
    {
        HideBatterZone();
        HideBatterZone();
    }

    public void HideBallTrace()
    {
        m_ballTracer.gameObject.SetActive(false);
    }

    public void HideBatterZone()
    {
        m_batterZoneParent.gameObject.SetActive(false);
        m_batterZoneLineParent.gameObject.SetActive(false);
    }

    private void CreateBatterZone()
    {
        bool isStrikeZone;
        float widthSize = strikeZoneWidth / m_horiDiv;
        float heightSize = strikeZoneHeight / m_vertDiv;
        Vector2 startPos = m_pos + new Vector2(-1f * (strikeZoneWidth / 2f + (m_horiOffset - 0.5f) * widthSize), -1f * (strikeZoneHeight / 2f + (m_vertOffset - 0.5f) * heightSize));

        m_batterZoneParent = new GameObject("BatterZoneParent").transform;
        m_batterZoneParent.SetParent(this.transform);
        m_batterZoneParent.localPosition = Vector3.zero;

        for(int i = 0; i < m_widthNum; i++)
        {
            for(int j = 0; j < m_heightNum; j++)
            {
                // GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Quad);
                GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                panel.transform.SetParent(m_batterZoneParent);
                panel.transform.localPosition = new Vector3(startPos.x + i * widthSize, startPos.y + j * heightSize, m_posZ);
                panel.transform.localScale = new Vector3(widthSize, heightSize, zoneZScale);
                isStrikeZone = m_horiOffset <= i && i <= m_horiOffset + m_horiDiv - 1 && m_vertOffset <= j && j <= m_vertOffset + m_vertDiv - 1;
                m_batterZones[i * m_widthNum + j] = panel.AddComponent<StrikeZoneComponent>();
                m_batterZones[i * m_widthNum + j].Init(this, i * m_widthNum + j, isStrikeZone, isStrikeZone ? m_strikeMat : m_ballMat);
            }
        }
    }

    private void SetUpLineRenderer(LineRenderer line)
    {
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = Color.black;
        line.endColor = Color.black;
        line.numCapVertices = 10;
        line.numCornerVertices = 10;
        line.loop = true;
        line.widthMultiplier = 0.2f;
    }

    private void CreateLine(Vector3 start, Vector3 end)
    {
        GameObject strikeInnerFrame = new GameObject("StrikeInnerFrame");
        strikeInnerFrame.transform.SetParent(m_batterZoneLineParent);
        strikeInnerFrame.transform.localPosition = Vector3.zero;
        LineRenderer innerLine = strikeInnerFrame.AddComponent<LineRenderer>();
        innerLine.SetPosition(0, start);
        innerLine.SetPosition(1, end);
        SetUpLineRenderer(innerLine);
        innerLine.startWidth = subLineWidth;
        innerLine.endWidth = subLineWidth;
    }

    private void CreateBatterZoneLine()
    {
        float widthSize = strikeZoneWidth / m_horiDiv;
        float heightSize = strikeZoneHeight / m_vertDiv;

        m_batterZoneLineParent = new GameObject("BatterZoneLineParent").transform;
        m_batterZoneLineParent.SetParent(this.transform);
        m_batterZoneLineParent.localPosition = Vector3.zero;

        GameObject strikeFrame = new GameObject("StrikeZoneFrame");
        strikeFrame.transform.SetParent(m_batterZoneLineParent);
        strikeFrame.transform.localPosition = Vector3.zero;
        LineRenderer strikeLine = strikeFrame.AddComponent<LineRenderer>();
        Vector3 startPos = centerPosition + new Vector3(-strikeZoneWidth / 2f, -strikeZoneHeight / 2f, 0);

        Vector3[] strikePositions = new Vector3[4]{
            startPos + new Vector3(0, 0, 0),
            startPos + new Vector3(0, strikeZoneHeight, 0),
            startPos + new Vector3(strikeZoneWidth, strikeZoneHeight, 0),
            startPos + new Vector3(strikeZoneWidth, 0, 0)
        };
        strikeLine.positionCount = strikePositions.Length;
        strikeLine.SetPositions(strikePositions);
        SetUpLineRenderer(strikeLine);
        strikeLine.startWidth = mainLineWidth;
        strikeLine.endWidth = mainLineWidth;

        for(int i = 0; i <= m_horiDiv; ++i)
        {
            if(i == 0 || i == m_horiDiv) continue;
            CreateLine(startPos + new Vector3(widthSize * i, 0, 0), startPos + new Vector3(widthSize * i, strikeZoneHeight, 0));
        }

        for(int i = 0; i <= m_vertDiv; ++i)
        {
            if(i == 0 || i == m_vertDiv) continue;
            CreateLine(startPos + new Vector3(0, heightSize * i, 0), startPos + new Vector3(strikeZoneWidth, heightSize * i, 0));
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
        m_tracerMat = Resources.Load<Material>("BallTracer");
        m_strikeMat = Resources.Load<Material>("StrikeZone");
        // m_ballMat = Resources.Load<Material>("Transparent");
        m_ballMat = Resources.Load<Material>("BallTracer");
        m_vertDiv = verticalDivisionNumber;
        m_horiDiv = horizontalDivisionNumber;
        m_vertOffset = verticalBallZoneOffset;
        m_horiOffset = horizontalBallZoneOffset;
        m_widthNum = m_horiDiv + m_horiOffset * 2;
        m_heightNum = m_vertDiv + m_vertOffset * 2;
        m_pos = new Vector2(centerPosition.x, centerPosition.y);
        m_posZ = centerPosition.z;
        m_batterZones = new StrikeZoneComponent[m_widthNum * m_heightNum];
        CreateBatterZone();
        CreateBatterZoneLine();
        CreateBallTracer();
        HideBallTrace();
    }
}
