using Assets.Scripts.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RolePanel : BasePanel
{
    public Button BtnLeft;
    public Button BtnRight;

    public Button BtnBegin;
    public Button BtnBack;

    public Button BtnBuy;
    public Text hasmoney;
    public Text heroName;
    public Text heromoney;


    public Transform HeroPos;
    public GameObject Hero;
    public RoleData nowHeroData;
    public int index=0;
    public override void Init()
    {
        HeroPos = GameObject.Find("HeroPos").transform;

        hasmoney.text = DataManager.Instance.playerData.HasMoney.ToString();

        BtnBegin.onClick.AddListener(() =>
        {
          DataManager.Instance.nowrole=nowHeroData;

            UIManager.Instance.HidePanel<RolePanel>();

            UIManager.Instance.ShowPanel<MapPanel>();
        });

        BtnLeft.onClick.AddListener(() =>
        {
            --index;
            if (index < 0)
            {
                index=DataManager.Instance.roleDataList.Count-1;
            }

            ChangeHero();
        });
        BtnRight.onClick.AddListener(() =>
        {
            ++index;
            if (index > DataManager.Instance.roleDataList.Count - 1)
            {
                index = 0;
            }
            ChangeHero();

        });
        BtnBack.onClick.AddListener(() =>
        {
            UIManager.Instance.HidePanel<RolePanel>();
            Camera.main.GetComponent<CarmraAnimator>().TurnRight(() =>
            {
               
                //显示选角界面
                UIManager.Instance.ShowPanel<BeginPanel>();
            });
        });
        BtnBuy.onClick.AddListener(() =>
        {
            PlayerData playerData= DataManager.Instance.playerData;
            if (playerData.HasMoney >= nowHeroData.lockMoney)
            {
                playerData.HasMoney-=nowHeroData.lockMoney;

                hasmoney.text=playerData.HasMoney.ToString() ;

                playerData.HeroBuy.Add(nowHeroData.id);


                DataManager.Instance.SavePlayerData();

                UnlockHero();

                //提示购买成功

                UIManager.Instance.ShowPanel<TipPanel>().ChangeInfo("购买成功");


            }
            else
            {
                //提示购买失败
                UIManager.Instance.ShowPanel<TipPanel>().ChangeInfo("金币不足");
            }
        });

        ChangeHero();
      
    }

    public void ChangeHero()
    {
        if(Hero != null)
        {
            Destroy(Hero);
            Hero = null;
        }
        nowHeroData =DataManager.Instance.roleDataList[index];

        Hero = Instantiate(Resources.Load<GameObject>(nowHeroData.res),HeroPos.position,HeroPos.rotation);

        Destroy(Hero.gameObject.GetComponent<PlayerObject>());

        heroName.text = nowHeroData.tips;

        UnlockHero();
    }
    public void UnlockHero()
    {
        if (nowHeroData.lockMoney > 0 && !DataManager.Instance.playerData.HeroBuy.Contains(nowHeroData.id))
        {
            BtnBuy.gameObject.SetActive(true);
            heromoney.text="￥:"+nowHeroData.lockMoney;
            BtnBegin.gameObject.SetActive(false);
        }
        else
        {
            BtnBuy.gameObject.SetActive(false);
            BtnBegin.gameObject.SetActive(true);
        }
    }
    public override void HideMe(UnityAction callback)
    {
        base.HideMe(callback);

        if (Hero!=null)
        {
            DestroyImmediate(Hero);
            Hero = null;
        }
    }

}
