using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.RestService;
using UnityEngine;

public class DataManager 
{
    private static DataManager instance=new DataManager();
    public static DataManager Instance => instance;

    public MusicData musicData;

    public List<RoleData> roleDataList;

    public PlayerData playerData;

    public  RoleData nowrole;

    public List<WeaponData> WeaponDataList;

    public List<MapData> mapDataList;

    public List<MonsterData> monsterDataList;

    public List<TowerData> TowerDataList;
    public DataManager()
    {
        musicData = JsonMgr.Instance.LoadData<MusicData>("MusicData");

        playerData = JsonMgr.Instance.LoadData<PlayerData>("PlayerData");

        WeaponDataList = JsonMgr.Instance.LoadData<List<WeaponData>>("WeaponData");
        
        roleDataList = JsonMgr.Instance.LoadData<List<RoleData>>("RoleData");

        mapDataList = JsonMgr.Instance.LoadData<List<MapData>>("MapData");

        monsterDataList = JsonMgr.Instance.LoadData<List<MonsterData>>("MonsterData");

        TowerDataList = JsonMgr.Instance.LoadData<List<TowerData>>("TowerData");
    }

    public void SaveMusicData()
    {
        JsonMgr.Instance.SaveData(musicData, "MusicData");

       
    }
    public void SavePlayerData()
    {

        JsonMgr.Instance.SaveData(playerData, "PlayerData");
    }

    public void PlaySound(string resName)
    {
        GameObject musicObj = new GameObject();
        AudioSource a=musicObj.AddComponent<AudioSource>();
        a.clip = Resources.Load<AudioClip>(resName);
        a.volume=musicData.SoundValue;
        a.mute = !musicData.Sound;
        a.Play();

        GameObject.Destroy(musicObj,1);
    }
}
