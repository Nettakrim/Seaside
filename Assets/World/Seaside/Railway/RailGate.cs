using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RailGate : MonoBehaviour
{
    public TrainSpawner trainSpawner;
    public Animator animator;

    private void Update() {
        float progress = trainSpawner.ProgressToTrainSpawn();
        animator.SetFloat("Progress", progress);
    }
}
