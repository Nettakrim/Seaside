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

    private float trainSpawnedAt;
    private float blockDuration;
    private float trainDuration;

    void Update() {
        if (PhotoManager.instance.isComplete && InputManager.instance.train.GetDown()) {
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
        if (Time.time < trainSpawnedAt + blockDuration) return;

        TrackData trackData = tracks[UnityEngine.Random.Range(0, tracks.Length)];
        TrainType trainType = trainTypes[UnityEngine.Random.Range(0, trainTypes.Length)];
        
        float length = trackData.SpawnTrain(trainType);

        // prevent any trains from spawning inside of existing trains
        // despite not tracking any trains once theyre spawned, this is still easy to do
        // the trains move at a predictable speed and are only so long, so (length / m/s) = time to move one length
        trainSpawnedAt = Time.time;
        blockDuration = length/trackData.movementSpeed;
        trainDuration = trackData.GetLength()/trackData.movementSpeed;
    }

    public float ProgressToTrainSpawn() {
        if (Time.time < trainSpawnedAt + blockDuration + trainDuration) {
            return 1;
        }
        DateTime date = DateTime.Now;
        int roundedMinutes = ((date.Minute+minutes)/minutes)*minutes;
        DateTime target = new DateTime(date.Year, date.Month, date.Day, date.Hour + roundedMinutes/60, roundedMinutes%60, 0);
        float time = (target.Ticks - date.Ticks)/(float)TimeSpan.TicksPerSecond;
        return 1-(time/(minutes*60-(trainDuration+blockDuration)));
    }
}
