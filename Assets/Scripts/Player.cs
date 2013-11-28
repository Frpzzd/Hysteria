using UnityEngine;
using System;
using System.Collections;

public class Player : MonoBehaviour 
{
	[HideInInspector]
	public static Transform playerTransform;
	[HideInInspector]
	public static Player instance;

	public GameObject optionPrefab;
	public int maximumOptions;
	public float optionDistance;
	
	public uint lives;
	public uint bombs;
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

	public Trait FireRate;
	public Trait Homing;
	public Trait Spread;
	public Trait ShotDamage;

	//Note Spread is in terms of PI
	/*public float baseFireRate = 3;
	public float baseHoming = 0;
	public float baseShotDamage = 10;
	public float baseSpread = 0.5f;
	public float FireRateTraitScaling = 2;
	public float HomingTraitScaling = 1;
	public float ShotDamageTraitScaling = 6;
	public float SpreadTraitScaling = 0.5f;*/
	
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
	private GameObject[] options;
	private float oldPower;
	
	void Start () 
	{
		//Cache commonly accessed components of player
		instance = this;
		playerTransform = transform;
		audioSource = audio;
		hitboxRenderer = gameObject.GetComponentInChildren<Renderer>();

		options = new GameObject[maximumOptions];

		for (int i = 0; i < maximumOptions; i++)
		{
			GameObject option = (GameObject)Instantiate(optionPrefab, playerTransform.position, Quaternion.identity);
			option.transform.parent = playerTransform;
			option.SetActive(false);
			options[i] = option;
		}
	}

	private int Sign(float x)
	{
		return (x == 0) ? 0 : (x > 0) ? 1 : -1;
	}

	void Update () 
	{
		float deltat = Time.deltaTime;
		bool focused = hitboxRenderer.enabled = (Input.GetAxis("Focus") != 0);

		//Movement
		Vector3 movementVector = Vector3.zero;
		movementVector.x = Sign(Input.GetAxis("Horizontal")) * ((focused) ? focusedSpeed : unfocusedSpeed);
		movementVector.y = Sign(Input.GetAxis("Vertical")) * ((focused) ? focusedSpeed : unfocusedSpeed);
		playerTransform.position += movementVector * deltat;

		//Bombing
		if(Input.GetButtonDown("Bomb"))
		{
			Debug.Log("Bomb Deployed");
			if(!bombDeployed)
			{
				//Instantiate Bomb at character location
				StartCoroutine(Invincibility(false, bombInvincibilityTime, Time.deltaTime));
				bombDeployed = true;
			}
		}

		//Shooting
		if(Input.GetButtonDown("Shoot"))
		{
			Debug.Log ("Start Shooting");
			shooting = true;
		}
		else if(Input.GetButtonUp("Shoot"))
		{
			Debug.Log ("Stop Shooting");
			shooting = false;
		}
		if(shooting)
		{

		}

		for(int i = 0; i < (int)power; i++)
		{
			float distance = (focused) ? optionDistance * (2f/3f) : optionDistance;
			options[i].SetActive(true);
			float angle = -(Mathf.PI / (power + 1)) * (i + 1);
			Vector3 position = new Vector3(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance, 0);
			options[i].transform.localPosition = position;
		}
		for(int i = (int)power; i < options.Length; i++)
		{
			options[i].SetActive(false);
		}
	}

	void OnTriggerEnter2D(Collider2D col)
	{
		switch(col.gameObject.layer)
		{
			case 9:			//Enemy
			case 11:		//Enemy Bullet
				break;
			default:
				break;
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
				oldPower = power;
				power += 0.01f;
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
			GameObject[] pickups = GameObject.FindGameObjectsWithTag("Pickup");
			foreach(GameObject go in pickups)
			{
				go.GetComponent<Pickup>().state = global::Pickup.PickupState.Normal;
			}
			StartCoroutine(Invincibility(true, deathInvincibilityTime, invincibilityFlashInterval));
		}
	}
}
