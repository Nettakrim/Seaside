using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public float movementSpeed = 5f;
    public float rotationSpeed = 200f;
    public float gravity = 1f;
    public float jump = 100f;

    private float yVelocity = 1f;
    private int airTime = 0;


    private CharacterController characterController;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        // Player movement
        float moveVertical = Input.GetAxis("Vertical");
        Vector3 forward = transform.forward;
        forward.y = 0;
        Vector3 movement = forward * moveVertical * movementSpeed * Time.deltaTime;
        movement += transform.right * Input.GetAxis("Horizontal") * movementSpeed * Time.deltaTime;
        movement += Vector3.down * yVelocity * Time.deltaTime;

        characterController.Move(movement);
        
        if (!characterController.isGrounded) {
            yVelocity += gravity*Time.deltaTime;
            airTime++;
        } else {
            yVelocity = 0;
            airTime = 0;
        }

        if (Input.GetKeyDown(KeyCode.Space) && airTime < 10) {
            yVelocity -= jump;
            airTime = 10;
        }



        // Player rotation
        float rotation = Input.GetAxis("Turn") * rotationSpeed * Time.deltaTime;
        float pitch = Input.GetAxis("Pitch") * rotationSpeed * Time.deltaTime;
        transform.Rotate(pitch, rotation, 0f);
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
    }
}


