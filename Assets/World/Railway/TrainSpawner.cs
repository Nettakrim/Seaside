using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainSpawner : MonoBehaviour
{
    private bool spawnReady = false;

    [SerializeField] private int minutes = 10;

    public TrainType[] trainTypes;

    public TrackData[] tracks;

    public float blockUntil;

    void Update() {
        if (PhotoManager.instance != null && PhotoManager.instance.isComplete && Input.GetKeyDown(KeyCode.T)) {
            SpawnRandomTrain();
        }

        DateTime time = DateTime.Now;
        if (time.Minute%minutes == 0) {
            if (spawnReady) {
                spawnReady = false;
                SpawnRandomTrain();
            }
        } else {
            spawnReady = true;
        }
    }

    void SpawnRandomTrain() {
        if (Time.time < blockUntil) return;

        TrackData trackData = tracks[UnityEngine.Random.Range(0, tracks.Length)];
        TrainType trainType = trainTypes[UnityEngine.Random.Range(0, trainTypes.Length)];
        
        float length = trackData.SpawnTrain(trainType);

        // prevent any trains from spawning inside of existing trains
        // despite not tracking any trains once theyre spawned, this is still easy to do
        // the trains move at a predictable speed and are only so long, so (length / m/s) = time to move one length
        blockUntil = Time.time + length/trackData.movementSpeed;
    }
}
