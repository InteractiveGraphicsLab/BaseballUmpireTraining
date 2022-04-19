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

    GameObject[] zones;
    GameObject ballTracer;
    GameObject strikeZone;
    GameObject strikeZoneLine;

    public bool IsStrike(Vector2 posOnBase) {
        var zoneLeft = center.x - zoneWidth / 2;
        var zoneRight = center.x + zoneWidth / 2;
        var zoneDown = center.y - zoneHeight / 2;
        var zoneUp = center.y + zoneHeight / 2;

        if (zoneLeft <= posOnBase.x && posOnBase.x <= zoneRight
        && zoneDown <= posOnBase.y && posOnBase.y <= zoneUp) {
            //absolutely strike
            return true;
        }

        var d = Ball.diameter;
        if (zoneLeft - d < posOnBase.x && posOnBase.x <= zoneLeft
        && zoneDown <= posOnBase.y && posOnBase.y <= zoneUp) {
            //strike left side
            return true;
        }

        if (zoneRight <= posOnBase.x && posOnBase.x < zoneRight + d
        && zoneDown <= posOnBase.y && posOnBase.y <= zoneUp) {
            //strike right side
            return true;
        }

        if (zoneLeft <= posOnBase.x && posOnBase.x <= zoneRight
        && zoneDown - d < posOnBase.y && posOnBase.y <= zoneDown) {
            //strike down side
            return true;
        }

        if (zoneLeft <= posOnBase.x && posOnBase.x <= zoneRight
        && zoneUp <= posOnBase.y && posOnBase.y < zoneUp + d) {
            //strike up side
            return true;
        }

        var r = Ball.radius;
        if ((posOnBase - new Vector2(zoneLeft, zoneDown)).magnitude < r
         || (posOnBase - new Vector2(zoneLeft, zoneUp)).magnitude < r
         || (posOnBase - new Vector2(zoneRight, zoneDown)).magnitude < r
         || (posOnBase - new Vector2(zoneRight, zoneUp)).magnitude < r) {
            //close strike
            return true;
        }

        return false;
    }

    public void ActiveTracer(Vector2 pos) {
        ballTracer.SetActive(true);
        ballTracer.transform.localPosition = new Vector3(pos.x, pos.y, centerPos.z);
    }

    public void InactiveTracer() {
        ballTracer.SetActive(false);
    }

    public void Visualize(Vector2 pos) {
        strikeZone.SetActive(true);
        strikeZoneLine.SetActive(true);
        ActiveTracer(pos);
    }

    public void Invisualize() {
        strikeZone.SetActive(false);
        strikeZoneLine.SetActive(false);
        InactiveTracer();
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
        strikeInnerFrame.transform.SetParent(strikeZoneLine.transform);
        strikeInnerFrame.transform.localPosition = Vector3.zero;
        LineRenderer innerLine = strikeInnerFrame.AddComponent<LineRenderer>();
        innerLine.SetPosition(0, start);
        innerLine.SetPosition(1, end);
        SetUpForLineRenderer(innerLine);
        innerLine.startWidth = subLineWidth;
        innerLine.endWidth = subLineWidth;
    }

    void DrawZoneLine() {
        strikeZoneLine = new GameObject("ZoneLines");
        strikeZoneLine.transform.SetParent(this.transform);
        strikeZoneLine.transform.localPosition = Vector3.zero;
        strikeZoneLine.transform.localRotation = Quaternion.identity;

        GameObject strikeFrame = new GameObject("StrikeZoneFrame");
        strikeFrame.transform.SetParent(strikeZoneLine.transform);
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

        zones = new GameObject[horiDivNum * vertDivNum];

        strikeZone = new GameObject("StrikeZone");
        strikeZone.transform.SetParent(this.transform);
        strikeZone.transform.localPosition = Vector3.zero;
        strikeZone.transform.localRotation = Quaternion.identity;

        for (int i = 0; i < horiDivNum; i++) {
            for (int j = 0; j < vertDivNum; j++) {
                GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                panel.transform.SetParent(strikeZone.transform);
                panel.transform.localPosition = new Vector3(startPos.x + i * panelSize.x, startPos.y + j * panelSize.y, center.z);
                panel.transform.localScale = new Vector3(panelSize.x, panelSize.y, zoneZScale);
                panel.GetComponent<Renderer>().material = m_zoneMat;
                zones[i * vertDivNum + j] = panel;
            }
        }

        DrawZoneLine();
    }

    void CreateBallTracer() {
        ballTracer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ballTracer.name = "BallTracer";
        ballTracer.transform.SetParent(this.transform);
        ballTracer.transform.localPosition = new Vector3(0, 0, center.z);
        ballTracer.transform.localScale = ballTracerScale * Vector3.one;
        ballTracer.GetComponent<Renderer>().material = m_tracerMat;
    }

    void Start() {
        CreateStrikeZone();
        CreateBallTracer();
        Invisualize();
    }
}
