using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sheep : MonoBehaviour
{
    protected Rigidbody rb;

    [SerializeField] protected float walkForce;
    [SerializeField] protected float turnForce;

    protected float pitch;

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

    [SerializeField] protected Animator animator;

    [SerializeField] protected float animationSpeed;

    [SerializeField] protected SheepHome sheepHome;

    protected Vector3 stuckPos;

    protected void Start() {
        rb = GetComponent<Rigidbody>();

        targetX = transform.position.x;
        targetZ = transform.position.z;

        nextTargetAt = Random.value*minWanderTime;

        transform.rotation = Quaternion.Euler(0, Random.value*360, 0);
        CalculatePitch();
    }

    protected void Update() {
        animator.SetFloat("WalkSpeed", rb.velocity.magnitude*animationSpeed);
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
        Vector3 playerPos = PhotoManager.instance.GetPlayerPosition();

        float timeDiff = Time.time - nextTargetAt;
        float speed = rb.velocity.magnitude;

        bool someTimePassed = timeDiff > -minWanderTime/2;
        bool stuck = someTimePassed && speed < 0.1f && targetOffset.magnitude > deadzone+slowdownDistance/2;
        bool farFromHome = Vector3.Distance(transform.position, sheepHome.home.position) > sheepHome.homeThreshold;
        bool scared = Vector3.Distance(transform.position, playerPos) < scareRadius;

        if ((scared || someTimePassed) && farFromHome) {
            // return home
            offset = new Vector2(sheepHome.home.position.x - transform.position.x, sheepHome.home.position.z - transform.position.z) + (Random.insideUnitCircle * sheepHome.homeThreshold/2);
        } else if (scared) {
            // run away from the player
            offset = new Vector2(transform.position.x - playerPos.x, transform.position.z - playerPos.z).normalized * scareWalkDistance;
        } else if (timeDiff > 0 || stuck || (walkForce < 0 && Vector3.Distance(stuckPos, transform.position) > maxWanderDistance)) {
            // randomly wander
            offset = Random.insideUnitCircle * maxWanderDistance;
        }

        if (offset.magnitude > deadzone) {
            if (!stuck) {
                foreach (Transform obstacle in sheepHome.obstacles) {
                    if (Vector3.Distance(obstacle.position, transform.position) < sheepHome.obstacleSize) {
                        stuck = true;
                        break;
                    }
                }
            }

            walkForce = Mathf.Abs(walkForce);
            if (stuck && someTimePassed && Random.Range(0, 5) < 4) {
                stuckPos = transform.position;
                walkForce = -walkForce;
            }

            
            nextTargetAt = Random.Range(minWanderTime, maxWanderTime);
            if (walkForce < 0) nextTargetAt /= 2;
            nextTargetAt += Time.time;
            targetX = transform.position.x + offset.x;
            targetZ = transform.position.z + offset.y;
        }


        targetOffset = new Vector3(targetX, transform.position.y, targetZ) - transform.position;
    }
}
