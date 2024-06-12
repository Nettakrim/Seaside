using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RailGate : MonoBehaviour
{
    public TrainSpawner trainSpawner;
    public Animator animator;

    private float currentProgress;
    public float maxProgressSpeed;

    private void Update() {
        float progress = trainSpawner.ProgressToTrainSpawn();
        if (currentProgress == 1) {
            currentProgress = progress < 1 ? 0 : 1;
        } else {
            currentProgress = Mathf.MoveTowards(currentProgress, progress, maxProgressSpeed*Time.deltaTime);
        }
        animator.SetFloat("Progress", currentProgress);
    }
}
