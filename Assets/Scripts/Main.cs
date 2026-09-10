using Assets.Scripts.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro.EditorUtilities;
using UnityEngine;

namespace Assets.Scripts
{
    internal class Main:MonoBehaviour
    {

         void Start()
        {
            UIManager.Instance.ShowPanel<BeginPanel>();
          
        }
    }
}
