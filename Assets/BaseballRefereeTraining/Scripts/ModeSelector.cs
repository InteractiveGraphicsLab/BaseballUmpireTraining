using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModeSelector : MonoBehaviour
{
    [SerializeField] OVRInput.Button input = OVRInput.Button.PrimaryIndexTrigger;
    [SerializeField] Mode mode;
    [SerializeField] string righthand = "HandCollierR";
    [SerializeField] string lefthand = "HandCollierL";
    [SerializeField] Material material;
    [SerializeField] Material outlineMat;
    private Renderer renderer;

    private void OnTriggerStay(Collider other)
    {
        if((other.name == righthand && OVRInput.GetDown(input, OVRInput.Controller.RTouch)) ||
           (other.name == lefthand && OVRInput.GetDown(input, OVRInput.Controller.LTouch)))
        {
            GameManager.instance.ChangeMode(mode);
            GameManager.instance.Play();
            renderer.material = material;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == righthand || other.name == lefthand)
        {
            renderer.material = outlineMat;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.name == righthand || other.name == lefthand)
        {
            renderer.material = material;
        }
    }

    void Start()
    {
        renderer = this.GetComponent<Renderer>();
    }

    void Update()
    {

    }
}
