using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Motion : MonoBehaviour
{
    [SerializeField] string animationLayerName;
    [SerializeField] string animationBoolSymbol = "start";

    private Animator anim;
    private Vector3 initPos;
    private Quaternion initRot;
    private bool isAnimating;

    public void StartPitching()
    {
        if(anim.GetBool(animationBoolSymbol))
            anim.SetBool(animationBoolSymbol, false);
        InitTransform();
        isAnimating = true;
        // todo: StateMachineBehaviourを使おう！
        StartCoroutine(GameManager.instance.Wait(0.5f, () => {
            isAnimating = true;
            anim.SetBool(animationBoolSymbol, isAnimating);
        }));
    }

    public bool IsAnimating()
    {
        return isAnimating;
    }

    public bool Isis()
    {
        return anim.GetBool(animationBoolSymbol);
    }

    private void InitTransform()
    {
        this.transform.position = initPos;
        this.transform.rotation = initRot;
    }

    void Start()
    {
        anim = this.GetComponent<Animator>();
        initPos = this.transform.position;
        initRot = this.transform.rotation;
        isAnimating = false;
        anim.SetBool(animationBoolSymbol, isAnimating);
    }

    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Alpha2) ||
        //     Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Alpha4))
        if(Input.GetKeyDown(KeyCode.Alpha6))
        {
            StartPitching();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            isAnimating = false;
            anim.SetBool(animationBoolSymbol, isAnimating);
            InitTransform();
        }

        if(anim.GetCurrentAnimatorStateInfo(0).IsName(animationLayerName) && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
        {
            isAnimating = false;
            anim.SetBool(animationBoolSymbol, isAnimating);
        }
    }
}
