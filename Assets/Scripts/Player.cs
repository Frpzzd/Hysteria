using UnityEngine;
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

	[HideInInspector]
	private GameObject[] options;
	private float oldPower;
	
	public uint lives;
	public uint bombs;
	public float power;
	public uint point;

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

	[HideInInspector]
	public float FireRate;
	[HideInInspector]
	public float Homing;
	[HideInInspector]
	public float ShotDamage;
	[HideInInspector]
	public float Spread;

	//Note Spread is in terms of PI
	public float baseFireRate = 3;
	public float baseHoming = 0;
	public float baseShotDamage = 10;
	public float baseSpread = 0.5f;
	public float FireRateTraitScaling = 2;
	public float HomingTraitScaling = 1;
	public float ShotDamageTraitScaling = 6;
	public float SpreadTraitScaling = 0.5f;

	public PlayerHitboxHandler HitboxHandler;
	
	public bool invincible;
	[HideInInspector]
	public bool bombDeployed;

	public float deathInvincibilityTime;
	public float bombInvincibilityTime;
	public float invincibilityFlashInterval;

	void Start () 
	{
		//Cache commonly accessed components of player
		instance = this;
		playerTransform = transform;

		options = new GameObject[maximumOptions];

		for (int i = 0; i < maximumOptions; i++)
		{
			GameObject option = (GameObject)Instantiate(optionPrefab, playerTransform.position, Quaternion.identity);
			option.SetActive(false);
			options[i] = option;
		}
	}

	void Update () 
	{
		//Movement
		float deltat = Time.deltaTime;
		bool focused = HitboxHandler.hitboxRenderer.enabled = (Input.GetAxis("Focus") != 0);
		Vector3 movementVector = Vector3.zero;
		movementVector.x = Mathf.Sign(Input.GetAxis("Horizontal")) * ((focused) ? focusedMovementSpeed : unfocusedMovementSpeed);
		movementVector.y = Mathf.Sign(Input.GetAxis("Vertical")) * ((focused) ? focusedMovementSpeed : unfocusedMovementSpeed);
		playerTransform.position += movementVector * deltat;

		if(Input.GetButtonDown("Bomb"))
		{
			if(!bombDeployed)
			{
				//Instantiate Bomb at character location
				StartCoroutine(Invincibility(false, bombInvincibilityTime, Time.deltaTime));
				bombDeployed = true;
			}
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

	void OnTrigger(Collider2D col)
	{
		if(col.gameObject.CompareTag("Enemy Bullet"))
		{
			if(lives <= 0)
			{
				Global.gameState = GameState.Game_Over;
			}
			lives--;
			GameObject[] pickups = GameObject.FindGameObjectsWithTag("Pickup");
			foreach(GameObject go in pickups)
			{
				go.GetComponent<Pickup>().state = Pickup.PickupState.Normal;
			}
			StartCoroutine(Invincibility(true, deathInvincibilityTime, invincibilityFlashInterval));
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
		if(!invincible)
		{
			Global.Graze++;
		}
	}
}
