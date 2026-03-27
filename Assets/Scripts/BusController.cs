using UnityEngine;
using UnityEngine.InputSystem;

public class BusController : MonoBehaviour
{
    [Header("Speed Settings")]
    public float topSpeed = 60f;
    public float accelerationRate = 2f;
    public float brakingRate = 8f;
    public float naturalDeceleration = 2f;

    [Header("Steering Settings")]
    public float maxSteerAngle = 60f;
    public float steerSpeedInfluence = 0.9f;
    public float minSteerSpeed = 0.1f;
    public float driftFactor = 0.95f;
	public float activeDriftFactor = 0.1f;
	public float driftSteerMultiplier = 10.0f;

	private float currentDriftFactor;
	private bool isDrifting = false;
    
	// Add this later broken ATM
    //public float steerReturnSpeed = 90f;

    [Header("Collision Settings")]
    public float crashSpeedThreshold = 2f;
    public float crashSpeedLossFactor = 0.8f;
    private AudioSource audioSource;
    public AudioClip crashSound;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Transform cameraTransform;
    private float currentSpeed = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cameraTransform = GetComponentInChildren<Camera>().transform;
        audioSource = GetComponent<AudioSource>();
		currentDriftFactor = driftFactor;
    }

    void OnDrift(InputValue value)
	{
    	isDrifting = value.isPressed;
    	currentDriftFactor = isDrifting ? activeDriftFactor : driftFactor;
	}

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    private void FixedUpdate()
    {
		ApplyDrift(currentDriftFactor);
        ApplyAcceleration();
        ApplySteering();
    }

    private void LateUpdate()
    {
        if (cameraTransform != null)
            cameraTransform.rotation = Quaternion.identity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        audioSource.PlayOneShot(crashSound);
        float impactForce = collision.relativeVelocity.magnitude;

        if (impactForce > crashSpeedThreshold)
        {
            currentSpeed *= (1f - crashSpeedLossFactor);
            rb.linearVelocity = transform.up * currentSpeed;
        }
    }

    private void ApplyAcceleration()
    {
        float targetSpeed = moveInput.y * topSpeed;

        float rate;
        if (Mathf.Abs(moveInput.y) < 0.01f)
        {
            rate = naturalDeceleration;
        }
        else if (Mathf.Abs(targetSpeed) < Mathf.Abs(currentSpeed) && Mathf.Sign(targetSpeed) == Mathf.Sign(currentSpeed))
        {
            rate = brakingRate;
        }
        else
        {
            rate = accelerationRate;
        }

        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, rate * Time.fixedDeltaTime);
        
		Vector2 forwardVelocity = transform.up * currentSpeed;
    	Vector2 lateralVelocity = transform.right * Vector2.Dot(rb.linearVelocity, transform.right);
    	rb.linearVelocity = forwardVelocity + lateralVelocity;
    }

    private void ApplySteering()
    {
        float speed = Mathf.Abs(currentSpeed);
        if (speed < minSteerSpeed) return;
    
        float speedFactor = 1f - (speed / topSpeed) * steerSpeedInfluence;
        float steerMult = isDrifting ? driftSteerMultiplier : 1f;
        float steerAmount = -moveInput.x * maxSteerAngle * speedFactor * steerMult * Time.fixedDeltaTime;
    
        float direction = currentSpeed >= 0 ? 1f : -1f;
        rb.MoveRotation(rb.rotation + steerAmount * direction);
    }

    private void ApplyDrift(float driftFactor)
    {
        Vector2 forwardVelocity = transform.up * Vector2.Dot(rb.linearVelocity, transform.up);
        Vector2 rightVelocity = transform.right * Vector2.Dot(rb.linearVelocity, transform.right);
        rb.linearVelocity = forwardVelocity + rightVelocity * driftFactor;
    }
}