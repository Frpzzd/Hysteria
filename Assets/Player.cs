using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour 
{
	[HideInInspector]
	public static Transform playerTransform;
	[HideInInspector]
	public static Collider playerCollider;

	public float unfocusedMovementSpeed;
	public float focusedMovementSpeed;

	// Based on Hans Eysenck's ideas of temprament
	// Introversion vs Extraverison controls spread and power
	// Psychotism vs Neurotism controls speed and power

	// Extraversion
	// -- Hot Red
	// -- Homing
	// -- Weaker Shots
	// -- Wider Spread

	// Introversion
	// -- Cool Blue
	// -- Focused
	// -- Stronger shots
	// -- Tighter Spread

	// Psychotism 
	// -- High Power Shots
	// -- Slow Shot Speed

	// Neurotism
	// -- Low Power Shots
	// -- Fast Shot Speed

	// > 0 := Extraversion/Neurotism
	// < 0 := Intraversion/Psychotism
	// = 0 := balanced
	public float IntroExtraVersion;
	public float NeuroPsychOtism;

	public float FireRate;
	public float Homing;
	public float ShotDamage;
	public float Spread;

	public float baseFireRate = 3;
	public float TraitFireRateScaling = 2;
	public float baseHoming = 0;
	public float TraitHomingScaling = 1;
	public float baseShotDamage = 10;
	public float TraitShotDamageScaling = 6;
	public float baseSpread = Mathf.PI / 2;
	public float TraitSpreadScaling = Mathf.PI / 2;

	void Start () 
	{
		//Cache commonly accessed components of player
		playerTransform = transform;

		FireRate = baseFireRate + NeuroPsychOtism * TraitFireRateScaling;
		Homing = baseHoming + (IntroExtraVersion + 2) / 2 * TraitHomingScaling;
		ShotDamage = baseShotDamage + NeuroPsychOtism * TraitShotDamageScaling;
		Spread = baseSpread + IntroExtraVersion * TraitSpreadScaling;
	}

	void Update () 
	{
		//Movement
		Vector3 movementVector = Vector3.zero;
		bool focused = Input.GetKeyDown (Global.Control.Focus);
		if(Input.GetKeyDown(Global.Control.Up))
		{
			movementVector.y += (focused) ? focusedMovementSpeed : unfocusedMovementSpeed;
		}
		if(Input.GetKeyDown(Global.Control.Down))
		{
			movementVector.y -= (focused) ? focusedMovementSpeed : unfocusedMovementSpeed;
		}
		if(Input.GetKeyDown(Global.Control.Left))
		{
			movementVector.x -= (focused) ? focusedMovementSpeed : unfocusedMovementSpeed;
		}
		if(Input.GetKeyDown(Global.Control.Right))
		{
			movementVector.x += (focused) ? focusedMovementSpeed : unfocusedMovementSpeed;
		}
		playerTransform.localPosition += movementVector;
	}
}
