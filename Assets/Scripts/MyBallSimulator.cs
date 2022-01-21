using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//datas: https://baseballsavant.mlb.com/statcast_search
public class MyBallSimulator : MonoBehaviour {
    public GameObject ballObj;
    public GameObject tracerObj;
    public TextAsset csv;

    List<Ball> presetBalls;

    Ball fourseam, cutter, slider, curve, sinker, ball;
    Vector3 posOnBase;
    bool isPitching;
    float basePosZ;

    public async void ReadPresetBalls() {

    }

    void Start() {
        basePosZ = StrikeZone.inst.center.z;

        // slider
        // ball = Ball.Compose(73.4f, 1.51f, 6.35f, -53.89f, 36f, 88.2f, 2225, -0.09f, 3.21f);
        // ball = Ball.Compose(83.9f, 2.01f, 5.9f, -54.03f, 67f, 57.1f, 2226, -0.17f, 2.3f);
        //curve
        curve = Ball.Compose("Someone", "Curve", "R", 80.3f, 2f, 5.41f, -54.1f, 37f, 79f, 2998, -0.01f, 2.77f);

        //Shohei Ohtani
        //fourseam / ball
        fourseam = Ball.Compose("Someone", "Fourseam", "R", 92.9f, 2.06f, 5.87f, -53.9f, 212f, 80.5f, 1994, -1.52f, 2.4f);
        //cutter / ball
        cutter = Ball.Compose("Someone", "Cutter", "R", 80.3f, 2f, 5.41f, -54.1f, 70f, 35.7f, 2333, 0.93f, 0.96f);
        //slider / strike
        slider = Ball.Compose("Someone", "Slider", "R", 87f, 2.1f, 5.88f, -53.7f, 89f, 57.1f, 2361, 0.77f, 1.88f);
        //curve / strike
        // curve = Ball.Compose(73.4f, 1.51f, 6.35f, -53.89f, 36f, 88.2f, 2225, -0.09f, 3.21f);
        //sinker /
        sinker = Ball.Compose("Someone", "Sinker", "R", 92.9f, 2.14f, 5.73f, -55.05f, 231f, 85.2f, 1920, -0.32f, 3.2f);
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Q)) {
            isPitching = false;
        }
        if (Input.GetKeyDown(KeyCode.P)) {
            isPitching = true;
            fourseam.Init();
            ball = fourseam;
        }
        if (Input.GetKeyDown(KeyCode.O)) {
            isPitching = true;
            cutter.Init();
            ball = cutter;
        }
        if (Input.GetKeyDown(KeyCode.I)) {
            isPitching = true;
            slider.Init();
            ball = slider;
        }
        if (Input.GetKeyDown(KeyCode.U)) {
            isPitching = true;
            curve.Init();
            ball = curve;
        }
        if (Input.GetKeyDown(KeyCode.Y)) {
            isPitching = true;
            sinker.Init();
            ball = sinker;
        }
    }

    void FixedUpdate() {
        if (isPitching) {
            posOnBase = ball.pos;
            ball.Simulate(Time.deltaTime);
            ballObj.transform.position = ball.pos;
            if (posOnBase.z <= basePosZ && basePosZ <= ball.pos.z) {
                float t = (basePosZ - ball.pos.z) / (posOnBase.z - ball.pos.z);
                posOnBase = ball.pos + t * (posOnBase - ball.pos);
                tracerObj.transform.position = posOnBase;
                isPitching = false;
            }
        }
    }
}
