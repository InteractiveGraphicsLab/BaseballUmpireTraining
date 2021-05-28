using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatterZoneManager : MonoBehaviour
{
    [SerializeField] float lineLength = 0.43f / 3f * 7f;
    [SerializeField] float colunmLength = 1.4f;
    [SerializeField] float z = 0.43f;
    [SerializeField] int divNum = 7;
    [SerializeField] Material strikeMat;
    [SerializeField] Material ballMat;
    [SerializeField] Material tracerMat;

    private Renderer m_ballTracer;
    private Transform m_batterZoneParent;
    private Transform m_batterZoneLineParent;

    private BatterZoneComponent[] m_batterZones;

    public void ShowBallTrace(Vector3 pos)
    {
        m_ballTracer.gameObject.transform.localPosition = new Vector3(-pos.x, pos.y, -pos.z);

        m_batterZoneParent.gameObject.SetActive(true);
        m_batterZoneLineParent.gameObject.SetActive(true);
        m_ballTracer.gameObject.SetActive(true);
    }

    public void HideBallTrace()
    {
        m_batterZoneParent.gameObject.SetActive(false);
        m_batterZoneLineParent.gameObject.SetActive(false);
        m_ballTracer.gameObject.SetActive(false);
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
                // GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Quad);
                GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                panel.transform.SetParent(m_batterZoneParent);
                panel.transform.localPosition = new Vector3(startLine + j * lineSize, i * colunmSize, z);
                panel.transform.localScale = new Vector3(lineSize, colunmSize, 0.001f);
                isStrikeZone = 2 <= i && i <= 4 && 2 <= j && j <= 4;
                m_batterZones[i * divNum + j] = panel.AddComponent<BatterZoneComponent>();
                m_batterZones[i * divNum + j].Init(this, i * divNum + j, isStrikeZone, isStrikeZone ? strikeMat : ballMat);
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

    private void CreateBatterZoneLine()
    {
        m_batterZoneLineParent = new GameObject("BatterZoneLineParent").transform;
        m_batterZoneLineParent.SetParent(this.transform);
        m_batterZoneLineParent.localPosition = Vector3.zero;

        GameObject strikeFrame = new GameObject("StrikeZoneFrame");
        strikeFrame.transform.SetParent(m_batterZoneLineParent);
        strikeFrame.transform.localPosition = Vector3.zero;
        LineRenderer strikeLine = strikeFrame.AddComponent<LineRenderer>();
        Vector3[] strikePositions = new Vector3[4]{
            new Vector3(lineLength / 2f - lineLength / divNum * 2f, colunmLength / divNum * 2f, z),
            new Vector3(lineLength / 2f - lineLength / divNum * 2f, colunmLength - colunmLength / divNum * 2f, z),
            new Vector3(lineLength / -2f + lineLength / divNum * 2f, colunmLength - colunmLength / divNum * 2f, z),
            new Vector3(lineLength / -2f + lineLength / divNum * 2f, colunmLength / divNum * 2f, z)
        };
        strikeLine.positionCount = strikePositions.Length;
        strikeLine.SetPositions(strikePositions);
        SetUpLineRenderer(strikeLine);
        strikeLine.startWidth = 0.01f;
        strikeLine.endWidth = 0.01f;

        //todo 7分割前提
        Vector3[][] innerPositions = new Vector3[][]{
            new Vector3[]{
                new Vector3(lineLength / 2f - lineLength / divNum * 3f, colunmLength / divNum * 2f, z),
                new Vector3(lineLength / 2f - lineLength / divNum * 3f, colunmLength - colunmLength / divNum * 2f, z)
            },
            new Vector3[]{
                new Vector3(lineLength / 2f - lineLength / divNum * 4f, colunmLength / divNum * 2f, z),
                new Vector3(lineLength / 2f - lineLength / divNum * 4f, colunmLength - colunmLength / divNum * 2f, z)
            },
            new Vector3[]{
                new Vector3(lineLength / -2f + lineLength / divNum * 2f, colunmLength / divNum * 3f, z),
                new Vector3(lineLength / 2f - lineLength / divNum * 2f, colunmLength / divNum * 3f, z)
            },
            new Vector3[]{
                new Vector3(lineLength / -2f + lineLength / divNum * 2f, colunmLength / divNum * 4f, z),
                new Vector3(lineLength / 2f - lineLength / divNum * 2f, colunmLength / divNum * 4f, z)
            }
        };
        for(int i = 0; i < 4; i++)
        {
            GameObject strikeInnerFrame = new GameObject("StrikeInnerFrame");
            strikeInnerFrame.transform.SetParent(m_batterZoneLineParent);
            strikeInnerFrame.transform.localPosition = Vector3.zero;
            LineRenderer innerLine = strikeInnerFrame.AddComponent<LineRenderer>();
            innerLine.positionCount = innerPositions[i].Length;
            innerLine.SetPositions(innerPositions[i]);
            SetUpLineRenderer(innerLine);
            innerLine.startWidth = 0.001f;
            innerLine.endWidth = 0.001f;
        }
    }

    private void CreateBallTracer()
    {
        GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball.name = "BallTracer";
        ball.transform.localScale = 0.073f * Vector3.one;
        m_ballTracer = ball.GetComponent<Renderer>();
        m_ballTracer.material = tracerMat;
    }

    void Start()
    {
        m_batterZones = new BatterZoneComponent[49];
        CreateBatterZone();
        CreateBatterZoneLine();
        CreateBallTracer();
        HideBallTrace();
    }

    void Update()
    {

    }
}
