﻿using UnityEngine;

public abstract class Entity:MonoBehaviour {
	[SerializeField] protected GameObject combatTextGO;
	
	[SerializeField] protected Animator animator;

	[SerializeField] protected EntityData entityData;

	/** <summary>Damage the entity, <code>Kill()</code> is called when the entity's health reaches 0</summary>
	 * <param name="amount">The amount of damage dealt to the entity</param> */
	public virtual void Damage(float amount) {
		entityData.Health -= amount;

		if(entityData.Health <= 0)
			Kill();
	}

	/** <summary>Kill the entity, also called when the entities health reaches 0</summary> */
	public virtual void Kill() { }

	/** <summary>Display a new combat text popup</summary>
	 * <param name="text">The text of the popup</param>
	 * <param name="color">The color of the popup</param>
	 * <param name="size">The size of the popup</param> */
	protected void DisplayCombatText(string text, Color color, float size) {
		Vector3 startPosition = new Vector3(transform.position.x, transform.position.y + 5, transform.position.z);
		TextMesh combatText = (Instantiate(combatTextGO, startPosition, Quaternion.identity) as GameObject).GetComponent<TextMesh>();

		combatText.transform.parent = transform;
		combatText.transform.localScale.Scale(new Vector3(size, size, size));

		combatText.text = text;
		combatText.color = color;
	}
}

public class EntityData {
	[SerializeField]
	private float health;

	[SerializeField]
	private float walkSpeed;

	[SerializeField]
	private float runSpeed;

	[SerializeField]
	private float attackDamage;

	[SerializeField]
	private float attackSpeed;

	[SerializeField]
	private float attackRange;

	/** <summary>Set or get the health of the entity</summary> */
	public float Health {
		set { health = value; }
		get { return health; }
	}

	/** <summary>Set or get the walk speed of the entity</summary> */
	public float WalkSpeed {
		set { walkSpeed = value; }
		get { return walkSpeed; }
	}

	/** <summary>Set or get the run speed of the entity</summary> */
	public float RunSpeed {
		set { runSpeed = value; }
		get { return runSpeed; }
	}

	/** <summary>Set or get the attack damage of the entity</summary> */
	public float AttackDamage {
		set { attackDamage = value; }
		get { return attackDamage; }
	}

	/** <summary>Set or get the attack speed of the entity</summary> */
	public float AttackSpeed {
		set { attackSpeed = value; }
		get { return attackSpeed; }
	}

	/** <summary>Set or get the attack range of the entity</summary> */
	public float AttackRange {
		set { attackRange = value; }
		get { return attackRange; }
	}
}