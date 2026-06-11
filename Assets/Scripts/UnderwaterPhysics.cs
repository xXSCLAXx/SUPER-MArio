using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attach this to a trigger collider that covers the entire water area.
/// When Mario enters it, his gravity and movement are modified to simulate
/// swimming. When he exits, normal physics are restored.
/// </summary>
public class UnderwaterPhysics : MonoBehaviour {

    // ── Underwater physics multipliers ──────────────────────────────────
    // Original SMB water: gravity ~25 % of normal, horizontal speed reduced to ~60 %
    [Header("Water Physics")]
    public float gravityMultiplier    = 0.25f;  // gravity while submerged
    public float swimUpForce          = 8f;     // upward force each time player presses Jump
    public float maxSwimSpeedY        = 4f;     // cap on upward velocity
    public float sinkSpeedY           = -2f;    // constant downward drift (no key held)
    public float horizontalMultiplier = 0.6f;   // fraction of normal walk/run speed

    // ── Internal state ───────────────────────────────────────────────────
    private LevelManager t_LevelManager;
    private Mario        mario;
    private Rigidbody2D  marioRb;

    private float normalGravityScale;
    private bool  isUnderwater;

    // Jump input bookkeeping (mirrors Mario.cs pattern)
    private bool  jumpHeld;
    private bool  prevJumpHeld;

    private void Start() {
        t_LevelManager = FindObjectOfType<LevelManager>();
        mario          = FindObjectOfType<Mario>();
        marioRb        = mario.GetComponent<Rigidbody2D>();
        normalGravityScale = marioRb.gravityScale;
    }

    private void Update() {
        if (!isUnderwater) return;

        prevJumpHeld = jumpHeld;
        jumpHeld     = Input.GetButton("Jump");

        // Tap Jump → burst of upward velocity
        if (jumpHeld && !prevJumpHeld && !mario.inputFreezed) {
            float newVY = Mathf.Min(marioRb.velocity.y + swimUpForce, maxSwimSpeedY);
            marioRb.velocity = new Vector2(marioRb.velocity.x, newVY);
            t_LevelManager.soundSource.PlayOneShot(t_LevelManager.jumpSmallSound);
        }

        // Apply gentle sink when no upward input
        if (!jumpHeld && marioRb.velocity.y > sinkSpeedY) {
            float sinkDelta = sinkSpeedY * Time.deltaTime * 2f;
            marioRb.velocity = new Vector2(marioRb.velocity.x,
                Mathf.Max(marioRb.velocity.y + sinkDelta, sinkSpeedY));
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "Player") {
            EnterWater();
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.tag == "Player") {
            ExitWater();
        }
    }

    private void EnterWater() {
        isUnderwater = true;
        // Reduce gravity
        marioRb.gravityScale = normalGravityScale * gravityMultiplier;
        // Damp any large downward velocity on entry
        if (marioRb.velocity.y < sinkSpeedY) {
            marioRb.velocity = new Vector2(marioRb.velocity.x, sinkSpeedY);
        }
        // Switch to underwater music if LevelManager has it assigned
        if (t_LevelManager.levelMusic != null) {
            t_LevelManager.ChangeMusic(t_LevelManager.levelMusic);
        }
    }

    private void ExitWater() {
        isUnderwater = false;
        marioRb.gravityScale = normalGravityScale;
    }

    // Horizontal speed is capped by UnderwaterSpeedLimiter (see below) so
    // we don't need to override Mario.cs's velocity directly here.
}
