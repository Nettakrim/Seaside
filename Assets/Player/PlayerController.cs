using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float rotationSpeed = 200f;
    [SerializeField] private float gravity = 1f;
    [SerializeField] private float jump = 100f;

    private float downVelocity = 1f;
    private int airTime = 0;
    [SerializeField] private int coyoteTime = 0;

    private CharacterController characterController;

    [SerializeField] private AnimationCurve windCurve;
    [SerializeField] private float windDistance;
    [SerializeField] private float windForce;

    [SerializeField] private float oceanHeight;
    [SerializeField] private float oceanForce;
    [SerializeField] private float oceanDamp;
    [SerializeField] private float oceanWalkSpeedMultiplier = 0.75f;

    [SerializeField] private Transform cameras;
    private float pitch;
    private float yaw;

    private bool jumpBuffered;

    private bool movementLocked;

    private float rotationSpeedScale = 1;

    private float ridingSpeed;
    private Transform ridingTransform;
    [SerializeField] private float rideTime;
    private float ridingAt;
    
    [SerializeField] private float floorDistance;

    private void Awake() {
        characterController = GetComponent<CharacterController>();
        ridingAt = -rideTime;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            SceneManager.LoadScene("MainMenu");
        }

        Vector3 movement = Vector3.zero;
        
        if (!movementLocked) {
            movement += GetWalkingForce() * movementSpeed;
        }

        movement += Vector3.down * downVelocity;

        movement += GetInwardsWindForce() * windForce;

        if (Physics.Raycast(new Ray(transform.position, Vector3.down), out RaycastHit hit, floorDistance, int.MaxValue, QueryTriggerInteraction.Ignore)) {
            if (hit.transform == ridingTransform) {
                ridingAt = Time.time;
            } else if (hit.transform.CompareTag("Train")) {
                ridingTransform = hit.transform;
                ridingAt = Time.time;
                ridingSpeed = 15;
            }
        }

        float t = (Time.time - ridingAt)/rideTime;
        if (ridingTransform != null && t < 1) {
            movement += ridingTransform.forward * ridingSpeed * (1-t);
        }

        movement *= Time.deltaTime;

        characterController.Move(movement);
        
        if (!characterController.isGrounded) {
            if (transform.localPosition.y > oceanHeight) {
                downVelocity += gravity*Time.deltaTime;
                airTime++;     
            } else {
                if (downVelocity > 0) downVelocity /= 1+(Time.deltaTime*oceanDamp);
                downVelocity -= (oceanHeight-transform.localPosition.y)*oceanForce*Time.deltaTime;
                airTime = 0;
            } 
        } else {
            downVelocity = 0;
            airTime = 0;
        }

        if (Input.GetKeyDown(KeyCode.Space) && (downVelocity >= -0.1 || airTime == 0) && !movementLocked) {
            jumpBuffered |= true;
        }

        if (jumpBuffered && airTime < coyoteTime) {
            downVelocity -= jump;
            airTime = coyoteTime;
            jumpBuffered = false;
        }

        // Player rotation - rotate the camera instead of the player themselves
        pitch += -Input.GetAxis("Mouse Y") * rotationSpeed * rotationSpeedScale * Time.deltaTime;
        yaw += Input.GetAxis("Mouse X") * rotationSpeed * rotationSpeedScale * Time.deltaTime;
        cameras.rotation = Quaternion.Euler(pitch, yaw, 0);
    }

    [SerializeField] private Vector3 GetWalkingForce() {
        float r = yaw*Mathf.Deg2Rad;
        Vector3 forward = new Vector3(Mathf.Sin(r), 0, Mathf.Cos(r));

        Vector3 movement = (forward*Input.GetAxis("Vertical")) + (new Vector3(forward.z, 0, -forward.x)*Input.GetAxis("Horizontal"));
        if (movement.magnitude > 1) movement.Normalize();

        if (transform.localPosition.y < oceanHeight) movement *= oceanWalkSpeedMultiplier;

        return movement;
    }

    [SerializeField] private Vector3 GetInwardsWindForce() {
        Vector3 force = Vector3.zero;

        if (transform.localPosition.x < -500+windDistance) {
            force += new Vector3(1 - windCurve.Evaluate((transform.localPosition.x+500) / windDistance), 0f, 0f);
        }
        if (transform.localPosition.x > 500-windDistance) {
            force += new Vector3(-windCurve.Evaluate((transform.localPosition.x-500 + windDistance) / windDistance), 0f, 0f);
        }
        
        if (transform.localPosition.z < -500+windDistance) {
            force += new Vector3(0f, 0f, 1 - windCurve.Evaluate((transform.localPosition.z+500) / windDistance));
        }
        if (transform.localPosition.z > 500-windDistance) {
            force += new Vector3(0f, 0f, -windCurve.Evaluate((transform.localPosition.z-500 + windDistance) / windDistance));
        }
        
        return force;
    }

    public Vector3 GetPosition() {
        return transform.localPosition;
    }

    public Vector2 GetRotation() {
        return new Vector2(pitch, yaw);
    }

    public void SetPositionAndRotation(Vector3 position, Vector2 rotation) {
        SetLocalPosition(position);
        pitch = rotation.x;
        yaw = rotation.y;
    }

    private void SetLocalPosition(Vector3 position) {
        //for some reason character controllers prevent position being set when enabled
        characterController.enabled = false;
        transform.localPosition = position;
        characterController.enabled = true;
    }

    public void TeleportToDefaultSpawnPosition() {
        GameObject[] spawns = GameObject.FindGameObjectsWithTag("PlayerSpawn");
        if (spawns.Length == 0) {
            Debug.LogWarning("No objects with the PlayerSpawn tag found");
            return;
        }
        Transform spawn = spawns[Random.Range(0, spawns.Length)].transform;
        SetPositionAndRotation(spawn.position, new Vector2(spawn.rotation.eulerAngles.x, spawn.rotation.eulerAngles.y));
    }

    public void SetMovementLock(bool locked) {
        movementLocked = locked;
    }

    public void SetRotationSpeed(float speed) {
        rotationSpeedScale = speed;
    }
}
