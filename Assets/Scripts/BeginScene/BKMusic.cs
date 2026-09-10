using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BKMusic : MonoBehaviour
{
    private static BKMusic instance;
    public static BKMusic Instance=>instance;
    public AudioSource bkSource;
     void Awake()
    {
        instance = this;

        bkSource = GetComponent<AudioSource>();

        MusicOpen(DataManager.Instance.musicData.Music);
        ChangeMusic(DataManager.Instance.musicData.MusicValue);

    }

    public void MusicOpen(bool isopen)
    {
        bkSource.mute = !isopen;
    }

    public void ChangeMusic(float data)
    {
        bkSource.volume = data;
    }
}
