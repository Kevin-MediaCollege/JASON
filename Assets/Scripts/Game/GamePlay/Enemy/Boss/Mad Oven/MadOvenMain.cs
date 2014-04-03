﻿using UnityEngine;
using System.Collections;

public class MadOvenMain:Boss {
	private MadOvenHamer hamer;
	private MadOvenSpatel spatel;
	public Animator animatie;
	private bool attacking;
	public bool damageable;
	public int spawncounter;
	public ParticleSystem[] fireAttack;
	protected override void Start() {
		hamer = GameObject.Find("Hamer").GetComponent<MadOvenHamer>();
		spatel = GameObject.Find("Spatel").GetComponent<MadOvenSpatel>();
	}

	protected override void Update() {
		if(Input.GetKeyDown(KeyCode.T))
			StartAttack();

		if(isDead && hamer.IsDead && spatel.IsDead)
			StartCoroutine("SwitchLevel");
	}

	public void StartAttack() {
		hamer.StartAttack();
		spatel.StartAttack();
		animatie.SetBool("start",true);
		hamer.animatie.SetBool("start",true);
		spatel.animatie.SetBool("start",true);
		Debug.Log("ASD");
		StartCoroutine("AnimationDelay");
	}
	IEnumerator AnimationDelay(){
		yield return new WaitForSeconds(5);
		StartCoroutine(Attack());
	}
	void OnCollisionEnter(Collision collision) {
		if(!attacking)
			return;

		if(collider.gameObject.CompareTag("Player"))
			collider.GetComponent<Player>().Damage(data.AttackDamage, 0, false);
	}

	public override void Damage(int amount) {
		if(data.Health <= 0)
			return;

		if(hamer.IsDead && spatel.IsDead) {
			data.Health -= amount;
			DisplayCombatText(amount.ToString(), Color.red, 0.7f);
		} else {
			DisplayCombatText("Blocked", Color.gray, 0.4f);
		}

		Debug.Log("Main: " + data.Health);
	}

	private IEnumerator Attack() {
		while(true) {
			yield return new WaitForSeconds(data.AttackDelay / 2);
			int random = Random.Range(0,10);
			if(random >= 4){
				if(spawncounter < 3){
				animatie.SetInteger("Attack",1);
				damageable = true;
				attacking = true;
				yield return new WaitForSeconds(0.5f);
				spawncounter++;
				fireAttack[0].light.enabled = true;
				for(int i = 0; i < fireAttack.Length; i++){
					fireAttack[i].renderer.enabled = true;
				}
				yield return new WaitForSeconds(1);
				animatie.SetInteger("Attack",0);
				yield return new WaitForSeconds(2.5f);
				fireAttack[0].light.enabled = false;
				for(int i = 0; i < fireAttack.Length; i++){
					fireAttack[i].renderer.enabled = false;
				}
				attacking = false;
				damageable = false;
				}else{
					spawncounter = 0;
					animatie.SetBool("spawnItem",true);
					yield return new WaitForSeconds(0.5f);
					animatie.SetBool("spawnItem",false);
					animatie.SetBool("toIdle",true);
					/* Spawn Item*/
					yield return new WaitForSeconds(4);
					animatie.SetBool("toIdle",false);
				}
			}
			yield return new WaitForSeconds(3);
		}
	}

	private IEnumerator SwitchLevel() {
		GameObject.Find("SGUI Manager").GetComponent<SGUIManager>().RemoveAll();

		yield return new WaitForSeconds(3);

		PlayerData.Instance.Reset();

		Application.LoadLevel("Boss Door");
	}
}
