using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODOs:
// [ ] Add in the pillars
// [ ] Move the pillars
// [ ] Collide with the pillars
// [ ] Die on collide
// [ ] Game state goodness
// [ ] Add in points
// [ ] Save the points

// NOTEs:
// Obstacle height should be between 4 and 10 (2, 5)

[System.Serializable]
public struct Timer {
	public bool active;
	public bool finished;
	public float length; 
	public float currentTime;

	public Timer(float seconds) {
		length = seconds;
		currentTime = 0;
		active = false;
		finished = false;
	} 

	public void tick(float dt) {
		if (!active) { return; }
		if (finished) { finished = false; }
		currentTime -= dt;
		if (currentTime < 0) {
			currentTime = 0;
			active = false;
			finished = true;
		}
	}

	public void start() {
		currentTime = length;
		active = true;
	}

	public void stop() {
		currentTime = 0;
		active = false;
	}
}

public class PlayerController : MonoBehaviour {
    public Vector2 Velocity =  Vector2.zero;
    private ContactFilter2D contactFilter2D;
    private RaycastHit2D[] hits = new RaycastHit2D[8];

	// The time the player has held the jump button down
	[SerializeField] private Timer jumpTimer      = new Timer(1.0f);
	// The timer for the jump buffering grace 
	[SerializeField] private Timer jumpGraceTimer = new Timer(0.3f);
    public float Gravity        = 1.0f;
    public float initJumpHeight = 1.0f;
    public float JumpAmount     = 1.0f;
	private float jumpAmount    = 0.0f;

    private Rigidbody2D rb2;
    private bool onFloor = false;

    private bool jumping = false;

	public GameObject    Obstacle;
	[SerializeField] private GameObject[] Obstacles;

	[SerializeField] public  int    NumObstacles                = 5;
	[SerializeField] public float   ObstacleSpawnSpeedVariation = 0.75f;
	[SerializeField] public float   ObstacleBaseSpawnSpeed	    = 3f;
	[SerializeField] private Timer  obstacleSpawnTimer 		    = new Timer(6f);
	[SerializeField] public float   ObstacleHieghtVariation     = 4f;
	[SerializeField] public Vector2 ObstacleSpawnPosition       = new Vector2(0, 0);
	[SerializeField] public float   ObstacleSpeed 			    = 4;

	private int NextObstacleLocation = 0;

    private void Awake() {
        rb2 = GetComponent<Rigidbody2D>();
        rb2.isKinematic = true;

        contactFilter2D = new ContactFilter2D();
        contactFilter2D.SetLayerMask(Physics2D.DefaultRaycastLayers);
        contactFilter2D.useTriggers = false;

        Obstacles = new GameObject[NumObstacles]; // WHY; I cannot set it at global spot :(

		jumpTimer.stop();
		obstacleSpawnTimer.stop();
    }

    private Vector2 collide(Vector2 movement) {
		onFloor = false;
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
					onFloor = true;
                }
            }
        }

		return movement;
    }

	private void update_player(float dt) {
		jumpTimer.tick(dt);

		if (Input.GetKeyDown(KeyCode.Space) && !onFloor) {
			jumpGraceTimer.start();
		} else {
			jumpGraceTimer.tick(dt);
		}

		if ((Input.GetKeyDown(KeyCode.Space) || jumpGraceTimer.active) && onFloor) {
			jumping = true;

			jumpAmount = initJumpHeight;
			jumpTimer.start();

			// TODO: Kinda shitty interface but don't know if I will have time to change it before game needs to be in
			if (jumpGraceTimer.active) {
				float heldTime = jumpGraceTimer.length - jumpGraceTimer.currentTime;
				jumpTimer.tick(heldTime);
				jumpAmount += JumpAmount * heldTime / 2;
			}
		} 

		if (Input.GetKeyUp(KeyCode.Space) && jumping) {
			jumping = false;
			if (jumping) { jumping = false; }
			jumpTimer.stop();

			Velocity.y = jumpAmount;
		}

		if (jumping && jumpTimer.active) {
			if (jumpTimer.active) {
				jumpAmount += JumpAmount * dt;
			}
		}

		Velocity.y -= Gravity * dt;
	}

    private void Update() {
		float dt = Time.deltaTime;
		update_player(dt);
		update_obstacles(dt);
    }

    private void FixedUpdate() {
        float dt = Time.fixedDeltaTime;
        Vector2 movement = Velocity * dt;
        movement = collide(movement);

        rb2.MovePosition(rb2.position + movement);
    }
	
	private void update_obstacles(float dt) {
			obstacleSpawnTimer.active = true;
		if (obstacleSpawnTimer.finished) {
			//Debug.Log("Spawning Obstacle");
			{
				Destroy(Obstacles[NextObstacleLocation]);
				Vector3 position                = new Vector3(ObstacleSpawnPosition.x, ObstacleSpawnPosition.y);
				Obstacles[NextObstacleLocation] = Instantiate(Obstacle, position, Quaternion.identity);
				NextObstacleLocation++;
				if (NextObstacleLocation >= NumObstacles) { NextObstacleLocation = 0; }
			}
			float speed        = ObstacleBaseSpawnSpeed + UnityEngine.Random.Range(0, ObstacleSpawnSpeedVariation);
			obstacleSpawnTimer = new Timer(speed);
			obstacleSpawnTimer.start();
		} else {
			obstacleSpawnTimer.tick(dt);
		}
		
		foreach (var obstacle in Obstacles) {
			if (obstacle == null) { continue; }
			Vector3 oldPos = obstacle.transform.position;
			obstacle.transform.position = new (oldPos.x + (ObstacleSpeed * dt), oldPos.y, oldPos.z);
		}
	}
}
