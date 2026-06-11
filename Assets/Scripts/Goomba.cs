using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goomba : Enemy {
	private Animator m_Animator;
	private const float StompedDuration = 0.5f;

	void Start () {
		m_Animator = GetComponent<Animator> ();

		starmanBonus      = 100;
		rollingShellBonus = 500;
		hitByBlockBonus   = 100;
		fireballBonus     = 100;
		stompBonus        = 100;
	}

	public override void StompedByMario() {
		isBeingStomped = true;
		StopInteraction ();
		m_Animator.SetTrigger ("stomped");
		Destroy (gameObject, StompedDuration);
		isBeingStomped = false;
	}
}
