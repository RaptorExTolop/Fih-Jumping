using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    public Vector2 Velocity =  Vector2.zero;
    public float Gravity = 1.0f;
    private ContactFilter2D contactFilter2D;
    private RaycastHit2D[] hits = new RaycastHit2D[8];

    private Rigidbody2D rb2;
    private bool onFloor = false;

    private void Awake() {
        rb2 = GetComponent<Rigidbody2D>();
        rb2.isKinematic = true;

        contactFilter2D = new ContactFilter2D();
        contactFilter2D.SetLayerMask(Physics2D.DefaultRaycastLayers);
        contactFilter2D.useTriggers = false;
    }

    private void FixedUpdate() {
        float dt = Time.fixedDeltaTime;

        Velocity.y -= Gravity * dt;

        Vector2 movement = Velocity * dt;

        int hitCount = rb2.Cast(
            movement.normalized, contactFilter2D, hits, movement.magnitude
        ); 
        foreach (RaycastHit2D hit in hits) {
            if (Vector2.Dot(movement, hit.normal) < 0) {
                float distance = Mathf.Max(0, hit.distance );
                movement = movement.normalized * distance;

                float intoSurface = Vector2.Dot(Velocity, hit.normal);
                if (intoSurface < 0) {
                    Velocity -= hit.normal * intoSurface;
                }
            }
        }
        
        rb2.MovePosition(rb2.position + Velocity * dt);
    }
}