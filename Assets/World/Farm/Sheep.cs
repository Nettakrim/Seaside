using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sheep : MonoBehaviour
{
    protected Rigidbody rb;

    [SerializeField] protected float walkForce;
    [SerializeField] protected float turnForce;

    protected void Start() {
        rb = GetComponent<Rigidbody>();
    }

    protected void Update()
    {
        Vector3 v = transform.rotation.eulerAngles;
        v.x = 0;
        transform.rotation = Quaternion.Euler(v);

        rb.AddForce(transform.forward*walkForce);
        rb.AddTorque(new Vector3(0, turnForce, 0));
    }
}
