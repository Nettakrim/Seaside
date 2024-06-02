using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;

    [SerializeField] private MusicData[] musicLayers;
    private List<MusicSource> musicSources;

    private List<MusicTrack> musicTracks;
    [SerializeField] private float volumeChangeSpeed = 1;

    [SerializeField] private AudioSource audioSourcePrefab;
    [SerializeField] private Transform audioSourceParent;

    [SerializeField] private float scheduleBuffer;

    private void Awake() {
        instance = this;

        InitialiseTracks();
    }

    private void InitialiseTracks() {
        musicTracks = new List<MusicTrack>();
        musicSources = new List<MusicSource>();

        //play scheduled ensures the music has had time to load
        double playAt = Time.timeAsDouble+scheduleBuffer;

        foreach (MusicData musicData in musicLayers) {
            AudioSource audioSource = Instantiate(audioSourcePrefab, audioSourceParent);
            audioSource.name = "Source: "+musicData.name;

            MusicTrack musicTrack = new MusicTrack(musicData, audioSource);
            musicTracks.Add(musicTrack);

            audioSource.PlayScheduled(playAt);
        }
    }

    public void AddSource(MusicSource musicSource) {
        bool found = false;

        foreach (MusicTrack existing in musicTracks) {
            if (existing.musicData == musicSource.musicData) {
                musicSource.SetIndex(musicTracks.IndexOf(existing));
                found = true;
                break;
            }
        }

        if (found) {
            musicSources.Add(musicSource);
        } else {
            Debug.LogWarning("Music source "+musicSource+" has layer "+musicSource.musicData+" which is not present in manager "+name);
        }
    }

    public void RemoveSource(MusicSource musicSource) {
        musicSources.Remove(musicSource);
    }

    private void Update() {
        // music is pretty slow to catch up to changes anyway so it doesnt need to be updated every frame
        if (Time.frameCount%10 == 0) {
            CalculateTracks();
        }

        float deltaDecay = Mathf.Exp(-volumeChangeSpeed*Time.deltaTime);
        UpdateTracks(deltaDecay);
    }

    private void UpdateTracks(float deltaDecay) {
        foreach (MusicTrack musicTrack in musicTracks) {
            musicTrack.Update(deltaDecay);
        }
    }

    private void CalculateTracks() {
        // as long as the tracks are all the exact same sample rate/count, they dont desync
        // if this ever changes, they should be synced here
        foreach (MusicTrack musicTrack in musicTracks) {
            musicTrack.Reset();
        }

        Vector3 listenerPosition = PhotoManager.instance.GetPlayerPosition();

        foreach (MusicSource musicSource in musicSources) {
            musicTracks[musicSource.GetIndex()].CalculateMusicSource(musicSource, listenerPosition);
        }
    }
}
