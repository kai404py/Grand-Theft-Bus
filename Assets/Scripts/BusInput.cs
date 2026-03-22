using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSteering : MonoBehaviour {

    Rigidbody2D rb;

    [SerializeField]
    float accelerationPower = 5f;
    [SerializeField]
    float steeringPower = 5f;
    float steeringAmount, speed, direction;

    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody2D> ();
    }
	
    // Update is called once per frame
    void FixedUpdate () {

        steeringAmount = - Input.GetAxis ("Horizontal");
        speed = Input.GetAxis ("Vertical") * accelerationPower;
        direction = Mathf.Sign(Vector2.Dot (rb.linearVelocity, rb.GetRelativeVector(Vector2.up)));
        rb.rotation += steeringAmount * steeringPower * rb.linearVelocity.magnitude * direction;

        rb.AddRelativeForce (Vector2.up * speed);

        rb.AddRelativeForce ( - Vector2.right * rb.linearVelocity.magnitude * steeringAmount / 2);
			
    }


}