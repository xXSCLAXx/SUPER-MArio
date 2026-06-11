using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/* Mario physics reference: http://s276.photobucket.com/user/jdaster64/media/smb_playerphysics.png.html */


public class Mario : MonoBehaviour {
	private LevelManager t_LevelManager;
	private Transform m_GroundCheck1, m_GroundCheck2;
	private GameObject m_StompBox;
	private Animator m_Animator;
	private Rigidbody2D m_Rigidbody2D;
	private CircleCollider2D m_CircleCollider2D;

	public LayerMask GroundLayers;
	public GameObject Fireball;
	public Transform FirePos;
	private const float WaitBetweenFire = 0.2f;
	private float fireTime1, fireTime2;

	private float faceDirectionX;
	private float moveDirectionX;
	private float normalGravity;

	private float currentSpeedX;
	private float speedXBeforeJump;

	// Physics constants based on original SMB reference
	private const float MinWalkSpeedX      = 0.28f;
	private const float WalkAccelerationX  = 0.14f;
	private const float RunAccelerationX   = 0.21f;
	private const float ReleaseDecelerationX = 0.25f;
	private const float SkidDecelerationX  = 0.5f;
	private const float SkidTurnaroundSpeedX = 3.5f;
	private const float MaxWalkSpeedX      = 5.86f;
	private const float MaxRunSpeedX       = 9.61f;

	private float jumpSpeedY;
	private float jumpUpGravity;
	private float jumpDownGravity;
	private float midairAccelerationX;
	private float midairDecelerationX;

	private float automaticWalkSpeedX;
	private float automaticGravity;

	public float castleWalkSpeedX = 5.86f;
	public float levelEntryWalkSpeedX = 3.05f;

	private bool isGrounded;
	private bool wasGrounded;
	private bool isDashing;
	private bool isFalling;
	private bool isJumping;
	private bool isChangingDirection;
	private bool wasDashingBeforeJump;
	private bool isShooting;
	public bool isCrouching;

	private bool jumpButtonHeld;
	private bool jumpButtonReleased;

	// Coyote time: brief window after walking off a ledge where Mario can still jump
	private const float CoyoteTime = 0.1f;
	private float coyoteTimer;

	public bool inputFreezed;


	void Start () {
		t_LevelManager = FindObjectOfType<LevelManager>();
		m_GroundCheck1 = transform.Find ("Ground Check 1");
		m_GroundCheck2 = transform.Find ("Ground Check 2");
		m_StompBox = transform.Find ("Stomp Box").gameObject;
		m_Animator = GetComponent<Animator> ();
		m_Rigidbody2D = GetComponent<Rigidbody2D> ();
		m_CircleCollider2D = GetComponent<CircleCollider2D> ();
		normalGravity = m_Rigidbody2D.gravityScale;

		// Use cached LevelManager reference (avoid duplicate FindObjectOfType call)
		transform.position = t_LevelManager.FindSpawnPosition();

		UpdateSize ();

		jumpButtonReleased = true;
		fireTime1 = 0;
		fireTime2 = 0;
	}


	/****************** Movement control */
	void SetJumpParams() {
		if (currentSpeedX < 3.75f) {
			jumpSpeedY = 15f;
			jumpUpGravity = .47f;
			jumpDownGravity = 1.64f;
		} else if (currentSpeedX < 8.67f) {
			jumpSpeedY = 15f;
			jumpUpGravity = .44f;
			jumpDownGravity = 1.41f;
		} else {
			jumpSpeedY = 18.75f;
			jumpUpGravity = .59f;
			jumpDownGravity = 2.11f;
		}
	}

	void SetMidairParams() {
		if (currentSpeedX < MaxWalkSpeedX) {
			midairAccelerationX = WalkAccelerationX;
			if (speedXBeforeJump < 6.80f) {
				midairDecelerationX = WalkAccelerationX;
			} else {
				midairDecelerationX = .19f;
			}
		} else {
			midairAccelerationX = RunAccelerationX;
			midairDecelerationX = RunAccelerationX;
		}
	}


	void FixedUpdate () {
		/******** Horizontal movement on ground */
		if (isGrounded) {
			if (faceDirectionX != 0) {
				if (currentSpeedX == 0) {
					currentSpeedX = MinWalkSpeedX;
				} else if (currentSpeedX < MaxWalkSpeedX) {
					currentSpeedX = IncreaseWithinBound (currentSpeedX, WalkAccelerationX, MaxWalkSpeedX);
				} else if (isDashing && currentSpeedX < MaxRunSpeedX) {
					currentSpeedX = IncreaseWithinBound (currentSpeedX, RunAccelerationX, MaxRunSpeedX);
				}
			}

			else if (currentSpeedX > 0) {
				currentSpeedX = DecreaseWithinBound (currentSpeedX, ReleaseDecelerationX, 0);
			}

			if (isChangingDirection) {
				if (currentSpeedX > SkidTurnaroundSpeedX) {
					moveDirectionX = -faceDirectionX;
					m_Animator.SetBool ("isSkidding", true);
					currentSpeedX = DecreaseWithinBound (currentSpeedX, SkidDecelerationX, 0);
				} else {
					moveDirectionX = faceDirectionX;
					m_Animator.SetBool ("isSkidding", false);
				}
			} else {
				m_Animator.SetBool ("isSkidding", false);
			}

			if (isCrouching) {
				currentSpeedX = 0;
			}


		/******** Horizontal movement in air */
		} else {
			SetMidairParams ();

			if (faceDirectionX != 0) {
				if (currentSpeedX == 0) {
					currentSpeedX = MinWalkSpeedX;
				} else if (currentSpeedX < MaxWalkSpeedX) {
					currentSpeedX = IncreaseWithinBound (currentSpeedX, midairAccelerationX, MaxWalkSpeedX);
				} else if (wasDashingBeforeJump && currentSpeedX < MaxRunSpeedX) {
					currentSpeedX = IncreaseWithinBound (currentSpeedX, midairAccelerationX, MaxRunSpeedX);
				}
			} else if (currentSpeedX > 0) {
				currentSpeedX = DecreaseWithinBound (currentSpeedX, ReleaseDecelerationX, 0);
			}

			if (isChangingDirection) {
				faceDirectionX = moveDirectionX;
				currentSpeedX = DecreaseWithinBound (currentSpeedX, midairDecelerationX, 0);
			}
		}


		/******** Vertical movement */
		if (isGrounded) {
			isJumping = false;
			m_Rigidbody2D.gravityScale = normalGravity;
		}

		if (!isJumping) {
			// Allow jump if grounded OR within coyote time window after leaving a ledge
			bool coyoteJumpAllowed = !isGrounded && coyoteTimer > 0;
			if ((isGrounded || coyoteJumpAllowed) && jumpButtonHeld && jumpButtonReleased) {
				coyoteTimer = 0; // consume coyote window immediately
				SetJumpParams ();
				m_Rigidbody2D.velocity = new Vector2 (m_Rigidbody2D.velocity.x, jumpSpeedY);
				isJumping = true;
				jumpButtonReleased = false;
				speedXBeforeJump = currentSpeedX;
				wasDashingBeforeJump = isDashing;
				if (t_LevelManager.marioSize == 0) {
					t_LevelManager.soundSource.PlayOneShot (t_LevelManager.jumpSmallSound);
				} else {
					t_LevelManager.soundSource.PlayOneShot (t_LevelManager.jumpSuperSound);
				}
			}
		} else {
			// Lower gravity while jump button held; higher gravity when released
			if (m_Rigidbody2D.velocity.y > 0 && jumpButtonHeld) {
				m_Rigidbody2D.gravityScale = normalGravity * jumpUpGravity;
			} else {
				m_Rigidbody2D.gravityScale = normalGravity * jumpDownGravity;
			}
		}


		// Disable Stomp Box if not falling; disable Circle Collider while falling (prevents multi-hit)
		if (!isFalling) {
			m_StompBox.SetActive (false);
			m_CircleCollider2D.enabled = true;
		} else {
			m_StompBox.SetActive (true);
			m_CircleCollider2D.enabled = false;
		}


		/******** Horizontal orientation */
		if (faceDirectionX > 0) {
			transform.localScale = new Vector2 (1, 1);
		} else if (faceDirectionX < 0) {
			transform.localScale = new Vector2 (-1, 1);
		}


		/******** Override for automatic movement sequences */
		if (inputFreezed) {
			currentSpeedX = automaticWalkSpeedX;
			m_Rigidbody2D.gravityScale = automaticGravity;
		}

		/******** Shooting */
		if (isShooting && t_LevelManager.marioSize == 2) {
			fireTime2 = Time.time;

			if (fireTime2 - fireTime1 >= WaitBetweenFire) {
				m_Animator.SetTrigger ("isFiring");
				GameObject fireball = Instantiate (Fireball, FirePos.position, Quaternion.identity);
				fireball.GetComponent<MarioFireball> ().directionX = transform.localScale.x;
				t_LevelManager.soundSource.PlayOneShot (t_LevelManager.fireballSound);
				fireTime1 = Time.time;
			}
		}

		/******** Apply velocity and update animator */
		m_Rigidbody2D.velocity = new Vector2 (moveDirectionX * currentSpeedX, m_Rigidbody2D.velocity.y);

		m_Animator.SetBool ("isJumping", isJumping);
		m_Animator.SetBool ("isFallingNotFromJump", isFalling && !isJumping);
		m_Animator.SetBool ("isCrouching", isCrouching);
		m_Animator.SetFloat ("absSpeed", Mathf.Abs (currentSpeedX));

		if (faceDirectionX != 0 && !isChangingDirection) {
			moveDirectionX = faceDirectionX;
		}
	}


	/****************** Input and state polling */
	void Update() {
		if (!inputFreezed) {
			faceDirectionX = Input.GetAxisRaw ("Horizontal");
			isDashing = Input.GetButton ("Dash");
			isCrouching = Input.GetButton ("Crouch");
			isShooting = Input.GetButtonDown ("Dash");
			jumpButtonHeld = Input.GetButton ("Jump");
			if (Input.GetButtonUp ("Jump")) {
				jumpButtonReleased = true;
			}
		}

		wasGrounded = isGrounded;
		isGrounded = Physics2D.OverlapPoint (m_GroundCheck1.position, GroundLayers)
		          || Physics2D.OverlapPoint (m_GroundCheck2.position, GroundLayers);
		isFalling = m_Rigidbody2D.velocity.y < 0 && !isGrounded;
		isChangingDirection = currentSpeedX > 0 && faceDirectionX * moveDirectionX < 0;

		// Coyote time: refresh while grounded, countdown when airborne
		if (isGrounded) {
			coyoteTimer = CoyoteTime;
			if (!wasGrounded) {
				// Landing resets the aerial stomp combo
				t_LevelManager.ResetStompCombo();
			}
		} else {
			coyoteTimer -= Time.deltaTime;
		}

		if (inputFreezed && !t_LevelManager.gamePaused) {
			if (isDying) {
				deadUpTimer -= Time.unscaledDeltaTime;
				if (deadUpTimer > 0) {
					gameObject.transform.position += Vector3.up * .22f;
				} else {
					gameObject.transform.position += Vector3.down * .2f;
				}
			} else if (isClimbingFlagPole) {
				m_Rigidbody2D.MovePosition (m_Rigidbody2D.position + climbFlagPoleVelocity * Time.deltaTime);
			}
		}
	}


	public bool isDying = false;
	float deadUpTimer = .25f;
	public void FreezeAndDie() {
		FreezeUserInput ();
		isDying = true;
		m_Rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
		m_Animator.SetTrigger ("respawn");
		gameObject.layer = LayerMask.NameToLayer ("Falling to Kill Plane");
		gameObject.GetComponent<SpriteRenderer>().sortingLayerName = "Foreground Effect";
	}


	bool isClimbingFlagPole = false;
	Vector2 climbFlagPoleVelocity = new Vector2 (0, -5f);
	public void ClimbFlagPole() {
		FreezeUserInput ();
		isClimbingFlagPole = true;
		m_Animator.SetBool ("climbFlagPole", true);
		m_Rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
	}


	void JumpOffPole() {
		transform.position = new Vector2 (transform.position.x + .5f, transform.position.y);
		m_Animator.SetBool ("climbFlagPole", false);
		AutomaticWalk(castleWalkSpeedX);
		m_Rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
	}


	/****************** Automatic movement (cutscenes, castle walk, etc.) */
	public void UnfreezeUserInput() {
		inputFreezed = false;
	}

	public void FreezeUserInput() {
		inputFreezed = true;
		jumpButtonHeld = false;
		jumpButtonReleased = true;

		faceDirectionX = 0;
		moveDirectionX = 0;

		currentSpeedX = 0;
		speedXBeforeJump = 0;
		automaticWalkSpeedX = 0;
		automaticGravity = normalGravity;

		isDashing = false;
		wasDashingBeforeJump = false;
		isCrouching = false;
		isChangingDirection = false;
		isShooting = false;

		gameObject.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
	}


	public void AutomaticWalk(float walkVelocityX) {
		FreezeUserInput ();
		if (walkVelocityX != 0) {
			faceDirectionX = walkVelocityX / Mathf.Abs (walkVelocityX);
		}
		automaticWalkSpeedX = Mathf.Abs(walkVelocityX);
	}


	public void AutomaticCrouch() {
		FreezeUserInput ();
		isCrouching = true;
	}


	/****************** Misc */
	public void UpdateSize() {
		// Use cached LevelManager reference instead of FindObjectOfType
		GetComponent<Animator>().SetInteger("marioSize", t_LevelManager.marioSize);
	}

	float IncreaseWithinBound(float val, float delta, float maxVal = Mathf.Infinity) {
		val += delta;
		if (val > maxVal) val = maxVal;
		return val;
	}

	float DecreaseWithinBound(float val, float delta, float minVal = 0) {
		val -= delta;
		if (val < minVal) val = minVal;
		return val;
	}

	void OnCollisionEnter2D(Collision2D other) {
		Vector2 normal = other.contacts[0].normal;
		Vector2 bottomSide = new Vector2 (0f, 1f);
		bool bottomHit = normal == bottomSide;

		if (other.gameObject.tag.Contains ("Enemy")) {
			Enemy enemy = other.gameObject.GetComponent<Enemy> ();

			if (!t_LevelManager.isInvincible ()) {
				if (!other.gameObject.GetComponent<KoopaShell> () ||
					other.gameObject.GetComponent<KoopaShell> ().isRolling ||
					!bottomHit || (bottomHit && !enemy.isBeingStomped))
				{
					t_LevelManager.MarioPowerDown ();
				}

			} else if (t_LevelManager.isInvincibleStarman) {
				t_LevelManager.MarioStarmanTouchEnemy (enemy);
			}

		} else if (other.gameObject.tag == "Goal" && isClimbingFlagPole && bottomHit) {
			isClimbingFlagPole = false;
			JumpOffPole ();
		}
	}

}
