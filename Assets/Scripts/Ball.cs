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
    static float mpm = 1.60934f;    // mile/meter;

    static int trialNum = 10;

    // z => y, x => z, y => -x
    public static Vector3 CalcAcceleration(Ball b) {
        float vz2 = Mathf.Pow(b.vel.z, 2f);
        float vx2 = Mathf.Pow(b.vel.x, 2f);
        float vy2 = Mathf.Pow(b.vel.y, 2f);

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

    public static Ball Compose(float velocity, Vector3 releasePosition, Vector2 spinAxis, int spinRate, Vector2 expectedPosOnBase) {
        return new Ball(velocity, releasePosition, spinAxis, spinRate, expectedPosOnBase);
    }

    public static Ball Compose(
        float velocityMile,
        float rePosx, float rePosy, float rePosz,
        float spinAxisxy, float activeSpin,
        int spinRate,
        float xOnPlate, float yOnPlate
        ) {
        float v = velocityMile * mpm * 1000f / 3600f;
        Vector3 rp = new Vector3(rePosx, rePosy, rePosz) / fpm;
        float theta = Mathf.Abs(spinAxisxy - 180f) * Mathf.Deg2Rad;
        float phi = Mathf.Abs(90f - Mathf.Acos(activeSpin / 100f)) * Mathf.Deg2Rad;
        Vector2 sa = new Vector2(theta, phi);
        Vector2 pop = new Vector2(xOnPlate, yOnPlate) / fpm;

        return Compose(v, rp, sa, spinRate, pop);
    }

    public string name { get; private set; }		// 球種
    public float initVelo { get; private set; }		// 初速度（m/s）
    public Vector3 initPos { get; private set; }	// リリースポジション，初期位置（m）
    public Quaternion initRot { get; private set; }	// 初速度方向
    public Vector2 spinAxis { get; private set; }	// 回転軸 [zx平面上の角度, zx平面とy軸との角度]
    public int spinRate { get; private set; }       // 回転数（rps）
    public Vector2 expectedPosOnBase { get; private set; }	// 予想されるホームベース上のボールの通過位置
    public float spinParam { get { return Mathf.PI * ballRadius * spinRate / vel.magnitude; } } 		// スピンパラメータ
    public float CD { get { return 0.188f * spinParam * spinParam + 1.1258f * spinParam + 0.3679f; } } 	// 抗力定数
    public float CL { get { return -0.4288f * spinParam * spinParam + 1.0002f * spinParam; } }          // 揚力定数

    // 時刻tの時の位置・速度
    public Vector3 pos { get; private set; }
    public Vector3 vel { get; private set; }

    public void Simulate(float dt) {
        vel += CalcAcceleration(this) * dt;
        pos += vel * dt;
    }

    public void Init() {
        Vector3 posOnBase = initPos;
        float basePosZ = StrikeZone.inst.center.z;

        initRot = Quaternion.Euler(0, 0, 1);

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
                    break;
                }
            }

            float dx = expectedPosOnBase.x - posOnBase.x;
            float dy = expectedPosOnBase.y - posOnBase.y;
            // float L = 18.44f - property.centerPosition.z;
            float L = basePosZ - initPos.z;
            float angleX = Mathf.Atan(dx / L) / Mathf.PI * 180;
            float angleY = Mathf.Atan(dy / L) / Mathf.PI * 180;
            initRot = Quaternion.Euler(-angleY, angleX, 0) * initRot;
            // Debug.Log($"[{trial}th] ({posOnBase.x}, {posOnBase.y}, {posOnBase.z}), Angle({initRot.eulerAngles.x}, {initRot.eulerAngles.y}, {initRot.eulerAngles.z})");
        }

        pos = initPos;
        vel = initRot * new Vector3(0, 0, initVelo);
    }

    Ball(float velocity, Vector3 releasePosition, Vector2 spinAxis, int spinRate, Vector2 posop) {
        (initVelo, initPos, spinAxis, spinRate, expectedPosOnBase) = (velocity, releasePosition, spinAxis, spinRate, posop);
        Debug.Log(spinAxis);
    }
}
