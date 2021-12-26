using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrikeZone : MonoBehaviour {
    static StrikeZone _inst;
    public static StrikeZone inst {
        get {
            if (_inst == null)
                _inst = (StrikeZone)FindObjectOfType(typeof(StrikeZone));
            return _inst;
        }
    }

    [SerializeField] Vector3 centerPos = new Vector3(0, 0.7f, 0);
    [SerializeField] float zoneWidth = 0.432f;
    [SerializeField] float zoneHeight = 0.6f;
    [SerializeField] int verticalDivNum = 4;
    [SerializeField] int horizontalDivNum = 4;

    [SerializeField] float mainLineWidth = 0.01f;
    [SerializeField] float subLineWidth = 0.001f;
    [SerializeField] float zoneZScale = 0.001f;
    [SerializeField] float ballTracerScale = 0.073f;
    [SerializeField] Material m_zoneMat;
    [SerializeField] Material m_tracerMat;

    public Vector3 center { get { return centerPos; } }
    public Vector2 size { get { return new Vector2(zoneWidth, zoneHeight); } }
    public int vertDivNum { get { return verticalDivNum; } }
    public int horiDivNum { get { return horizontalDivNum; } }
    public Vector2 panelSize { get { return new Vector2(size.x / horiDivNum, size.y / vertDivNum); } }

    GameObject[] m_zones;
    GameObject m_ballTracer;
    GameObject m_strikeZone;
    GameObject m_strikeZoneLine;

    public void SetTracer(bool isActive) {
        m_ballTracer.SetActive(isActive);
    }
    public void SetTracer(bool isActive, Vector2 pos) {
        if (isActive)
            m_ballTracer.transform.localPosition = new Vector3(-pos.x, pos.y, m_ballTracer.transform.localPosition.z);
        SetTracer(isActive);
    }

    public void MakeAvailable(bool isAvailable) {
        m_strikeZone.SetActive(isAvailable);
        m_strikeZoneLine.SetActive(isAvailable);
        SetTracer(isAvailable);
    }

    void SetUpForLineRenderer(LineRenderer line) {
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = Color.black;
        line.endColor = Color.black;
        line.numCapVertices = 10;
        line.numCornerVertices = 10;
        line.loop = true;
        line.widthMultiplier = 0.2f;
    }

    void CreateLine(Vector3 start, Vector3 end) {
        GameObject strikeInnerFrame = new GameObject("StrikeInnerFrame");
        strikeInnerFrame.transform.SetParent(m_strikeZoneLine.transform);
        strikeInnerFrame.transform.localPosition = Vector3.zero;
        LineRenderer innerLine = strikeInnerFrame.AddComponent<LineRenderer>();
        innerLine.SetPosition(0, start);
        innerLine.SetPosition(1, end);
        SetUpForLineRenderer(innerLine);
        innerLine.startWidth = subLineWidth;
        innerLine.endWidth = subLineWidth;
    }

    void DrawZoneLine() {
        m_strikeZoneLine = new GameObject("ZoneLines");
        m_strikeZoneLine.transform.SetParent(this.transform);
        m_strikeZoneLine.transform.localPosition = Vector3.zero;
        m_strikeZoneLine.transform.localRotation = Quaternion.identity;

        GameObject strikeFrame = new GameObject("StrikeZoneFrame");
        strikeFrame.transform.SetParent(m_strikeZoneLine.transform);
        strikeFrame.transform.localPosition = Vector3.zero;
        strikeFrame.transform.localRotation = Quaternion.identity;
        LineRenderer strikeLine = strikeFrame.AddComponent<LineRenderer>();
        Vector3 startPos = center + new Vector3(-size.x, -size.y, 0) / 2f;

        Vector3[] strikePositions = new Vector3[4]{
            startPos + new Vector3(0, 0, 0),
            startPos + new Vector3(0, size.y, 0),
            startPos + new Vector3(size.x, size.y, 0),
            startPos + new Vector3(size.x, 0, 0)
        };
        strikeLine.positionCount = strikePositions.Length;
        strikeLine.SetPositions(strikePositions);
        SetUpForLineRenderer(strikeLine);
        strikeLine.startWidth = mainLineWidth;
        strikeLine.endWidth = mainLineWidth;

        for (int i = 0; i <= horiDivNum; ++i) {
            if (i == 0 || i == horiDivNum) continue;
            CreateLine(startPos + new Vector3(panelSize.x * i, 0, 0), startPos + new Vector3(panelSize.x * i, size.y, 0));
        }

        for (int i = 0; i <= vertDivNum; ++i) {
            if (i == 0 || i == vertDivNum) continue;
            CreateLine(startPos + new Vector3(0, panelSize.y * i, 0), startPos + new Vector3(size.x, panelSize.y * i, 0));
        }
    }

    void CreateStrikeZone() {
        bool isStrikeZone;
        Vector2 startPos = new Vector2(center.x, center.y) + new Vector2(-(size.x - panelSize.x), -(size.y - panelSize.y)) / 2f;

        m_zones = new GameObject[horiDivNum * vertDivNum];

        m_strikeZone = new GameObject("StrikeZone");
        m_strikeZone.transform.SetParent(this.transform);
        m_strikeZone.transform.localPosition = Vector3.zero;
        m_strikeZone.transform.localRotation = Quaternion.identity;

        for (int i = 0; i < horiDivNum; i++) {
            for (int j = 0; j < vertDivNum; j++) {
                GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                panel.transform.SetParent(m_strikeZone.transform);
                panel.transform.localPosition = new Vector3(startPos.x + i * panelSize.x, startPos.y + j * panelSize.y, center.z);
                panel.transform.localScale = new Vector3(panelSize.x, panelSize.y, zoneZScale);
                panel.GetComponent<Renderer>().material = m_zoneMat;
                m_zones[i * vertDivNum + j] = panel;
            }
        }

        DrawZoneLine();
    }

    void CreateBallTracer() {
        m_ballTracer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        m_ballTracer.name = "BallTracer";
        m_ballTracer.transform.SetParent(this.transform);
        m_ballTracer.transform.localPosition = new Vector3(0, 0, center.z);
        m_ballTracer.transform.localScale = ballTracerScale * Vector3.one;
        m_ballTracer.GetComponent<Renderer>().material = m_tracerMat;
    }

    void Start() {
        CreateStrikeZone();
        CreateBallTracer();
        // MakeAvailable(false);
    }
}
