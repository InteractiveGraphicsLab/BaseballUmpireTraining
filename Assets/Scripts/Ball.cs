using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 加速度の計算
// https://baseballorbitsimulator.blogspot.com/2020/03/8ver32.html
// 抗力定数と揚力定数
// http://www.ipc.tohoku-gakuin.ac.jp/nken/labo2018/ball_net/theory.pdf

// x軸: 三塁方向
// y軸: 空方向
// z軸: ホームベース方向
// 原点: キャッチャー
public class Ball {
    static float gravity = -9.81f;
    static float airDensity = 1.166f; //20度

    static float ballMass = 0.145f;
    static float ballDiameter = 0.073f;
    static float ballRadius { get { return ballDiameter / 2; } }
    static float ballArea { get { return Mathf.PI * ballRadius * ballRadius; } }    // 断面積

    static float fpm = 3.281f;      // feet/meter;
    static float kmpm = 1.60934f;    // kilometer/mile;

    static int trialNum = 100;

    // z => y, x => z, y => -x
    public static Vector3 CalcAcceleration(Ball b) {
        float vz2 = b.vel.z * Mathf.Abs(b.vel.z);
        float vx2 = b.vel.x * Mathf.Abs(b.vel.x);
        float vy2 = b.vel.y * Mathf.Abs(b.vel.y);

        float za = -1f * b.CD * vz2;
        float zb = -1f * b.CL * Mathf.Cos(b.spinAxis[0]) * vx2;
        float zc = b.CL * Mathf.Sin(b.spinAxis[0]) * Mathf.Sin(b.spinAxis[1]) * vy2;

        float xa = b.CL * Mathf.Cos(b.spinAxis[0]) * vz2;
        float xb = -1f * b.CD * vx2;
        float xc = -1f * b.CL * Mathf.Sin(b.spinAxis[0]) * Mathf.Cos(b.spinAxis[1]) * vy2;

        float ya = -1f * b.CL * Mathf.Sin(b.spinAxis[0]) * Mathf.Sin(b.spinAxis[1]) * vz2;
        float yb = b.CL * Mathf.Sin(b.spinAxis[0]) * Mathf.Cos(b.spinAxis[1]) * vx2;
        float yc = -1f * b.CD * vy2;

        float w = airDensity * ballArea / (2f * ballMass);

        return w * new Vector3(-(xa + xb + xc), ya + yb + yc + gravity, za + zb + zc);
    }

    public static Ball Compose(string name, string type, bool isRightThrow, float velocity, Vector3 releasePosition, Vector2 spinAxis, float spinRate, Vector2 expectedPosOnBase) {
        return new Ball(name, type, isRightThrow, velocity, releasePosition, spinAxis, spinRate, expectedPosOnBase);
    }

    public static Ball Compose(
        string pitcherName, string ballType, string domHand,
        float velocityMile, //km/h
        float rePosx, float rePosy, float rePosz, // m, m, m
        float spinAxisxy, float activeSpin,
        int spinRate, // rpm
        float xOnPlate, float yOnPlate //m, m
        ) {
        bool isRight = domHand == "R";
        float v = velocityMile * kmpm * 1000f / 3600f;
        Vector3 rp = new Vector3(rePosx, rePosy, rePosz) / fpm;

        float alpha = (90f - spinAxisxy + 360f) % 360f;
        float phi = 90f - Mathf.Acos(activeSpin / 100f) * Mathf.Rad2Deg;
        float theta = Mathf.Atan(Mathf.Tan(alpha * Mathf.Deg2Rad) / Mathf.Sin(phi * Mathf.Deg2Rad)) * Mathf.Rad2Deg;
        if (alpha > 90f)
            theta += 180f;
        if (!isRight) // ???
            phi = -phi;
        Vector2 sa = new Vector2(theta, phi) * Mathf.Deg2Rad;

        float rate = (float)spinRate / 60f;
        Vector2 pop = new Vector2(xOnPlate, yOnPlate) / fpm;

        Debug.Log($"name:{ballType}\nspinAxis:{spinAxisxy}, activeSpin:{activeSpin}\ntheta:{sa.x * Mathf.Rad2Deg}, phi:{sa.y * Mathf.Rad2Deg}");

        return Compose(pitcherName, ballType, isRight, v, rp, sa, rate, pop);
    }

    public string pitcherName { get; private set; }	// 投手名
    public string balltype { get; private set; }	// 球種
    public bool isRightThrow { get; private set; }  // 右投げor左投げ
    public float initVelo { get; private set; }		// 初速度（m/s）
    public Vector3 initPos { get; private set; }	// リリースポジション，初期位置（m）
    public Quaternion initRot { get; private set; }	// 初速度方向
    public Vector2 spinAxis { get; private set; }	// 回転軸 [zx平面上の角度, zx平面とy軸との角度]
    public float spinRate { get; private set; }     // 回転数（rps）
    public Vector2 expectedPosOnBase { get; private set; }	// 予想されるホームベース上のボールの通過位置
    public float spinParam { get { return Mathf.PI * ballDiameter * spinRate / vel.magnitude; } } 		// スピンパラメータ
    public float CD { get { return 0.188f * spinParam * spinParam + 1.1258f * spinParam + 0.3679f; } } 	// 抗力定数
    public float CL { get { return -0.4288f * spinParam * spinParam + 1.0002f * spinParam; } }          // 揚力定数

    // 時刻tの時の位置・速度
    public Vector3 pos { get; private set; }
    public Vector3 vel { get; private set; }

    public void Simulate(float dt) {
        // Debug.Log($"Acc:{CalcAcceleration(this).ToString("f9")}\nCD:{CD.ToString("f9")}, CL{CL.ToString("f9")}, SP:{spinParam.ToString("f9")}\nr:{ballRadius.ToString("f9")}, rate:{spinRate.ToString("f9")}, v:{vel.magnitude.ToString("f9")}");
        vel += CalcAcceleration(this) * dt;
        pos += vel * dt;
    }

    public void Init() {
        Vector3 posOnBase = initPos;
        float basePosZ = StrikeZone.inst.center.z;

        initRot = Quaternion.Euler(0, 0, 0);

        // Debug.Log($"[Ans] ({expectedPosOnBase.x}, {expectedPosOnBase.y})");
        for (int trial = 0; trial < trialNum; trial++) {
            pos = initPos;
            vel = initRot * new Vector3(0, 0, initVelo);

            for (int i = 0; i < 300; ++i) {
                posOnBase = pos;
                Simulate(0.02f);
                if (posOnBase.z <= basePosZ && basePosZ <= pos.z) {
                    float t = (basePosZ - pos.z) / (posOnBase.z - pos.z);
                    posOnBase = pos + t * (posOnBase - pos);
                    // Debug.Log($"exit!, pob:{posOnBase.x.ToString("f9")}");
                    break;
                }
                // Debug.Log($"pos:{pos.ToString("f9")}");
            }

            float dx = expectedPosOnBase.x - posOnBase.x;
            float dy = expectedPosOnBase.y - posOnBase.y;
            // float L = 18.44f - property.centerPosition.z;
            float L = basePosZ - initPos.z;
            float angleX = Mathf.Atan(dx / L) * Mathf.Rad2Deg;
            float angleY = Mathf.Atan(dy / L) * Mathf.Rad2Deg;
            if (angleX == 0 && angleY == 0) continue;
            initRot = Quaternion.Euler(-angleY, angleX, 0) * initRot;
            // Debug.Log($"[{trial}th] ({posOnBase.x}, {posOnBase.y}, {posOnBase.z}), Angle({initRot.eulerAngles.x}, {initRot.eulerAngles.y}, {initRot.eulerAngles.z})");
        }

        pos = initPos;
        vel = initRot * new Vector3(0, 0, initVelo);
        // Debug.Log("initRot" + initRot.eulerAngles);
        // Debug.Log($"expectedPosOnBase:{expectedPosOnBase.ToString("f9")}\nposOnBase:{posOnBase.ToString("f9")}");
    }

    Ball(string name, string type, bool isRight, float velocity, Vector3 releasePosition, Vector2 saxis, float rate, Vector2 posop) {
        (pitcherName, balltype, isRightThrow, initVelo, initPos, spinAxis, spinRate, expectedPosOnBase) = (name, type, isRight, velocity, releasePosition, saxis, rate, posop);
        Init();
        // Debug.Log("initVelo: " + (initVelo * 3600f / 1000f) + "km/h");
        // Debug.Log("initPos: " + initPos);
        // Debug.Log("SpinAxis: " + (spinAxis * Mathf.Rad2Deg));
        // Debug.Log("spinRate: " + spinRate);
        // Debug.Log("expectedPosOnBase: " + expectedPosOnBase);
    }

    public Ball(Ball b) :
    this(b.pitcherName, b.balltype, b.isRightThrow, b.initVelo, b.initPos, b.spinAxis, b.spinRate, b.expectedPosOnBase) { }
}
