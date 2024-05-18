using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sheep : MonoBehaviour
{
    protected Rigidbody rb;

    [SerializeField] protected float walkForce;
    [SerializeField] protected float turnForce;

    protected float pitch;

    protected Transform player;

    protected float targetX;
    protected float targetZ;

    protected Vector3 targetOffset;

    [SerializeField] protected float deadzone;
    [SerializeField] protected float slowdownDistance = 1;

    [SerializeField] protected float minWanderTime;
    [SerializeField] protected float maxWanderTime;
    [SerializeField] protected float maxWanderDistance;
    protected float nextTargetAt;

    [SerializeField] protected float maxSpeed;

    [SerializeField] protected float scareRadius;
    [SerializeField] protected float scareWalkDistance;

    protected void Start() {
        rb = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        targetX = transform.position.x;
        targetZ = transform.position.z;

        nextTargetAt = Random.value*minWanderTime;
    }

    protected void Update() {

    }

    protected void FixedUpdate() {
        CalculatePitch();
        transform.rotation = Quaternion.Euler(pitch, transform.rotation.eulerAngles.y, 0);

        UpdateTarget();

        MoveTowardsTarget();
    }

    protected void MoveTowardsTarget() {
        float distance = targetOffset.magnitude;
        if (distance < deadzone) return;

        float angle = Vector3.SignedAngle(transform.forward, targetOffset, Vector3.up)/360;

        float slowdown = Mathf.Clamp01((distance-deadzone)/slowdownDistance);

        rb.AddForce(transform.forward*walkForce*slowdown);
        rb.AddTorque(new Vector3(0, ((angle*angle*4)*turnForce*slowdown)/Mathf.Max(rb.angularVelocity.magnitude,1), 0));

        float magnitude = rb.velocity.magnitude;
        if (magnitude > maxSpeed) {
            rb.velocity = (rb.velocity/magnitude)*maxSpeed;
        }
    }

    protected void CalculatePitch() {
        Vector3 forwards = transform.forward;
        forwards.y = 0;
        forwards.Normalize();
        forwards *= 0.5f;

        Physics.Raycast(transform.position+forwards, Vector3.down, out RaycastHit front, 10);
        Physics.Raycast(transform.position-forwards, Vector3.down, out RaycastHit back, 10);
        Vector3 o = back.point-front.point;
        pitch = Mathf.Atan2(o.y, Mathf.Sqrt(o.x*o.x + o.z*o.z))*Mathf.Rad2Deg;
    }

    protected void UpdateTarget() {
        Vector2 offset = Vector2.zero;

        if (Vector3.Distance(transform.position, player.position) < scareRadius) {
            offset = new Vector2(transform.position.x - player.position.x, transform.position.z - player.position.z) * scareWalkDistance;
        } else if (Time.time > nextTargetAt || (rb.velocity.magnitude < 0.1f && targetOffset.magnitude > deadzone+slowdownDistance/2)) {
            offset = Random.insideUnitCircle * maxWanderDistance;
        }

        if (offset.magnitude > deadzone) {
            nextTargetAt = Time.time + Random.Range(minWanderTime, maxWanderTime);
            targetX = transform.position.x + offset.x;
            targetZ = transform.position.z + offset.y;
        }


        targetOffset = new Vector3(targetX, transform.position.y, targetZ) - transform.position;
    }

    protected void NewWanderTarget() {
        Vector2 offset = Random.insideUnitCircle * maxWanderDistance;
        targetX = transform.position.x + offset.x;
        targetZ = transform.position.z + offset.y;
    }

    protected void NewScareTarget() {
        Vector2 offset = (transform.position - player.position) * maxWanderDistance;
        targetX = transform.position.x + offset.x;
        targetZ = transform.position.z + offset.y;
    }
}
