using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CarmeraMove : MonoBehaviour
{
    public Transform target;
    public Vector3 offsetpos;
    public float BodyHeight;
    public float moveSpeed;
    public float RotateSpeed;
 
   private Vector3 Carmerapos;

    private Quaternion CarmeraposRot;

    void Update()
    {
        if(target == null)
        {
            return;
        }
        Carmerapos=target.position + target.forward*offsetpos.z + Vector3.up*offsetpos.y + target.right*offsetpos.x;
        this.transform.position =Vector3.Lerp(this.transform.position, Carmerapos,Time.deltaTime*moveSpeed);


        CarmeraposRot = Quaternion.LookRotation(target.position + Vector3.up * BodyHeight - this.transform.position);

        this.transform.rotation=Quaternion.Slerp(this.transform.rotation,CarmeraposRot,Time.deltaTime*RotateSpeed);

        
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
    }
}
