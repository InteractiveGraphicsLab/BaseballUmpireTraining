using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mit : MonoBehaviour
{
    [SerializeField] Transform ball;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (ball.position.z > 0f)
        {
            this.transform.position = new Vector3(ball.position.x, ball.position.y, this.transform.position.z);
        }
    }
}
