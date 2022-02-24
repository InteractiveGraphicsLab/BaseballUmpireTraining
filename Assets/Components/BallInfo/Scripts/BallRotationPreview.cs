using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallRotationPreview : MonoBehaviour {
    [SerializeField] GameObject ball;
    [SerializeField] GameObject axisObj;
    [SerializeField] float ballSpinRate = 1f;
    [SerializeField] float rotationRate = 1f;

    Vector3 axis = Vector3.one;
    float spinRate = 0;

    public void Rerotate(Vector2 angleAxis, float spin) {
        spinRate = spin;
        axisObj.transform.localRotation = Quaternion.identity;
        ball.transform.localRotation = Quaternion.identity;
        this.transform.localRotation = Quaternion.identity;

        axis = -new Vector3(
            -Mathf.Sin(angleAxis[0]) * Mathf.Sin(angleAxis[1]),
            Mathf.Cos(angleAxis[0]),
            Mathf.Sin(angleAxis[0]) * Mathf.Cos(angleAxis[1])
        );

        axisObj.transform.LookAt(axis + axisObj.transform.position);
        axisObj.transform.localRotation *= Quaternion.AngleAxis(90f, Vector3.right);
    }

    void FixedUpdate() {
        float dt = Time.deltaTime;
        ball.transform.localRotation *= Quaternion.AngleAxis(dt * spinRate * ballSpinRate, axis);
        this.transform.localRotation *= Quaternion.AngleAxis(dt * rotationRate, Vector3.up);
    }
}
