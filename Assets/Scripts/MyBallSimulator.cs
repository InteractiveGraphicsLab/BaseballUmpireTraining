using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

//datas: https://baseballsavant.mlb.com/statcast_search
public class MyBallSimulator : MonoBehaviour {
    static float gravity = -9.81f;
    static float airDensity = 1.166f; //20度

    // 加速度の計算
    // https://baseballorbitsimulator.blogspot.com/2020/03/8ver32.html
    // z => y, x => z, y => -x
    public static Vector3 CalcAcceleration(Ball b, Vector3 v) {
        float vz2 = v.z * Mathf.Abs(v.z);
        float vx2 = -v.x * Mathf.Abs(v.x);
        float vy2 = v.y * Mathf.Abs(v.y);

        float za = -b.CD * vz2;
        float zb = -b.CL * Mathf.Cos(b.spinAxis[0]) * vx2;
        float zc = b.CL * Mathf.Sin(b.spinAxis[0]) * Mathf.Sin(b.spinAxis[1]) * vy2;

        float xa = b.CL * Mathf.Cos(b.spinAxis[0]) * vz2;
        float xb = -b.CD * vx2;
        float xc = -b.CL * Mathf.Sin(b.spinAxis[0]) * Mathf.Cos(b.spinAxis[1]) * vy2;

        float ya = -b.CL * Mathf.Sin(b.spinAxis[0]) * Mathf.Sin(b.spinAxis[1]) * vz2;
        float yb = b.CL * Mathf.Sin(b.spinAxis[0]) * Mathf.Cos(b.spinAxis[1]) * vx2;
        float yc = -b.CD * vy2;

        float w = airDensity * Ball.area / (2f * Ball.mass);

        return w * new Vector3(-(xa + xb + xc), ya + yb + yc, za + zb + zc) + new Vector3(0, gravity, 0);
    }

    public static (Vector3, Vector3) Simulate(Ball b, Vector3 p, Vector3 v, float dt) {
        v += CalcAcceleration(b, v) * dt;
        p += v * dt;
        b.UpdateTransform(p, v);
        return (p, v);
    }

    [SerializeField] OVRInput.Button throwInput;
    [SerializeField] List<RandomBall> randomBalls;
    [SerializeField] float animeWaitingTime = 2.3f;

    public bool isPitching { get; private set; }
    public Vector2 posOnBase => new Vector2(prevPos.x, prevPos.y);
    public Ball thrownBall { get; private set; }
    GameObject ballObj;
    Ball nextBall;
    float basePosZ, time = 0;
    Vector3 vel, pos, prevPos;

    public void SetBall(Ball ball) {
        nextBall = ball;
    }

    public void Throw(GameObject ballObject, Ball ball) {
        if (ball == null) return;
        ballObj = ballObject;
        ballObj.SetActive(true);
        thrownBall = ball;
        Debug.Log($"type: {ball.balltype}, velo: {ball.velocity}, spinrate: {ball.spinRate * 60f}, axis: {ball.spinAxis * Mathf.Rad2Deg}, pop: {ball.expectedPosOnBase.ToString()}");
        pos = thrownBall.initPos;
        vel = thrownBall.initVelo;
        ballObj.transform.position = pos;
        thrownBall.UpdateTransform(pos, vel);
        time = 0;
        isPitching = true;
    }

    public void Throw(GameObject ballObject) {
        Throw(ballObject, nextBall ?? thrownBall);
        nextBall = null;
    }

    public void ThrowRandom(GameObject ballObject) {
        var ball = randomBalls[Random.Range(0, randomBalls.Count)].Compose();
        Throw(ballObject, ball);
    }

    async UniTask Start() {
        basePosZ = StrikeZone.inst.center.z;
        await BallData.LoadActiveSpins();
    }

    void Update() {
        // if (OVRInput.GetDown(throwInput)) {
        //     // Throw();
        //     ThrowRandom();
        // }
        // if (Input.GetKeyDown(KeyCode.P)) {
        //     ThrowRandom();
        // }
    }

    void FixedUpdate() {
        var dt = Time.deltaTime;
        if (isPitching) {
            time += dt;
            if (time < animeWaitingTime) {
                return;
            }
            prevPos = pos;
            (pos, vel) = Simulate(thrownBall, pos, vel, dt);
            ballObj.transform.position = pos;
            if (prevPos.z <= basePosZ && basePosZ <= pos.z) {
                float t = (basePosZ - pos.z) / (prevPos.z - pos.z);
                prevPos = pos + t * (prevPos - pos);
                isPitching = false;
                ballObj.SetActive(false);
                ballObj = null;
            }
        }
    }
}
