using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
public class TrailBall : MonoBehaviour {
    public GameObject obj => this.gameObject;
    public TrailRenderer trailRenderer { get; private set; }

    void Start() {
        trailRenderer = GetComponent<TrailRenderer>();
    }
}
