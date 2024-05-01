using UnityEngine;

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

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        Vector3 movement = GetWalkingForce() * movementSpeed;

        movement += Vector3.down * downVelocity;

        movement += GetInwardsWindForce() * windForce;

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

        if (Input.GetKeyDown(KeyCode.Space) && downVelocity >= -0.1) {
            jumpBuffered |= true;
        }

        if (jumpBuffered && airTime < coyoteTime) {
            downVelocity -= jump;
            airTime = coyoteTime;
            jumpBuffered = false;
        }

        // Player rotation - rotate the camera instead of the player themselves
        pitch += Input.GetAxis("Pitch") * rotationSpeed * Time.deltaTime;
        yaw += Input.GetAxis("Turn") * rotationSpeed * Time.deltaTime;
        cameras.rotation = Quaternion.Euler(pitch, yaw, 0);
    }

    [SerializeField] private Vector3 GetWalkingForce() {
        float r = yaw*Mathf.Deg2Rad;
        Vector3 forward = new Vector3(Mathf.Sin(r), 0, Mathf.Cos(r));

        Vector3 movement = (forward*Input.GetAxis("Vertical")) + (new Vector3(forward.z, 0, -forward.x)*Input.GetAxis("Horizontal"));

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
}


