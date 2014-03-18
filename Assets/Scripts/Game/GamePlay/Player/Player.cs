﻿using UnityEngine;
using System;
using System.Collections;

public class Player:MonoBehaviour {
	[SerializeField] private GameObject playerModel;

	private Animator playerAnimation;
	private PlayerCamera playerCamera;
	private PlayerCombat playerCombat;

	private Transform pushablePosition;
	private Transform throwablePosition;
	
	private Vector3 targetPosition;
	
	private PushableObject attachedPushable;
	private ThrowableObject attachedThrowable;
	
	private int mask = ~(1 << 8);
	
	private bool dataInstanceFound;
	private bool isHit;
	
	void Start() {
		playerCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerCamera>();
		playerCombat = GetComponent<PlayerCombat>();
		playerAnimation = GetComponentInChildren<Animator>();

		targetPosition = transform.position;
		
		throwablePosition = transform.FindChild("Model").FindChild("Throwable Position");
		pushablePosition = transform.FindChild("Pushable Position");
	}
	
	void Update() {
		if(!dataInstanceFound) {
			if(GameObject.FindGameObjectWithTag("Global Manager") == null) {
				return;
			} else {
				dataInstanceFound = true;
			}
		}
	}
	
	void FixedUpdate() {
		rigidbody.velocity = Vector3.zero;
		
		HandlePickedUpObject();
		CheckForInput();
		
		if(Vector3.Distance(transform.position, targetPosition) > 0.5f) {
			playerAnimation.SetBool("IsRunning", true);
			
			playerCamera.CameraDistance = 10;
			transform.position = Vector3.MoveTowards(transform.position, targetPosition, PlayerData.Instance.RunSpeed * Time.deltaTime); 
			
			if(attachedPushable == null) {
				Vector3 lookPosition = targetPosition - playerModel.transform.position;
				Quaternion lookRotation = Quaternion.identity;
				
				if(lookPosition != Vector3.zero)
					lookRotation = Quaternion.LookRotation(lookPosition);
				
				lookRotation.x = 0;
				lookRotation.z = 0;
				
				if(transform.position != targetPosition)
					playerModel.transform.rotation = Quaternion.Slerp(playerModel.transform.rotation, lookRotation, 30);
			} else {
				transform.LookAt(attachedPushable.transform.position);
				transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w);
			}
		} else {
			playerCamera.CameraDistance = -5;
			playerAnimation.SetBool("IsRunning", false);
		}
	}

	/** Pickup an object */
	public void Pickup() {
		Collider[] hits = Physics.OverlapSphere(transform.position, 4);
		
		if(attachedPushable != null || attachedThrowable != null) {
			Drop();
		} else {
			for(int i = 0; i < hits.Length; i++) {
				switch(hits[i].tag) {
				case "ThrowableObject":
					attachedThrowable = hits[i].GetComponent<ThrowableObject>();
					attachedThrowable.collider.enabled = false;
					break;
				case "PushableObject":
					attachedPushable = hits[i].GetComponent<PushableObject>();
					break;
				}
			}
		}
	}

	/** Drop the picked up object */
	public void Drop() {
		if(attachedPushable != null) {
			attachedPushable.rigidbody.mass = 1000;
			attachedPushable.rigidbody.drag = 1000;
			attachedPushable.rigidbody.angularDrag = 1000;
		}
		
		attachedThrowable = null;
		attachedPushable = null;
	}

	/** Throw the picked up object */
	public void ThrowObject() {
		if(attachedThrowable != null) {
			attachedThrowable.Thrown = true;
			Drop();
		}
	}

	/** Damage the player */
	public void Damage(int amount, float stunTime) {
		playerAnimation.SetBool("GettingHit", true);
		PlayerData.Instance.Health -= amount;
		
		isHit = true;
		
		StartCoroutine(DamageDelay(stunTime));
	}

	/** Handle the picked up object */
	private void HandlePickedUpObject() {
		if(attachedThrowable != null) {
			attachedThrowable.transform.position = throwablePosition.position;
			attachedThrowable.transform.rotation = throwablePosition.rotation;
		} else if(attachedPushable != null) {
			if(attachedPushable.transform.position == pushablePosition.position) {
				attachedPushable.rigidbody.mass = 1000;
				attachedPushable.rigidbody.drag = 1000;
				attachedPushable.rigidbody.angularDrag = 1000;
			} else {
				attachedPushable.rigidbody.mass = 1;
				attachedPushable.rigidbody.drag = 1;
				attachedPushable.rigidbody.angularDrag = 0.5f;
			}
			
			attachedPushable.rigidbody.velocity = -(attachedPushable.transform.position - pushablePosition.position).normalized * 5;

			if(Vector3.Distance(transform.position, attachedPushable.transform.position) > 5)
				Drop();
		}
	}

	/** Check for input */
	private void CheckForInput() {
		RaycastHit hit;
		Ray ray;
		
		if(Input.touchCount == 0) {
			if(Input.GetMouseButtonDown(0)) {
				if(Input.mousePosition.x <= 176 && Input.mousePosition.y <= 50)
					return;
				
				ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				Physics.Raycast(ray, out hit, 100, mask);
				
				HandleInput(hit);
			}
		} else {
			Touch touch = Input.GetTouch(0);
			
			if(touch.position.x <= 176 && touch.position.y <= 50)
				return;
			
			ray = Camera.main.ScreenPointToRay(touch.position);
			Physics.Raycast(ray, out hit, 100, mask);
			
			HandleInput(hit);
		}
	}

	/** Handle the input */
	private void HandleInput(RaycastHit hit) {
		if(hit.collider == null)
			return;
		
		if(playerCombat.Defending)
			return;
		
		switch(hit.transform.tag) {
		case "Floor":
			Move(hit.point);
			break;
		case "Enemy":
		case "Destructable":
			playerCombat.Attack(hit.transform.gameObject, hit.transform.tag);
			break;
		case "ThrowableObject":
		case "PushableObject":
			Pickup();
			break;
		default:
			Move(hit.point);
			break;
		}
	}

	/** Move the player */
	private void Move(Vector3 position) {
		playerCombat.TargetEnemy = null;
		playerCombat.TargetDestructable = null;
		targetPosition = new Vector3(position.x, 1, position.z);
	}

	/** The damage delay of the player */
	private IEnumerator DamageDelay(float stunTime){
		yield return new WaitForSeconds(stunTime);
		
		isHit = false;
		
		playerAnimation.SetBool("GettingHit", false);
	}

	/** Get the model of the player */
	public GameObject PlayerModel {
		get { return playerModel; }
	}

	/** Get the animator component of the player */
	public Animator PlayerAnimation {
		get { return playerAnimation; }
	}

	/** Get the camera component of the player */
	public PlayerCamera PlayerCamera {
		get { return PlayerCamera; }
	}

	/** Get the combat component of the player */
	public PlayerCombat PlayerCombat {
		get { return playerCombat; } 
	}
	
	/** Get the pushable position */
	public Transform PushablePosition {
		get { return pushablePosition; }
	}

	/** Get the throwable position */
	public Transform ThrowablePosition {
		get { return throwablePosition; }
	}

	/** Set and/or get the target position */
	public Vector3 TargetPosition {
		set { targetPosition = value; }
		get { return targetPosition; }
	}

	/** Set and/or get the attached pushable object */
	public PushableObject AttachedPushable {
		set { attachedPushable = value; }
		get { return attachedPushable; }
	}

	/** Set and/or get the attached throwable object */
	public ThrowableObject AttachedThrowable {
		set { attachedThrowable = value; }
		get { return attachedThrowable; }
	}
	
	/** Get wheter or not the player is hit */
	public bool Hit { 
		get { return isHit; }
	}
}
