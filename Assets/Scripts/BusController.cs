using UnityEngine;
using UnityEngine.InputSystem;

public class BusController : MonoBehaviour
{
    [Header("Bus Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 150f;
    public float driftFactor = 0.95f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Transform cameraTransform;  // <-- new

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cameraTransform = GetComponentInChildren<Camera>().transform;  // <-- new
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
        ApplySteering();
        ApplyDrift();
    }
    private void LateUpdate()
    {
        if (cameraTransform != null)
            cameraTransform.rotation = Quaternion.identity;
    }

    private  void ApplyMovement()
    {
        Vector2 force = transform.up * moveInput.y * moveSpeed;
        rb.AddForce(force);
        rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, moveSpeed);
    }

    private void ApplySteering()
    {
        var speed = rb.linearVelocity.magnitude;
        if (speed > 0.1f)
        {
            var direction = moveInput.y >= 0 ? 1f : -1f;
            var rotation = -moveInput.x * rotationSpeed * Time.fixedDeltaTime * direction;
            rb.MoveRotation(rb.rotation + rotation);
        }
    }

    private void ApplyDrift()
    {
        Vector2 forwardVelocity = transform.up * Vector2.Dot(rb.linearVelocity, transform.up);
        Vector2 rightVelocity = transform.right * Vector2.Dot(rb.linearVelocity, transform.right);
        rb.linearVelocity = forwardVelocity + rightVelocity * driftFactor;
    }
}