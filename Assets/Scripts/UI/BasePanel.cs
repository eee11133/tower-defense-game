using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public abstract class BasePanel:MonoBehaviour 
{
    private CanvasGroup canvasGroup;

    private float alphaSpeed=10;

    public bool isShow=false;

    public UnityAction hidecallback=null;
    protected virtual void Awake()
    {
        canvasGroup=this.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup=this.gameObject.AddComponent<CanvasGroup>();
        }
    }
    public abstract void Init();
    protected virtual void Start()
    {
        Init();
    }

    public virtual void ShowMe()
    {
        canvasGroup.alpha = 0;
        isShow=true;
    }
    public virtual void HideMe(UnityAction callback)
    {
        canvasGroup.alpha = 1;
         isShow=false;
        hidecallback=callback;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (isShow && canvasGroup.alpha != 1)
        {
            
                canvasGroup.alpha += alphaSpeed * Time.deltaTime;
                if(canvasGroup.alpha >= 1)
                {
                    canvasGroup.alpha = 1;
                }
            

        }
        else if (!isShow && canvasGroup.alpha != 0)
        {
            canvasGroup.alpha -= alphaSpeed * Time.deltaTime;
            if (canvasGroup.alpha <= 0)
            {
                canvasGroup.alpha = 0;
                hidecallback?.Invoke();
            }
        }
    }
}
