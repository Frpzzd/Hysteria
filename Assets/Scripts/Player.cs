using UnityEngine;
using System;
using System.Collections;

public class Player : MonoBehaviour 
{
	[HideInInspector]
	public static Transform playerTransform;
	[HideInInspector]
	public static Player instance;

	public float optionDistance;

	public int maxLives;
	public int maxBombs;
	public int lives;
	public int bombs;
	public float power;
	public uint point;

	public float unfocusedSpeed;
	public float focusedSpeed;

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

	[Serializable]
	public class Trait
	{
		public float baseValue;
		public float scaling;
		[HideInInspector]
		public float value;
	}
	
	//Note Spread is in terms of PI
	public Trait FireRate;
	public Trait Homing;
	public Trait Spread;
	public Trait ShotDamage;

	public bool invincible = false;

	public float deathInvincibilityTime;
	public float bombInvincibilityTime;
	public float invincibilityFlashInterval;
	public AudioClip DeathClip;
	public AudioClip GrazeClip;
	public AudioClip PickupClip;
	public AudioClip ExtendClip;
	public AudioClip BombUpClip;

	//Private Variables
	private bool bombDeployed = false;
	private bool shooting = false;
	private Renderer hitboxRenderer;
	private AudioSource audioSource;
	[HideInInspector]
	public Option[] options;
	private float oldPower;
	[HideInInspector]
	public bool[] atMovementLimit;
	
	void Start () 
	{
		//Cache commonly accessed components of player
		instance = this;
		playerTransform = transform;
		audioSource = audio;
		hitboxRenderer = playerTransform.FindChild("Death Hitbox").renderer;
		oldPower = power;
		atMovementLimit = new bool[4];
		options = playerTransform.GetComponentsInChildren<Option>();
		foreach(Option o in options)
		{
			o.gamObj.SetActive(false);
		}
	}

	private int Sign(float x)
	{
		return (x == 0) ? 0 : (x > 0) ? 1 : -1;
	}

	void Update () 
	{
		bool focused, focusUp, focusDown, shootUp, shootDown;
		float deltat, speed;
		Vector3 movementVector;

		deltat = Time.deltaTime;
		focused = Input.GetButton("Focus");
		focusUp = Input.GetButtonUp ("Focus");
		focusDown = Input.GetButtonDown ("Focus");
		shootUp = Input.GetButtonUp ("Shoot");
		shootDown = Input.GetButtonDown ("Shoot");
		speed = (focused) ? focusedSpeed : unfocusedSpeed;
		
		//Movement
		movementVector = Vector3.zero;
		movementVector.x = Sign(Input.GetAxisRaw("Horizontal")) * speed;
		movementVector.y = Sign(Input.GetAxisRaw("Vertical")) * speed;
		if((atMovementLimit[0] && movementVector.y > 0) ||
		   (atMovementLimit[1] && movementVector.y < 0))
		{
			movementVector.y = 0;
		}
		if((atMovementLimit[2] && movementVector.x > 0) ||
		   (atMovementLimit[3] && movementVector.x < 0))
		{
			movementVector.x = 0;
		}
		playerTransform.position += movementVector * deltat;

		if(focusDown)
		{
			hitboxRenderer.enabled = true;
		}
		else if(focusUp)
		{
			hitboxRenderer.enabled = false;
		}

		//Bombing
		if(!bombDeployed)
		{
			if(Input.GetButtonDown("Bomb"))
			{
				//Instantiate Bomb at character location
				StartCoroutine(Invincibility(false, bombInvincibilityTime, Time.deltaTime));
				bombDeployed = true;
			}
		}

		//Shooting
		if(shootDown)
		{
			shooting = true;
		}
		else if(shootUp)
		{
			shooting = false;
		}

		if(focusDown || focusUp || Mathf.FloorToInt(oldPower) != Mathf.FloorToInt(power))
		{
			bool optActive;
			for(int i = 0; i < options.Length; i++)
			{
				optActive = i < power;
				options[i].gamObj.SetActive(optActive);
				if(optActive)
				{
					float distance = (focused) ? optionDistance * (2f/3f) : optionDistance;
					float angle = -(Mathf.PI / (Mathf.FloorToInt(power) + 1)) * (i + 1);
					Vector3 position = new Vector3(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance, 0);
					options[i].trans.localPosition = position;
				}
			}
			oldPower = power;
		}
	}

	private IEnumerator Invincibility(bool flash, float time, float intervalTime)
	{
		invincible = true;
		float elapsedTime = 0f;
		if(flash)
		{
			Material mat = GetComponent<MeshRenderer>().material;
			Color[] colors = new Color[]{Color.clear, mat.color};
			int index = 0;
			while(elapsedTime < time)
			{
				mat.color = colors[index % 2];
				elapsedTime += Time.deltaTime;
				index++;
				yield return new WaitForSeconds(intervalTime);
			}
			mat.color = colors[1];
		}
		else
		{
			while(elapsedTime < time)
			{
				elapsedTime += Time.deltaTime;
				yield return new WaitForSeconds(intervalTime);
			}
		}
		invincible = false;
	}

	public void Graze(Bullet bullet)
	{
		if(!invincible && !bullet.grazed)
		{
			Global.Graze++;
			bullet.grazed = true;
		}
	}

	public void Pickup(PickupType type)
	{
		switch(type)
		{
			case PickupType.Point:
				audioSource.PlayOneShot (PickupClip);
				break;
			case PickupType.PointValue:
				audioSource.PlayOneShot (PickupClip);
				break;
			case PickupType.Power:
				ChangePower(0.01f);
				audioSource.PlayOneShot (PickupClip);
				break;
			case PickupType.Bomb:
				audioSource.PlayOneShot (BombUpClip);
				break;
			case PickupType.Life:
				audioSource.PlayOneShot(ExtendClip);
				break;
		}
	}

	private void ChangePower(float amount)
	{
		power = (power + amount < options.Length) ? power + amount : options.Length;
	}

	public void Die()
	{
		if(!invincible)
		{
			if(lives <= 0)
			{
				Global.gameState = GameState.Game_Over;
			}
			lives--;
			audioSource.PlayOneShot(DeathClip);
			ChangePower(-1.0f);
			GameObject[] pickups = GameObject.FindGameObjectsWithTag("Pickup");
			foreach(GameObject go in pickups)
			{
				go.GetComponent<Pickup>().state = global::Pickup.PickupState.Normal;
			}
			StartCoroutine(Invincibility(true, deathInvincibilityTime, invincibilityFlashInterval));
		}
	}
}
