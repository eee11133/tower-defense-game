using Assets.Scripts.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapPanel : BasePanel
{

    public Button BtnStart;
    public Button BtnBack;
    public Button BtnLeft;
    public Button BtnRight;
    public Text MapName;
    public Text MapDescription;

    public Image SenceImg;

    private int index=0;
    private MapData nowMapindex;
    public override void Init()
    {
        BtnStart.onClick.AddListener(() =>
        {

            UIManager.Instance.HidePanel<MapPanel>();


            AsyncOperation ao = SceneManager.LoadSceneAsync(nowMapindex.sceneName);


            ao.completed += (a) =>
            {


                LevelDataMgr.Instance.InitInfo(nowMapindex);
            };

     

        });
        BtnBack.onClick.AddListener(() =>
        {
            UIManager.Instance.HidePanel<MapPanel>();
            UIManager.Instance.ShowPanel<RolePanel>();

        });
        BtnLeft.onClick.AddListener(() =>
        {
            --index;
            if (index < 0)
            {
                index = DataManager.Instance.mapDataList.Count - 1;

            }
            ChangeMap();
        });
        BtnRight.onClick.AddListener(() =>
        {
            ++index;
            if (index > DataManager.Instance.mapDataList.Count - 1)
            {
                index = 0;

            }
            ChangeMap();
        });
        ChangeMap();
    }
    public void ChangeMap()
    {
        nowMapindex=DataManager.Instance.mapDataList[index];
        SenceImg.sprite = Resources.Load<Sprite>(nowMapindex.imgRes);
        MapName.text="名称："+nowMapindex.name;
        MapDescription.text="详情："+nowMapindex.tips;
    }
}
