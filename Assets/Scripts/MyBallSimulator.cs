using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MyBallSimulator : MonoBehaviour {
    public GameObject ballObj;
    public GameObject tracerObj;

    Ball ball;
    Vector3 posOnBase;
    bool isPitching;
    float basePosZ;

    void Start() {
        basePosZ = StrikeZone.inst.center.z;
        // ball = Ball.Compose(73.4f, 1.51f, 6.35f, -53.89f, 36f, 88.2f, 2225, -0.09f, 3.21f);
        // ball = Ball.Compose(83.9f, 2.01f, 5.9f, -54.03f, 67f, 57.1f, 2226, -0.17f, 2.3f);
        ball = Ball.Compose(80.3f, 2f, 5.41f, -54.1f, 37f, 79f, 2998, -0.01f, 2.77f);
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Q)) {
            isPitching = false;
        }
        if (Input.GetKeyDown(KeyCode.P)) {
            isPitching = true;
            ball.Init();
        }
    }

    void FixedUpdate() {
        if (isPitching) {
            posOnBase = ball.pos;
            ball.Simulate(Time.deltaTime);
            // Debug.Log(ball.vel.magnitude * 3600f / 1000f);
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
