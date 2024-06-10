using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSlider: Slider
{
    public AudioMixer mixer;
    public string parameter;

    protected override void Start() {
        base.Start();
        #if UNITY_EDITOR
        if (Application.isPlaying) 
        #endif
        Init();
    }

    protected void Init() {
        onValueChanged.AddListener(SetVolume);
        value = PlayerPrefs.GetFloat(parameter, value);
    }

    protected void SetVolume(float volume) {
        mixer.SetFloat(parameter, Mathf.Log(Mathf.Max(volume, 0.01f))*20);
        PlayerPrefs.SetFloat(parameter, volume);
    }
}