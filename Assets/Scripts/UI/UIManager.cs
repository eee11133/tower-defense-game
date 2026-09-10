using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.UI
{
    internal class UIManager
    {
        private static UIManager instance=new UIManager();

        public static UIManager Instance => instance;

  

        private Dictionary<string, BasePanel> paneldic= new Dictionary<string, BasePanel>();

        private Transform canvastrans;

        public UIManager()
        {
            GameObject canvas = GameObject.Instantiate(Resources.Load<GameObject>("UI/Canvas"));
            canvastrans=canvas.transform;
           GameObject.DontDestroyOnLoad(canvas);
        }
        public T ShowPanel<T>()where T : BasePanel
        {
            string panelName=typeof(T).Name;

            if(paneldic.ContainsKey(panelName))
            {
                return paneldic[panelName] as T;
            }
            GameObject panelobj = GameObject.Instantiate(Resources.Load<GameObject>("UI/" + panelName));
           
            panelobj.transform.SetParent(canvastrans,false);
            
            T panel = panelobj.GetComponent<T>();

            paneldic.Add(panelName, panel);

            panel.ShowMe();

            return panel;
        }

        public void HidePanel<T>(bool isFade=true) where T : BasePanel
        {
            string panelName = typeof(T).Name;
            if (paneldic.ContainsKey(panelName))
            {
                if (isFade)
                {
                    paneldic[panelName].HideMe(() =>
                    {
                        GameObject.Destroy(paneldic[panelName].gameObject);
                        paneldic.Remove(panelName);
                    });

                }
                else
                {
                    GameObject.Destroy(paneldic[panelName].gameObject);
                    paneldic.Remove(panelName);
                }


            }


        }

        public T GetPanel<T>()where T : BasePanel
        {
            string panelName = typeof(T).Name;

            if (paneldic.ContainsKey(panelName))
            {
                return paneldic[panelName] as T;
            }
            return null;
        }

    }
}
