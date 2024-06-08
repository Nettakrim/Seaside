using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float rotationSpeed = 200f;
    [SerializeField] private float controllerRotationScale;
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
    private Vector3 ridingDirection;
    [SerializeField] private float rideTime;
    private float ridingAt;
    
    [SerializeField] private float floorDistance;

    private bool canFly;
    private bool flying;
    private Vector3 flyingVelocity;
    [SerializeField] private float flyingAcceleration;
    [SerializeField] private float topFlyingSpeed;
    [SerializeField] private float flyingDecay;

    [SerializeField] private LayerMask terrain;

    [SerializeField] private CapsuleCollider playerCollider;
    [SerializeField] private float maxSnapDropDistance;

    private void Awake() {
        characterController = GetComponent<CharacterController>();
        ridingAt = -rideTime;
    }

    private void Update() {
        if (!SceneLoader.instance.IsLoadingScene() && InputManager.instance.pause.GetDown()) {
            SceneLoader.instance.LoadScene("MainMenu", LoadSceneMode.Single);
        }

        Vector3 movement;
        bool jumpKeyPressed = InputManager.instance.jump.GetDown();

        if (flying) {
            movement = CalculateFlyingForce();

            if ((jumpKeyPressed || !canFly) && !movementLocked) {
                if (Physics.Raycast(new Ray(transform.position, Vector3.down), transform.position.y, terrain)) {
                    SetFlying(false, movement);
                }
            }
        } else {
            movement = CalculateHorizontalForce();
            movement += CalculateVerticalForce(jumpKeyPressed);

            if (canFly && jumpKeyPressed && airTime > coyoteTime && !movementLocked) {
                SetFlying(true, movement);
            }
        }

        movement *= Time.deltaTime;

        if (!flying) {
            movement += CalculateFloorSnapOffset();
        }

        characterController.Move(movement);

        // player rotation - rotate the camera instead of the player themselves
        float rotation = rotationSpeed * rotationSpeedScale;
        pitch += -InputManager.instance.turnY.value * rotation;
        yaw   +=  InputManager.instance.turnX.value * rotation;

        rotation *= Time.deltaTime * controllerRotationScale;
        pitch += InputManager.instance.turnYController.value * rotation;
        yaw   += InputManager.instance.turnXController.value * rotation;

        cameras.rotation = Quaternion.Euler(pitch, yaw, 0);
    }

    public void SetFlying(bool to, Vector3 movement) {
        if (flying == to || !canFly) {
            return;
        }
        flying = to;

        if (flying) {
            AreaData.instance.flyingMusic.SetActive(true);
            flyingVelocity = new Vector3(movement.x, -downVelocity, movement.z);
            jumpBuffered = false;
        } else {
            AreaData.instance.flyingMusic.SetActive(false);
            // transfer velocity, default movement doesnt track this, so it instead uses the riding mechanism
            ridingAt = Time.time;
            Vector3 horizontalFlying = new Vector3(flyingVelocity.x, 0, flyingVelocity.z);
            ridingSpeed = horizontalFlying.magnitude;
            ridingDirection = horizontalFlying.normalized;
            downVelocity = -flyingVelocity.y;
        }

        foreach (GameObject disable in AreaData.instance.disableWhileFlying) {
            disable.SetActive(!flying);
        }
    }

    private Vector3 CalculateHorizontalForce() {
        Vector3 movement = Vector3.zero;

        if (!movementLocked) {
            movement += GetWalkingForce() * movementSpeed;
        }

        movement += GetInwardsWindForce() * windForce;

        // this system is very specific to trains
        if (Physics.Raycast(new Ray(transform.localPosition, Vector3.down), out RaycastHit hit, floorDistance, int.MaxValue, QueryTriggerInteraction.Ignore)) {
            if (hit.transform == ridingTransform) {
                ridingAt = Time.time;
                ridingDirection = ridingTransform.forward;
            } else if (hit.transform.CompareTag("Train")) {
                ridingTransform = hit.transform;
                ridingDirection = ridingTransform.forward;
                ridingAt = Time.time;
                ridingSpeed = 15;
            }
        }

        float t = (Time.time - ridingAt)/rideTime;
        if (t < 1) {
            movement += ridingDirection * ridingSpeed * (1-t);
        }

        return movement;
    }

    private Vector3 CalculateVerticalForce(bool jumpKeyPressed) {
        if (!characterController.isGrounded) {
            //fall when above water, float up when below it
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

        if (jumpKeyPressed && (downVelocity >= -0.1 || airTime == 0) && !movementLocked) {
            jumpBuffered |= true;
        }

        if (jumpBuffered && airTime < coyoteTime) {
            downVelocity -= jump;
            airTime = coyoteTime;
            jumpBuffered = false;
        }

        return Vector3.down * downVelocity;
    }

    private Vector3 GetWalkingForce() {
        float r = yaw*Mathf.Deg2Rad;
        Vector3 forward = new Vector3(Mathf.Sin(r), 0, Mathf.Cos(r));

        Vector3 movement = (forward*InputManager.instance.moveY.value) + (new Vector3(forward.z, 0, -forward.x)*InputManager.instance.moveX.value);
        if (movement.magnitude > 1) movement.Normalize();

        if (transform.localPosition.y < oceanHeight) movement *= oceanWalkSpeedMultiplier;

        return movement;
    }

    private Vector3 GetInwardsWindForce() {
        //makes a that softly stops you walking too far and hitting the invisible wall when near the edge of the map
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
        // for some reason character controllers prevent position being set when enabled
        characterController.enabled = false;
        transform.localPosition = position;
        characterController.enabled = true;
    }

    public void TeleportToDefaultSpawnPosition() {
        int length = AreaData.instance.playerSpawns.Length;
        if (length == 0) {
            Debug.LogWarning("No objects with the PlayerSpawn tag found");
            return;
        }
        Transform spawn = AreaData.instance.playerSpawns[Random.Range(0, length)].transform;
        SetPositionAndRotation(spawn.position, new Vector2(spawn.rotation.eulerAngles.x, spawn.rotation.eulerAngles.y));
    }

    public void SetMovementLock(bool locked) {
        movementLocked = locked;
    }

    public void SetRotationSpeed(float speed) {
        rotationSpeedScale = speed;
    }

    public Vector3 CalculateFlyingForce() {
        Vector3 input = Get3DMovement();
        flyingVelocity += input * (flyingAcceleration * Time.deltaTime);

        Vector3 damped = flyingVelocity * Mathf.Exp(-flyingDecay*Time.deltaTime);
        
        float cosSimilarity = 0;
        if (input.magnitude > 0) {
            cosSimilarity = Vector3.Dot(input.normalized, flyingVelocity.normalized);
        }

        // effectively increases the dampening force when youre turning or stop moving
        flyingVelocity = Vector3.Lerp(damped, flyingVelocity, ((cosSimilarity+1)/2)*input.magnitude);

        float magnitude = flyingVelocity.magnitude;
        if (magnitude > topFlyingSpeed) {
            flyingVelocity *= topFlyingSpeed/magnitude;
        }

        Vector3 movement = flyingVelocity;
        
        return movement;
    }

    public Vector3 Get3DMovement() {
        Vector3 movement = Vector3.zero;

        if (!movementLocked) {
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

            Vector2 input = new Vector2(InputManager.instance.moveX.value, InputManager.instance.moveY.value);
            if (input.magnitude > 1) input.Normalize();

            movement += rotation * Vector3.forward * input.y;
            movement += rotation * Vector3.right * input.x;
        }

        return movement;
    }

    public void SetCanFly(bool to) {
        canFly = to;
    }

    public bool IsFlying() {
        return flying;
    }

    public Vector3 CalculateFloorSnapOffset() {
        // airtime check means jumping cancels snapping, riding check is because slopes on trains act buggy and this helps
        if (airTime > 2 || Time.time-ridingAt < rideTime/2) return Vector3.zero;

        float offset = playerCollider.height/2 - playerCollider.radius;
        bool floorFound = Physics.SphereCast(transform.position+new Vector3(0, -offset, 0), playerCollider.radius, Vector3.down, out RaycastHit snapHit, transform.position.y, int.MaxValue, QueryTriggerInteraction.Ignore);
        if (floorFound) {
            if (Physics.Raycast(transform.position+new Vector3(0, -playerCollider.height/2, 0), Vector3.down, maxSnapDropDistance)) {
                float newY = offset + snapHit.point.y + (snapHit.normal.y*playerCollider.radius);
                if (newY > oceanHeight) {
                    return new Vector3(0, newY - transform.position.y, 0);
                }
            }
        }

        return Vector3.zero;
    }
}
