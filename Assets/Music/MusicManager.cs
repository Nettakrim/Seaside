using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private Transform player;

    [SerializeField] private MusicSource[] musicAreas;

    private List<MusicTrack> musicTracks;
    [SerializeField] private float volumeChangeSpeed = 1;

    [SerializeField] private AudioSource audioSourcePrefab;

    [SerializeField] private float scheduleBuffer;

    private void Start() {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        InitialiseTracks();
    }

    private void InitialiseTracks() {
        musicTracks = new List<MusicTrack>();
        double playAt = Time.timeAsDouble+scheduleBuffer;

        // create a music track for all unique layers - if multpile areas use the same musicdata the system just uses whichever reports a higher volume
        foreach (MusicSource musicArea in musicAreas) {
            foreach (MusicTrack existing in musicTracks) {
                if (existing.musicData == musicArea.musicData) {
                    musicArea.SetIndex(musicTracks.IndexOf(existing));
                    continue;
                }
            }

            AudioSource audioSource = Instantiate(audioSourcePrefab, transform);

            MusicTrack musicTrack = new MusicTrack(musicArea.musicData, audioSource);
            musicTracks.Add(musicTrack);

            audioSource.PlayScheduled(playAt);
            musicArea.SetIndex(musicTracks.Count-1);
        }

        CalculateTracks();
        UpdateTracks(0);
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
        // if the tracks go out of sync (they might, currently untested) they can be synced here
        foreach (MusicTrack musicTrack in musicTracks) {
            musicTrack.Reset();
        }

        Vector3 listenerPosition = player.position;

        foreach (MusicSource musicArea in musicAreas) {
            musicTracks[musicArea.GetIndex()].CalculateMusicArea(musicArea, listenerPosition);
        }
    }
}
