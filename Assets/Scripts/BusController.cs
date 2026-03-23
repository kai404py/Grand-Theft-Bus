using UnityEngine;
using UnityEngine.InputSystem;

public class BusController : MonoBehaviour
{
    [Header("Speed Settings")]
    public float topSpeed = 10f;
    public float accelerationRate = 4f;
    public float brakingRate = 8f;
    public float naturalDeceleration = 3f;

    [Header("Steering Settings")]
    public float maxSteerAngle = 120f;        // Max rotation speed (degrees/sec)
    public float steerSpeedInfluence = 0.6f;  // How much speed reduces steering (0-1)
    public float minSteerSpeed = 0.5f;        // Minimum speed needed to steer
    public float driftFactor = 0.95f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Transform cameraTransform;
    private float currentSpeed = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cameraTransform = GetComponentInChildren<Camera>().transform;
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    private void FixedUpdate()
    {
        ApplyAcceleration();
        ApplySteering();
        ApplyDrift();
    }

    private void LateUpdate()
    {
        if (cameraTransform != null)
            cameraTransform.rotation = Quaternion.identity;
    }

    private void ApplyAcceleration()
    {
        float targetSpeed = moveInput.y * topSpeed;
        float speedDiff = targetSpeed - currentSpeed;

        // Choose rate depending on whether accelerating, braking, or coasting
        float rate;
        if (Mathf.Abs(moveInput.y) < 0.01f)
        {
            rate = naturalDeceleration;  // No input — coast to a stop
        }
        else if (Mathf.Abs(targetSpeed) < Mathf.Abs(currentSpeed) && Mathf.Sign(targetSpeed) == Mathf.Sign(currentSpeed))
        {
            rate = brakingRate;          // Reducing speed in same direction — braking
        }
        else
        {
            rate = accelerationRate;     // Accelerating
        }

        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, rate * Time.fixedDeltaTime);
        rb.linearVelocity = transform.up * currentSpeed;
    }

    private void ApplySteering()
    {
        float speed = Mathf.Abs(currentSpeed);
        if (speed < minSteerSpeed) return;

        // Steering tightens at low speed, widens at high speed
        float speedFactor = 1f - (speed / topSpeed) * steerSpeedInfluence;
        float steerAmount = -moveInput.x * maxSteerAngle * speedFactor * Time.fixedDeltaTime;

        // Flip steering when reversing
        float direction = currentSpeed >= 0 ? 1f : -1f;
        rb.MoveRotation(rb.rotation + steerAmount * direction);
    }

    private void ApplyDrift()
    {
        Vector2 forwardVelocity = transform.up * Vector2.Dot(rb.linearVelocity, transform.up);
        Vector2 rightVelocity = transform.right * Vector2.Dot(rb.linearVelocity, transform.right);
        rb.linearVelocity = forwardVelocity + rightVelocity * driftFactor;
    }
}