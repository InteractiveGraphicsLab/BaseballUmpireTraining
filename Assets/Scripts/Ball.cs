using UnityEngine;

// 抗力定数と揚力定数
// http://www.ipc.tohoku-gakuin.ac.jp/nken/labo2018/ball_net/theory.pdf

// x軸: 三塁方向
// y軸: 空方向
// z軸: ホームベース方向
// 原点: キャッチャー
public class Ball {
    public static float mass { get; } = 0.145f; //kg
    public static float diameter { get; } = 0.073f; //m
    public static float radius { get { return diameter / 2; } } //m
    public static float area { get { return Mathf.PI * radius * radius; } }

    static float fpm = 3.281f;      // feet/meter;
    static float kmpm = 1.60934f;    // kilometer/mile;

    static int trialNum = 10;

    // public static Ball Compose(string name, BallData.Type type, bool isRightThrow, float velocity, Vector3 releasePosition, Vector2 spinAxis, float spinRate, Vector2 expectedPosOnBase) {
    //     return new Ball(name, type, isRightThrow, velocity, releasePosition, spinAxis, spinRate, expectedPosOnBase);
    // }

    public static Ball Compose(
        string pitcherName, BallData.Type ballType, bool isRightThrow,
        float velocity, //km/h
        Vector3 releasePosition, // m
        float spinAxisxy, float activeSpin,
        int spinRate, // rpm
        Vector2 posOnPlate // m
    ) {
        string name = pitcherName.Trim('"');
        float v = velocity * 1000f / 3600f;

        // float alpha = (90f - spinAxisxy + 360f) % 360f;
        float alpha = Mathf.Abs(spinAxisxy - 90f);
        if (alpha > 180f) alpha = 360f - alpha;
        float phi = 90f - Mathf.Acos(activeSpin / 100f) * Mathf.Rad2Deg;
        float theta;
        if (alpha > 90f)
            theta = Mathf.Atan(Mathf.Tan((alpha - 90f) * Mathf.Deg2Rad) / Mathf.Sin(phi * Mathf.Deg2Rad)) * Mathf.Rad2Deg + 90f;
        else
            theta = Mathf.Atan(Mathf.Tan(alpha * Mathf.Deg2Rad) / Mathf.Sin(phi * Mathf.Deg2Rad)) * Mathf.Rad2Deg;
        if (90f < spinAxisxy && spinAxisxy < 270f) phi = -phi;
        Vector2 sa = new Vector2(theta, phi) * Mathf.Deg2Rad;

        float rate = spinRate / 60f;

        // Debug.Log($"name:{ballType}\nspinAxis:{spinAxisxy}, activeSpin:{activeSpin}\ntheta:{sa.x * Mathf.Rad2Deg}, phi:{sa.y * Mathf.Rad2Deg}");

        return new Ball(name, ballType, isRightThrow, v, releasePosition, sa, rate, posOnPlate);
    }

    public static Ball Compose(
        string pitcherName, BallData.Type ballType, string domHand,
        float velocityMile, //mile
        float rePosx, float rePosy, float rePosz, //feet
        float spinAxisxy, float activeSpin,
        int spinRate, // rpm
        float xOnPlate, float yOnPlate //feet
    ) {
        string name = pitcherName.Trim('"');
        bool isRight = domHand == "R";
        float v = velocityMile * kmpm;
        Vector3 rp = new Vector3(rePosx, rePosy, rePosz) / fpm;
        Vector2 pop = new Vector2(xOnPlate, yOnPlate) / fpm;

        // Debug.Log($"name:{ballType}\nspinAxis:{spinAxisxy}, activeSpin:{activeSpin}\ntheta:{sa.x * Mathf.Rad2Deg}, phi:{sa.y * Mathf.Rad2Deg}");

        return Compose(name, ballType, isRight, v, rp, spinAxisxy, activeSpin, spinRate, pop);
    }

    public static Quaternion CalcRotation(Ball b) {
        Vector3 p, v;
        Vector3 posOnBase = b.initPos;
        // float basePosZ = StrikeZone.inst.center.z;
        float basePosZ = 0;
        Quaternion q = Quaternion.Euler(0, 0, 0);

        // Debug.Log($"[Ans] ({expectedPosOnBase.x}, {expectedPosOnBase.y})");
        for (int trial = 0; trial < trialNum; trial++) {
            p = b.initPos;
            v = q * new Vector3(0, 0, b.releaseVelo);
            b.UpdateTransform(p, v); //Simulateを実行するために

            for (int i = 0; i < 300; ++i) {
                posOnBase = p;
                (p, v) = MyBallSimulator.Simulate(b, p, v, 0.02f);
                if (posOnBase.z <= basePosZ && basePosZ <= p.z) {
                    float t = (basePosZ - p.z) / (posOnBase.z - p.z);
                    posOnBase = p + t * (posOnBase - p);
                    // Debug.Log($"exit!, pob:{posOnBase.x.ToString("f9")}");
                    break;
                }
                // Debug.Log($"pos:{pos.ToString("f9")}");
            }

            float dx = b.expectedPosOnBase.x - posOnBase.x;
            float dy = b.expectedPosOnBase.y - posOnBase.y;
            // float L = 18.44f - property.centerPosition.z;
            float L = basePosZ - b.initPos.z;

            float angleX = Mathf.Atan(dx / L) * Mathf.Rad2Deg;
            float angleY = Mathf.Atan(dy / L) * Mathf.Rad2Deg;

            if (angleX == 0 && angleY == 0) continue;
            if (float.IsNaN(angleX) || float.IsNaN(angleY) || float.IsInfinity(angleX) || float.IsInfinity(angleY)) break;
            // Debug.Log($"type: {b.balltype}, speed: {b.velocity} \n angleX: {angleX.ToString("F7")}, angleY: {angleY.ToString("F7")}");
            q = Quaternion.Euler(-angleY, angleX, 0) * q;
            // Debug.Log($"[{trial}th] ({posOnBase.x}, {posOnBase.y}, {posOnBase.z}), Angle({releaseRot.eulerAngles.x}, {releaseRot.eulerAngles.y}, {releaseRot.eulerAngles.z})");
        }

        return q;
        // Debug.Log("releaseRot" + releaseRot.eulerAngles);
        // Debug.Log($"expectedPosOnBase:{expectedPosOnBase.ToString("f9")}\nposOnBase:{posOnBase.ToString("f9")}");
    }

    public string id { get; private set; }                                              // Guid
    public string pitcherName { get; private set; }	                                    // 投手名
    public BallData.Type balltype { get; private set; } 	                            // 球種
    public bool isRightThrow { get; private set; }                                      // 右投げor左投げ
    public float releaseVelo { get; private set; }		                                // 初速度（m/s）
    public float velocity => releaseVelo * 3.6f;                                        // 速度（km/h）
    public Quaternion releaseRot { get; private set; }	                                // 初速度方向
    public Vector3 initPos { get; private set; }	                                    // リリースポジション，初期位置（m）
    public Vector3 initVelo => releaseRot * new Vector3(0, 0, releaseVelo);             // 方向付き初速度
    public Vector2 spinAxis { get; private set; }	                                    // 回転軸 [zx平面上の角度, zx平面とy軸との角度]
    public float spinRate { get; private set; }                                         // 回転数（rps）
    public Vector2 expectedPosOnBase { get; private set; }	                            // 予想されるホームベース上のボールの通過位置
    public float spinParam => Mathf.PI * diameter * spinRate / vel.magnitude;		    // スピンパラメータ
    public float CD => 0.188f * spinParam * spinParam + 1.1258f * spinParam + 0.3679f;	// 抗力定数
    public float CL => -0.4288f * spinParam * spinParam + 1.0002f * spinParam;          // 揚力定数

    public Vector3 vel { get; private set; }                                            // 現在の速度
    public Vector3 pos { get; private set; }                                            // 現在の位置

    //spinParam更新用にSimulate毎に速度を更新しないといけない
    public void UpdateTransform(Vector3 p, Vector3 v) {
        (pos, vel) = (p, v);
    }

    Ball(string name, BallData.Type type, bool isRight, float velo, Vector3 releasePosition, Vector2 saxis, float rate, Vector2 posop, string _id = null, Quaternion? velRot = null) {
        (pitcherName, balltype, isRightThrow, releaseVelo, initPos, spinAxis, spinRate, expectedPosOnBase) = (name, type, isRight, velo, releasePosition, saxis, rate, posop);
        id = _id ?? System.Guid.NewGuid().ToString("N");
        releaseRot = velRot ?? CalcRotation(this);
        UpdateTransform(initPos, initVelo);
        // Debug.Log("releaseVelo: " + (releaseVelo * 3600f / 1000f) + "km/h");
        // Debug.Log("initPos: " + initPos);
        // Debug.Log("SpinAxis: " + (spinAxis * Mathf.Rad2Deg));
        // Debug.Log("spinRate: " + spinRate);
        // Debug.Log("expectedPosOnBase: " + expectedPosOnBase);
    }

    public Ball() { }

    public Ball(Ball b) :
    this(b.pitcherName, b.balltype, b.isRightThrow, b.releaseVelo, b.initPos, b.spinAxis, b.spinRate, b.expectedPosOnBase, b.id, b.releaseRot) { }
}
