using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Koopa : Enemy {
	public GameObject KoopaShell;

	void Start () {
		starmanBonus      = 200;
		rollingShellBonus = 500;
		hitByBlockBonus   = 100;
		fireballBonus     = 200;
		stompBonus        = 100;
	}

	public override void StompedByMario() {
		isBeingStomped = true;
		StartCoroutine (SpawnKoopaShellCo ());
	}

	IEnumerator SpawnKoopaShellCo() {
		StopInteraction ();
		gameObject.GetComponent<SpriteRenderer> ().enabled = false;
		yield return new WaitForSecondsRealtime(.05f); // brief delay prevents the shell registering immediate contact damage
		Instantiate (KoopaShell, transform.position, Quaternion.identity);
		Destroy (gameObject);
		isBeingStomped = false;
	}
}
