using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public float movementSpeed = 5f;
    public float rotationSpeed = 200f;
    public float gravity = 1f;
    public float jump = 100f;

    private float downVelocity = 1f;
    private int airTime = 0;

    private CharacterController characterController;

    public AnimationCurve windCurve;
    public float windDistance;
    public float windForce;

    public float oceanHeight;
    public float oceanForce;
    public float oceanDamp;
    public float oceanWalkSpeedMultiplier = 0.75f;

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
            if (transform.position.y > oceanHeight) {
                downVelocity += gravity*Time.deltaTime;
                airTime++;     
            } else {
                if (downVelocity > 0) downVelocity /= 1+(Time.deltaTime*oceanDamp);
                downVelocity -= (oceanHeight-transform.position.y)*oceanForce*Time.deltaTime;
                airTime = 0;
            } 
        } else {
            downVelocity = 0;
            airTime = 0;
        }

        if (Input.GetKeyDown(KeyCode.Space) && airTime < 10) {
            downVelocity -= jump;
            airTime = 10;
        }

        // Player rotation
        float rotation = Input.GetAxis("Turn") * rotationSpeed * Time.deltaTime;
        float pitch = Input.GetAxis("Pitch") * rotationSpeed * Time.deltaTime;
        transform.Rotate(pitch, rotation, 0f);
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
    }

    public Vector3 GetWalkingForce() {
        float moveVertical = Input.GetAxis("Vertical");
        Vector3 forward = transform.forward;
        forward.y = 0;
        Vector3 movement = forward * moveVertical;

        movement += transform.right * Input.GetAxis("Horizontal");

        if (transform.position.y < oceanHeight) movement *= oceanWalkSpeedMultiplier;

        return movement;
    }

    public Vector3 GetInwardsWindForce() {
        Vector3 force = Vector3.zero;

        if (transform.position.x < -500+windDistance) {
            force += new Vector3(1 - windCurve.Evaluate((transform.position.x+500) / windDistance), 0f, 0f);
        }
        if (transform.position.x > 500-windDistance) {
            force += new Vector3(-windCurve.Evaluate((transform.position.x-500 + windDistance) / windDistance), 0f, 0f);
        }
        
        if (transform.position.z < -500+windDistance) {
            force += new Vector3(0f, 0f, 1 - windCurve.Evaluate((transform.position.z+500) / windDistance));
        }
        if (transform.position.z > 500-windDistance) {
            force += new Vector3(0f, 0f, -windCurve.Evaluate((transform.position.z-500 + windDistance) / windDistance));
        }
        
        return force;
    }
}


