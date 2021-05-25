using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mit : MonoBehaviour
{
    [SerializeField] Transform ball;
    private Vector3 initPos;

    // Start is called before the first frame update
    void Start()
    {
        initPos = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (ball.position.z > 0f)
        {
            this.transform.position = new Vector3(ball.position.x, ball.position.y, this.transform.position.z);
        }
        else
        {
            this.transform.position = initPos;
        }
    }
}
