using UnityEngine;
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
        float vx2 = v.x * Mathf.Abs(v.x);
        float vy2 = v.y * Mathf.Abs(v.y);

        float za = -1f * b.CD * vz2;
        float zb = -1f * b.CL * Mathf.Cos(b.spinAxis[0]) * vx2;
        float zc = b.CL * Mathf.Sin(b.spinAxis[0]) * Mathf.Sin(b.spinAxis[1]) * vy2;

        float xa = b.CL * Mathf.Cos(b.spinAxis[0]) * vz2;
        float xb = -1f * b.CD * vx2;
        float xc = -1f * b.CL * Mathf.Sin(b.spinAxis[0]) * Mathf.Cos(b.spinAxis[1]) * vy2;

        float ya = -1f * b.CL * Mathf.Sin(b.spinAxis[0]) * Mathf.Sin(b.spinAxis[1]) * vz2;
        float yb = b.CL * Mathf.Sin(b.spinAxis[0]) * Mathf.Cos(b.spinAxis[1]) * vx2;
        float yc = -1f * b.CD * vy2;

        float w = airDensity * Ball.area / (2f * Ball.mass);

        return w * new Vector3(-(xa + xb + xc), ya + yb + yc + gravity, za + zb + zc);
    }

    public static (Vector3, Vector3) Simulate(Ball b, Vector3 p, Vector3 v, float dt) {
        v += CalcAcceleration(b, v) * dt;
        p += v * dt;
        b.UpdateTransform(p, v);
        return (p, v);
    }

    public GameObject ballObj;
    public GameObject tracerObj;
    public TextAsset csv;

    // Ball fourseam, cutter, slider, curve, sinker;
    // Ball b1, b2, b3, b4, b5;

    Ball thrownBall, nextBall;
    bool isPitching;
    float basePosZ;
    Vector3 vel, pos, posBuf;

    public void SetBall(Ball ball) {
        nextBall = ball;
    }

    public void Throw(Ball ball) {
        thrownBall = ball;
        pos = thrownBall.initPos;
        vel = thrownBall.initVelo;
        thrownBall.UpdateTransform(pos, vel);
        isPitching = true;
    }

    void Throw() {
        Throw(nextBall ?? thrownBall);
        nextBall = null;
    }

    async UniTask Start() {
        basePosZ = StrikeZone.inst.center.z;

        // slider
        // ball = Ball.Compose(73.4f, 1.51f, 6.35f, -53.89f, 36f, 88.2f, 2225, -0.09f, 3.21f);
        // ball = Ball.Compose(83.9f, 2.01f, 5.9f, -54.03f, 67f, 57.1f, 2226, -0.17f, 2.3f);
        //curve
        // curve = Ball.Compose("Someone", BallData.Type.Curve, "R", 80.3f, 2f, 5.41f, -54.1f, 37f, 79f, 2998, -0.01f, 2.77f);

        //Shohei Ohtani
        //fourseam / ball
        // fourseam = Ball.Compose("Someone", BallData.Type.Fourseam, "R", 92.9f, 2.06f, 5.87f, -53.9f, 212f, 80.5f, 1994, -1.52f, 2.4f);
        //cutter / ball
        // cutter = Ball.Compose("Someone", BallData.Type.Cutter, "R", 80.3f, 2f, 5.41f, -54.1f, 70f, 35.7f, 2333, 0.93f, 0.96f);
        //slider / strike
        // slider = Ball.Compose("Someone", BallData.Type.Slider, "R", 87f, 2.1f, 5.88f, -53.7f, 89f, 57.1f, 2361, 0.77f, 1.88f);
        //curve / strike
        // curve = Ball.Compose(73.4f, 1.51f, 6.35f, -53.89f, 36f, 88.2f, 2225, -0.09f, 3.21f);
        //sinker /
        // sinker = Ball.Compose("Someone", BallData.Type.Sinker, "R", 92.9f, 2.14f, 5.73f, -55.05f, 231f, 85.2f, 1920, -0.32f, 3.2f);

        // await BallData.LoadPitcherData("Ohtani");

        // b1 = BallData.ComposeBallOfIndex(34);
        // b2 = BallData.ComposeBallOfIndex(453);
        // b3 = BallData.ComposeBallOfIndex(756);
        // b4 = BallData.ComposeBallOfIndex(93);
        // b5 = BallData.ComposeBallOfIndex(275);
    }

    void Update() {
        if (OVRInput.GetDown(OVRInput.RawButton.A)) {
            Throw();
        }
    }

    void FixedUpdate() {
        if (isPitching) {
            posBuf = pos;
            (pos, vel) = Simulate(thrownBall, pos, vel, Time.deltaTime);
            ballObj.transform.position = pos;
            if (posBuf.z <= basePosZ && basePosZ <= pos.z) {
                float t = (basePosZ - pos.z) / (posBuf.z - pos.z);
                posBuf = pos + t * (posBuf - pos);
                // todo
                tracerObj.transform.position = posBuf;
                isPitching = false;
            }
        }
    }
}
