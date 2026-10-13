using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    public Vector2 Velocity =  Vector2.zero;
    private ContactFilter2D contactFilter2D;
    private RaycastHit2D[] hits = new RaycastHit2D[8];

    public float Gravity        = 1.0f;
    public float initJumpHeight = 1.0f;
    public float JumpHoldTimer  = 3.0f;
    public float JumpAmount     = 1.0f;
    private float jumpHoldTime  = 0.0f;

    private Rigidbody2D rb2;
    private bool onFloor = false;

    private bool jumping;
	private bool wasJumping;

    private void Awake() {
        rb2 = GetComponent<Rigidbody2D>();
        rb2.isKinematic = true;

        contactFilter2D = new ContactFilter2D();
        contactFilter2D.SetLayerMask(Physics2D.DefaultRaycastLayers);
        contactFilter2D.useTriggers = false;
    }

    private void collide(Vector2 movement) {
        int hitCount = rb2.Cast(
                movement.normalized, contactFilter2D, hits, movement.magnitude
                ); 

        RaycastHit2D hit;
        for (int i = 0; i < hitCount; ++i) {
            hit = hits[i];
            if (Vector2.Dot(movement, hit.normal) < 0.0f) {
                float distance = Mathf.Max(0, hit.distance - 0.001f);
                movement = movement.normalized * distance;

                float intoSurface = Vector2.Dot(Velocity, hit.normal);
                if (intoSurface < 0) {
                    Velocity -= hit.normal * intoSurface;
                }
            }
        }
    }

    private void FixedUpdate() {
        float dt = Time.fixedDeltaTime;

        Velocity.y -= Gravity * dt;

        Vector2 movement = Velocity * dt;
        collide(movement);

        rb2.MovePosition(rb2.position + movement);
    }
    
    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space) && onFloor) {
			wasJumping = jumping;
			jumping = true;
			// TODO: Add initial jump burst
		} 
		if (Input.GetKeyUp(KeyCode.Space)) {
			wasJumping = jumping;
			jumping = false;
		}

		if (jumping) {
			// TODO: add jump vel
		}
		
		if (!jumping && wasJumping) {
			// TODO add the jump hieght amount to the player
		}
    }
}
