﻿using UnityEngine;
using System.Collections;

public class MadOvenSpatel:Boss {
	private bool attacking;
	public Animator animatie;
	void OnCollisionEnter(Collision collision) {
		if(!attacking)
			return;

		if(collider.gameObject.CompareTag("Player"))
			collider.GetComponent<Player>().Damage(data.AttackDamage, 0, false);
	}

	public void StartAttack() {
		StartCoroutine(Attack());
	}

	public override void Damage(int amount) {
		if(data.Health <= 0)
			return;

		data.Health -= amount;
		DisplayCombatText(amount.ToString(), Color.red, 0.4f);

		Debug.Log("Spatel: " + data.Health);
	}

	private IEnumerator Attack() {
		while(true) {
			yield return new WaitForSeconds(data.AttackDelay / 2);
			animatie.SetInteger("Attack",Random.Range(0,4));
			attacking = true;

			yield return new WaitForSeconds(data.AttackDelay / 2);
			animatie.SetInteger("Attack",0);
			attacking = false;
		}
	}
}
