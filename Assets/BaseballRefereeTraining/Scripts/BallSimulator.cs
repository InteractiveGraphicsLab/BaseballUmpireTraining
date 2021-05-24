using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class Param
{
    public static Vector3 zonePosLB = new Vector3(-0.43f * 3.5f / 3.0f, 0, 0.43f);
    public static float zoneWidth = 0.43f / 3.0f * 7.0f;
    public static float zoneHeight = 1.4f;
    public static int zoneDivNum = 7;

    public static Vector3 GRAVITY = new Vector3(0, -9.8f, 0);
    public static float   WEIGHT = 0.140f;
    public static float   BASE_Z = 0.0f;

    //変化球情報
    public static Vector3 initPosition    = new Vector3(-0.2f, 2.1f, 17.6f);
    // public static Vector3 initPosition    = new Vector3(-0.23f, 1.86f, 17.53f);

    //初速度
    public static Vector3 initVeloFast    = new Vector3(0, 0, -110.0f * 1000.0f / 3600.0f);
    public static Vector3 initVeloSlider  = new Vector3(0, 0, -100.0f * 1000.0f / 3600.0f);
    public static Vector3 initVeloCurve   = new Vector3(0, 0,  -90.0f * 1000.0f / 3600.0f);
    public static Vector3 initVeloScrew   = new Vector3(0, 0, -100.0f * 1000.0f / 3600.0f);

    //マグナス力（角度,力）
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
         Vector3    initPosition,
         Vector3    initVelocity,
         Quaternion initRotation,
         Vector3    force,
         float dt,
         int targetXi, //0,1,2..6
         int targetYi
         )
     {
        float targetX = (Param.zoneWidth  / Param.zoneDivNum) * (targetXi + 0.5f) + Param.zonePosLB[0];
        float targetY = (Param.zoneHeight / Param.zoneDivNum) * (targetYi + 0.5f) + Param.zonePosLB[1];

        Quaternion rot = initRotation;

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

                if (prePos[2] >= Param.zonePosLB[2] && Param.zonePosLB[2] >= p[2])
                {
                    float t = (Param.zonePosLB[2] - p[2]) / (prePos[2] - p[2]);
                    p = p + t * (prePos - p);
                    break;
                }
            }

            //補正
            //Debug.Log(trial + " COMPUTATION!! " + p + targetX + " " + targetY);

            //rotを更新
            float dx = targetX - p[0];
            float dy = targetY - p[1];
            float L = 18.44f - Param.zonePosLB[2];
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

    private Vector3 m_prevPos;
    private Vector3 m_pos   = new Vector3(0, 0, -100);
    private Vector3 m_velo  = new Vector3(0, 0, 0);
    private Vector3 m_force = new Vector3(0, 0, 0);

    private Vector3 m_nextPos   = new Vector3(0, 0, -100);
    private Vector3 m_nextVelo  = new Vector3(0, 0, 0);
    private Vector3 m_nextForce = new Vector3(0, 0, 0);
    // private int     count = 0;

    private float m_timecount  = 0.0f ; // ボタン押下後の経過時間
    private bool m_isPitching = false; // カウントの状態を表すフラグ
    private float m_dt = 0.02f;

    private TrailRenderer tr;

    public bool IsPitching()
    {
        return m_isPitching;
    }

    public void StartPitching()
    {
        m_pos = m_nextPos;
        m_velo = m_nextVelo;
        m_force = m_nextForce;

        this.transform.localPosition = Param.initPosition;
        tr.Clear();

        m_isPitching = true;
        m_timecount = 0;
    }

    public void Fastball(int line = -1, int colunm = -1)
    {
        SetParameterForBreakingball(Param.initPosition, Param.initVeloFast, Param.rotFast, Param.forceFast, line, colunm);
    }

    public void Curveball(int line = -1, int colunm = -1)
    {
        SetParameterForBreakingball(Param.initPosition, Param.initVeloCurve, Param.rotCurve, Param.forceCurve, line, colunm);
    }

    public void Sliderball(int line = -1, int colunm = -1)
    {
        SetParameterForBreakingball(Param.initPosition, Param.initVeloSlider, Param.rotSlider, Param.forceSlider, line, colunm);
    }

    public void Screwball(int line = -1, int colunm = -1)
    {
        SetParameterForBreakingball(Param.initPosition, Param.initVeloScrew, Param.rotScrew, Param.forceScrew, line, colunm);
    }

    private void SetParameterForBreakingball(Vector3 pos, Vector3 velo, Quaternion rotVelo, Vector3 force, int line, int colunm)
    {
        if((Param.zoneDivNum <= line || line < 0) || (Param.zoneDivNum <= colunm || colunm < 0))
        {
            line = Random.Range(0, Param.zoneDivNum);
            colunm = Random.Range(0, Param.zoneDivNum);
            Debug.Log(line + ", " + colunm);
        }

        Quaternion q = BallDirection.calcBallInitialRotation(pos, velo, rotVelo, force, m_dt, line, colunm);
        m_nextPos   = pos;
        m_nextVelo  = q * velo;
        m_nextForce = force;
    }

    private void Start() {
        tr = this.GetComponent<TrailRenderer>();
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
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            StartPitching();
        }

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (m_isPitching)
        {
            m_timecount += Time.deltaTime;
            //if (m_timecount > 6.0)
            //{
            //    m_timecount = 0;
            //    m_isPitching = false;
            //}

            if (m_timecount < waitTime) return;

            m_prevPos = m_pos;
            //位置、速度の更新
            if (m_pos.z > Param.BASE_Z)
            {
                //Simulation
                m_velo = m_velo + (Param.GRAVITY + m_force / Param.WEIGHT) * Time.deltaTime;
                m_pos = m_pos + m_velo * Time.deltaTime;
            }

            // this.transform.position = m_pos;
            this.transform.localPosition = m_pos;

            //zoneを跨いだ瞬間
            if (m_prevPos.z > Param.zonePosLB.z && Param.zonePosLB.z >= m_pos.z)
            {
                //内分点の計算
                float a = (0.43f - m_pos.z) / (m_prevPos.z - m_pos.z);
                Vector3 zp = m_pos + (a * (m_prevPos - m_pos));

                int xi = (int)((zp.x - Param.zonePosLB.x) / Param.zoneWidth * Param.zoneDivNum);
                int yi = (int)((zp.y - Param.zonePosLB.y) / Param.zoneHeight * Param.zoneDivNum);

                Debug.Log("ゾーン到達点のposの座標 " + zp);
                Debug.Log("ゾーン到達点のposの座標 " + xi + " " + yi);

                // たぶんきっとif(m_pos.z < 0f)より先に処理されるはず
                ballManager.EndPitching(xi, yi);
            }

            // todo: mit の位置で終了したい
            if(m_pos.z < 0f)
            {
                m_isPitching = false;
            }
        }
    }
}