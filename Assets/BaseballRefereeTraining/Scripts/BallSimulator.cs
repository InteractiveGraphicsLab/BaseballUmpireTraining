using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class Param
{
    // public static Vector3 zonePosLB = new Vector3(-0.43f * 3.5f / 3.0f, 0, 0);
    // public static Vector3 zonePosLB = new Vector3(-0.43f * 3.5f / 3.0f, 0, -0.43f);
    // public static float zoneWidth = 0.43f / 3.0f * 7.0f;
    // public static float zoneHeight = 1.4f;
    // public static int zoneDivNum = 7;

    public static Vector3 GRAVITY = new Vector3(0, -9.8f, 0);
    public static float   WEIGHT = 0.140f;
    public static float   BASE_Z = -0.6f;

    //変化球情報
    public static Vector3 initPosition    = new Vector3(-0.2f, 2f, 17.6f);
    // public static Vector3 initPosition    = new Vector3(-0.23f, 1.86f, 17.53f);

    //初速度
    // public static Vector3 initVeloFast    = new Vector3(0, 0, -110.0f * 1000.0f / 3600.0f);
    // public static Vector3 initVeloSlider  = new Vector3(0, 0, -100.0f * 1000.0f / 3600.0f);
    // public static Vector3 initVeloCurve   = new Vector3(0, 0,  -90.0f * 1000.0f / 3600.0f);
    // public static Vector3 initVeloScrew   = new Vector3(0, 0, -100.0f * 1000.0f / 3600.0f);

    //マグナス力（角度,力）
    // todo 力もランダムにしたい．めっちゃ曲がる
    public static float[] tmFast   = { -2.0f / 180.0f * Mathf.PI, 1.2f };
    public static float[] tmSlider = { 50.0f / 180.0f * Mathf.PI, 1.0f };
    public static float[] tmCurve  = { 90.0f / 180.0f * Mathf.PI, 0.5f };
    public static float[] tmScrew  = { -20.0f / 180.0f * Mathf.PI, 1.0f };

    //マグナス力がかかる方向
    public static Vector3 forceFast   = tmFast  [1] * (new Vector3(Mathf.Sin(tmFast  [0]), Mathf.Cos(tmFast  [0]), 0));
    public static Vector3 forceSlider = tmSlider[1] * (new Vector3(Mathf.Sin(tmSlider[0]), Mathf.Cos(tmSlider[0]), 0));
    public static Vector3 forceCurve  = tmCurve [1] * (new Vector3(Mathf.Sin(tmCurve [0]), Mathf.Cos(tmCurve [0]), 0));
    public static Vector3 forceScrew  = tmScrew [1] * (new Vector3(Mathf.Sin(tmScrew [0]), Mathf.Cos(tmScrew [0]), 0));

    //回転行列
    public static Quaternion rotFast   = Quaternion.Euler(-2.5f, -0.2f, 0.0f);
    public static Quaternion rotSlider = Quaternion.Euler( 0.0f,  4.0f, 0.0f);
    public static Quaternion rotCurve  = Quaternion.Euler( 5.2f,  3.0f, 0.0f);
    public static Quaternion rotScrew  = Quaternion.Euler(-1.5f, -1.5f, 0.0f);
}

public class BallDirection
{
    public static Quaternion calcBallInitialRotation(
         StrikeZoneProperty property,
         Vector3    initPosition,
         Vector3    initVelocity,
         Quaternion initRotation,
         Vector3    force,
         float dt,
         int targetXi, //0,1,2..6
         int targetYi
         )
     {
        float targetX = property.panelSize.x * (targetXi + 0.5f) + (property.centerPosition.x - property.zoneSize.x / 2f);
        float targetY = property.panelSize.y * (targetYi + 0.5f) + (property.centerPosition.y - property.zoneSize.y / 2f);

		// Quaternion rot = initRotation;
		Quaternion rot = Quaternion.Euler(0,0,1);

		for (int trial = 0; trial < 5; trial++)
        {
            //実際に投球
            Vector3 p = initPosition;
            Vector3 v = rot * initVelocity;

            for (int kk = 0; kk < 300; ++kk)
            {
                Vector3 prePos = p;
                v += (Param.GRAVITY + force / Param.WEIGHT) * dt;
                p += v * dt;

                if (prePos.z >= property.centerPosition.z && property.centerPosition.z >= p.z)
                {
                    float t = (property.centerPosition.z - p.z) / (prePos.z - p.z);
                    p = p + t * (prePos - p);
                    break;
                }
            }

            //補正
            // Debug.Log(trial + " COMPUTATION!! " + p + " " + targetX + " " + targetY);

            //rotを更新
            float dx = targetX - p.x;
            float dy = targetY - p.y;
            // todo
            float L = 18.44f - property.centerPosition.z;
            float angleX = Mathf.Atan(dx / L) / Mathf.PI * 180;
            float angleY = Mathf.Atan(dy / L) / Mathf.PI * 180;
            rot = Quaternion.Euler(angleY, -angleX, 0) * rot;
        }

        return rot;
    }
}

//todo 0 このコードの説明（3年生の自分が分かるように）
//todo 1 パラメタの微調整
//todo 2 回転行列の導入
//todo 3 投げるzoneの指定(井尻)
public class BallSimulator : MonoBehaviour
{
    [SerializeField] BallManager ballManager;
    [SerializeField] float waitTime = 2.30f;

    [SerializeField] float minVelocityFast = 110f;
    [SerializeField] float maxVelocityFast = 160f;
    [SerializeField] float minVelocityCurve = 70f;
    [SerializeField] float maxVelocityCurve = 110f;
    [SerializeField] float minVelocitySlider = 100f;
    [SerializeField] float maxVelocitySlider = 140f;
    [SerializeField] float minVelocityScrew = 100f;
    [SerializeField] float maxVelocityScrew = 140f;
    [SerializeField] Material transparentMat;
    [SerializeField] Material ballTextureMat;

    private Vector3 m_prevPos;
    private Vector3 m_pos   = new Vector3(0, 0, -100);
    private Vector3 m_velo  = new Vector3(0, 0, 0);
    private Vector3 m_force = new Vector3(0, 0, 0);

    private Vector3 m_nextPos   = new Vector3(0, 0, -100);
    private Vector3 m_nextVelo  = new Vector3(0, 0, 0);
    private Vector3 m_nextForce = new Vector3(0, 0, 0);
    private float m_velocityBuff = 0;
    // private int     count = 0;

    private Renderer m_renderer;
    private TrailRenderer m_trail;
    private float m_dt = 0.02f;
    private float m_timecount  = 0.0f ; // ボタン押下後の経過時間
    private bool m_isPitching = false; // カウントの状態を表すフラグ
    private bool m_isPause = false;

    public void Init()
    {
        m_timecount  = 0.0f ;
        m_isPitching = false;
    }

    public bool IsPitching()
    {
        return m_isPitching;
    }

    public void Pause(bool isPause)
    {
        m_isPause = isPause;
    }

    public void Trail(float time = 0)
    {
        m_trail.time = time;
    }

    public void InitPosition()
    {
        this.transform.localPosition = Param.initPosition;
        m_trail.Clear();
    }

    public void StartPitching()
    {
        m_pos = m_nextPos;
        m_velo = m_nextVelo;
        m_force = m_nextForce;

        InitPosition();

        m_isPitching = true;
        m_timecount = 0;
    }

    private int[] RandomTarget()
    {
        StrikeZoneProperty property = ballManager.strikeZoneProperty;
        int[] target = new int[2];

        if(Random.value <= 0.5f)
        {
            //strike
            target[0] = Random.Range(property.horiOffset + 1, property.horiOffset + property.horiDivNum);
            target[1] = Random.Range(property.vertOffset + 1, property.vertOffset + property.vertDivNum);
        }
        else
        {
            //ball
            target[0] = Random.Range(0, property.horiOffset * 2);
            if(target[0] > property.horiOffset - 1) target[0] += property.horiDivNum;
            target[1] = Random.Range(0, property.vertOffset * 2);
            if(target[1] > property.vertOffset - 1) target[1] += property.vertDivNum;
        }

        return target;
    }

    private void SetParameterForBreakingball(Vector3 pos, Vector3 velo, Quaternion rotVelo, Vector3 force, int line, int colunm)
    {
        Quaternion q = BallDirection.calcBallInitialRotation(ballManager.strikeZoneProperty, pos, velo, rotVelo, force, m_dt, line, colunm);
        m_nextPos   = pos;
        m_nextVelo  = q * velo;
        m_nextForce = force;
        // Debug.Log("line, column: (" + line + ", " + colunm + ")");
    }

    public void Fastball(float velocity = 0f, int line = -1, int colunm = -1)
    {
        if(velocity == 0f)
        {
            velocity = Random.Range(minVelocityFast, maxVelocityFast);
            // Debug.Log("Fastball Velocity: " + velocity);
        }
        m_velocityBuff = velocity;

        if(!ballManager.strikeZoneProperty.IsInZone(line, colunm))
        {
            int[] target = RandomTarget();
            line = target[0];
            colunm = target[1];
            target = null;
            // Debug.Log("Fastball line, column: (" + line + ", " + colunm + ")");
        }

        SetParameterForBreakingball(Param.initPosition, new Vector3(0, 0, velocity * -1000.0f / 3600.0f), Param.rotFast, Param.forceFast, line, colunm);
    }

    public void Curveball(float velocity = 0f, int line = -1, int colunm = -1)
    {
        if(velocity == 0f)
        {
            velocity = Random.Range(minVelocityCurve, maxVelocityCurve);
            // Debug.Log("Curveball Velocity: " + velocity);
        }
        m_velocityBuff = velocity;

        if(!ballManager.strikeZoneProperty.IsInZone(line, colunm))
        {
            int[] target = RandomTarget();
            line = target[0];
            colunm = target[1];
            target = null;
            // Debug.Log("Curveball line, column: (" + line + ", " + colunm + ")");
        }

        SetParameterForBreakingball(Param.initPosition, new Vector3(0, 0, velocity * -1000.0f / 3600.0f), Param.rotCurve, Param.forceCurve, line, colunm);
    }

    public void Sliderball(float velocity = 0f, int line = -1, int colunm = -1)
    {
        if(velocity == 0f)
        {
            velocity = Random.Range(minVelocitySlider, maxVelocitySlider);
            // Debug.Log("Sliderball Velocity: " + velocity);
        }
        m_velocityBuff = velocity;

        if(!ballManager.strikeZoneProperty.IsInZone(line, colunm))
        {
            int[] target = RandomTarget();
            line = target[0];
            colunm = target[1];
            target = null;
            // Debug.Log("Sliderball line, column: (" + line + ", " + colunm + ")");
        }

        SetParameterForBreakingball(Param.initPosition, new Vector3(0, 0, velocity * -1000.0f / 3600.0f), Param.rotSlider, Param.forceSlider, line, colunm);
    }

    public void Screwball(float velocity = 0f, int line = -1, int colunm = -1)
    {
        if(velocity == 0f)
        {
            velocity = Random.Range(minVelocityScrew, maxVelocityScrew);
            // Debug.Log("Screwball Velocity: " + velocity);
        }
        m_velocityBuff = velocity;

        if(!ballManager.strikeZoneProperty.IsInZone(line, colunm))
        {
            int[] target = RandomTarget();
            line = target[0];
            colunm = target[1];
            target = null;
            // Debug.Log("Screwball line, column: (" + line + ", " + colunm + ")");
        }

        SetParameterForBreakingball(Param.initPosition, new Vector3(0, 0, velocity * -1000.0f / 3600.0f), Param.rotScrew, Param.forceScrew, line, colunm);
    }

    private void Start() {
        m_trail = this.GetComponent<TrailRenderer>();
        m_renderer = this.GetComponent<Renderer>();
        m_renderer.material = transparentMat;
        this.transform.localPosition = Param.initPosition;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Fastball();
            //StartPitching();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Curveball();
            //StartPitching();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Sliderball();
            //StartPitching();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Screwball();
            //StartPitching();
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            StartPitching();
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (m_isPitching && !m_isPause)
        {
            m_timecount += Time.deltaTime;
            //if (m_timecount > 6.0)
            //{
            //    m_timecount = 0;
            //    m_isPitching = false;
            //}

            if (m_timecount < waitTime) return;

            m_renderer.material = ballTextureMat;
            m_prevPos = m_pos;
            //位置、速度の更新
            if (m_pos.z > Param.BASE_Z)
            {
                //Simulation
                m_velo = m_velo + (Param.GRAVITY + m_force / Param.WEIGHT) * Time.deltaTime;
                m_pos = m_pos + m_velo * Time.deltaTime;
            }
            else
            {
                m_renderer.material = transparentMat;
                m_isPitching = false;
            }

            // this.transform.position = m_pos;
            this.transform.localPosition = m_pos;

            //zoneを跨いだ瞬間
            float z = ballManager.strikeZoneProperty.centerPosition.z;
            if (m_prevPos.z > z && z >= m_pos.z)
            {
                //内分点の計算
                float a = (z - m_pos.z) / (m_prevPos.z - m_pos.z);
                Vector3 zp = m_pos + (a * (m_prevPos - m_pos));

                StrikeZoneProperty p = ballManager.strikeZoneProperty;
                float LBx = p.centerPosition.x - p.zoneSize.x / 2f;
                float LBy = p.centerPosition.y - p.zoneSize.y / 2f;
                int xi = (int)((zp.x - LBx) / p.zoneSize.x * p.horiPanelNum);
                int yi = (int)((zp.y - LBy) / p.zoneSize.y * p.vertPanelNum);

                // Debug.Log("ゾーン到達点のposの座標 " + zp);
                // Debug.Log("ゾーン到達点のposの座標 " + xi + " " + yi);

                // たぶんきっとif(m_pos.z < 0f)より先に処理されるはず
                ballManager.EndPitching(m_velocityBuff, xi, yi, zp);
            }
        }
    }
}