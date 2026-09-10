using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CarmraAnimator : MonoBehaviour
{

    public Animator animator;
    // Start is called before the first frame update

    public UnityAction carmeraact;
    void Start()
    {
        animator=this.GetComponent<Animator>();
    }


    public void TurnLeft(UnityAction action)
    {
        animator.SetTrigger("Left");
        carmeraact = action;
    }
    // Update is called once per frame
    public void TurnRight(UnityAction action)
    {
        animator.SetTrigger("Right");
        carmeraact = action;
    }

    public void  PlayOver()
    {
        carmeraact?.Invoke();
        carmeraact=null;
    }
}
