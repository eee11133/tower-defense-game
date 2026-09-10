using Assets.Scripts.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingPanel : BasePanel
{
   public Button btnclose;
    public Toggle togMusic;
    public Toggle togSound;
    public Slider sliderMusic;
    public Slider sliderSound;


    // Start is called before the first frame update
    public override void Init()
    {
        MusicData musicData =DataManager.Instance.musicData;

        togMusic.isOn = musicData.Music;
        togSound.isOn = musicData.Sound;
        sliderMusic.value = musicData.MusicValue;
        sliderSound.value = musicData.SoundValue;

        btnclose.onClick.AddListener(() =>
        {
            DataManager.Instance.SaveMusicData();

            UIManager.Instance.HidePanel<SettingPanel>();

        });

        togMusic.onValueChanged.AddListener((v) =>
        {
            BKMusic.Instance.MusicOpen(v);

            DataManager.Instance.musicData.Music = v;
        });
        togSound.onValueChanged.AddListener((v) =>
        {

            DataManager.Instance.musicData.Sound = v;
        });
        sliderMusic.onValueChanged.AddListener((v) =>
        {
            BKMusic.Instance.ChangeMusic(v);

            DataManager.Instance.musicData.MusicValue = v;
        });
        sliderSound.onValueChanged.AddListener((v) =>
        {
    
            DataManager.Instance.musicData.SoundValue = v;
        });
    }
}
