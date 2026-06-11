using System.Collections;
using UnityEngine;

/// <summary>
/// Attach to any trigger collider that covers lava. Instantly kills Mario
/// on contact (same as a fall into the kill plane, but with a brief visual delay).
/// Use this for lava rivers and pools in Bowser's Castle.
/// </summary>
public class LavaZone : MonoBehaviour {

    private LevelManager t_LevelManager;
    private bool         alreadyTriggered;

    private void Start() {
        t_LevelManager = FindObjectOfType<LevelManager>();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "Player" && !alreadyTriggered) {
            alreadyTriggered = true;
            StartCoroutine(LavaDeathCo(other.gameObject));
        }
    }

    private IEnumerator LavaDeathCo(GameObject player) {
        // Tiny delay so the player visually enters the lava before respawning
        yield return new WaitForSeconds(0.1f);
        t_LevelManager.MarioRespawn();
    }
}
