using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainElement : MonoBehaviour
{
    [NonSerialized] public TrainSpawner trainSpawner;

    private int index;
    private Transform next;

    [SerializeField] private Renderer randomHue;
    [SerializeField] private Animator wheelAnimator;

    void Start() {
        float hue = UnityEngine.Random.value;
        foreach (Material material in randomHue.materials) {
            material.SetFloat("_HueRotation", hue);
        }
        next = trainSpawner.tracks[0];
        wheelAnimator.SetFloat("Speed", trainSpawner.movementSpeed);
    }

    void Update() {
        if (Vector3.Distance(transform.position, next.position) < 1) {
            index++;
            if (index >= trainSpawner.tracks.Length) {
                Destroy(gameObject);
                return;
            }
            next = trainSpawner.tracks[index];
        }

        transform.position = Vector3.MoveTowards(transform.position, next.position, trainSpawner.movementSpeed*Time.deltaTime);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(transform.position-next.position), trainSpawner.rotationSpeed*Time.deltaTime);
    }
}
