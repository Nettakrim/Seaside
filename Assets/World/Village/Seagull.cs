using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seagull : MonoBehaviour
{
    [SerializeField] protected float distance;
    [SerializeField] protected float speed;
    [SerializeField] protected Transform bird;

    void Update() {
        bird.localPosition = new Vector3(0, 0, -distance);
        transform.rotation = Quaternion.Euler(0, ((Time.time/(speed*distance))%1)*360, 0);
    }
}
