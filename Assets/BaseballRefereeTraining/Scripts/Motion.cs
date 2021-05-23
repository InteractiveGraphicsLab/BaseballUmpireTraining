using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Motion : MonoBehaviour
{
    private Animator anim;
    private Vector3 initPos;
    private Quaternion initRot;

    [SerializeField] string animation_name;

    void Start()
    {
        anim = this.gameObject.GetComponent<Animator>();
        initPos = this.transform.position;
        initRot = this.transform.rotation;
    }

    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Alpha2) ||
        //     Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Alpha4))
        if(Input.GetKeyDown(KeyCode.Alpha6))
        {
            anim.SetBool("start", false);
            this.transform.rotation = initRot;
            this.transform.position = initPos;
            anim.SetBool("start", true);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            anim.SetBool("start", false);
            this.transform.rotation = initRot;
            this.transform.position = initPos;
        }

        if(anim.GetCurrentAnimatorStateInfo(0).IsName(animation_name) && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
            anim.SetBool("start", false);
    }
}
