using System.Collections.Generic;
using UnityEngine;

public class RandomBall : MonoBehaviour {
    public static Vector2 pitchingRangeWidth = new Vector2(-0.45f, 0.45f);
    public static Vector2 pitchingRangeHeight = new Vector2(0.15f, 1.25f);
    public static Vector3 releasePos = new Vector3(0.4f, 1.8f, -16.4f);
    // public static Vector3 releasePos = new Vector3(-0.2f, 2f, 17.6f);

    static Dictionary<BallData.Type, float> spinAxisDict = new Dictionary<BallData.Type, float>() {
        {BallData.Type.Fourseam, 210f},
        {BallData.Type.Curve, 135f},
        {BallData.Type.Cutter, 85f},
        {BallData.Type.Changeup, 235f},
        {BallData.Type.Sinker, 120f},
        {BallData.Type.Slider, 70f},
        {BallData.Type.Splitter, 230f}
    };

    [SerializeField] BallData.Type ballType;
    [SerializeField] float minVelocity = 70f, maxVelocity = 150f;
    [SerializeField] float minSpinRate = 2000f, maxSpinRate = 2500f;
    [SerializeField] float spinAxisDelta = 10f;

    public Ball Compose() {
        var v = Random.Range(minVelocity, maxVelocity);
        var sr = (int)Random.Range(minSpinRate, maxSpinRate);
        var sa = spinAxisDict[ballType] + Random.Range(-spinAxisDelta, spinAxisDelta);
        var ep = new Vector2(Random.Range(pitchingRangeWidth[0], pitchingRangeWidth[1]), Random.Range(pitchingRangeHeight[0], pitchingRangeHeight[1]));
        return Ball.Compose("", ballType, true, v, releasePos, sa, BallData.GetActiveSpin(ballType), sr, ep);
    }
}
