using UnityEngine;
using System;
using System.Collections;

public class Player : MonoBehaviour 
{
	public static Transform playerTransform;
	public static Player instance;

	public float optionDistance;

	public int maxLives = 10;
	public int maxBombs = 10;
	public int lives;
	public int bombs;
	public float power = 0f;
	public uint point;

	public float unfocusedSpeed;
	public float focusedSpeed;

	public bool EI;
	public bool SN;
	public bool TF;
	public bool JP;

	public string ShotType
	{
		get { return ((EI) ? "E" : "I") + ((SN) ? "S" : "N") + ((TF) ? "T" : "F") + ((JP) ? "J" : "P"); }
	}

	//TO-DO: Implement Bomb Functionality
	//Shot Type: 8x Shot Damage
	//Bomb Type: Shield, Absorbs incoming bullets, high bullet to point ratio, does not damage enemies
	public bool Extravert
	{
		get { return EI; }
		set { EI = value; }
	}
	
	//TO-DO: Implement Bomb Functionality
	//Shot Type: 8x shot speed
	//Bomb Type: Linear laser, Cancels bullets that collide with it, low bullet to point ratio, instantly kills non-boss enemies
	public bool Introvert
	{
		get { return !EI; }
		set { EI = !value; }
	}
	
	//TO-DO: Implement Functionality
	//Shot Type: Wide Spread, halved damage
	//Bomb Type: 2x Area of Effect, 1/2x effectiveness
	public bool Sensing
	{
		get { return SN; }
		set { SN = value; }
	}
	
	//TO-DO: Implement Functionality
	//Shot Type: Forward Focus, doubled damage
	//Bomb Type: 1/2x Area of effect, 2x as effective
	public bool Intuitive
	{
		get { return !SN; }
		set { SN = !value; }
	}
	
	//TO-DO: Implement Functionality
	//Shot Type: 
	//Bomb Type: Shorter bomb duration, longer death bomb window
	public bool Thinking
	{
		get { return TF; }
		set { TF = !value; }
	}
	
	//TO-DO: Implement Functionality
	//Shot Type:
	//Bomb Type: Longer Bomb Duration, shorter death bomb window
	public bool Feeling
	{
		get { return !TF; }
		set { TF = value; }
	}
	
	//TO-DO: Implement Functionality
	//Shot Type: Makes Player shots homing
	//Bomb Type: Six starting lives, two bombs each
	public bool Judging
	{
		get { return JP; }
		set { JP = value; }
	}
	
	//TO-DO: Implement Functionality
	//Shot Type: Makes Player Shots piercing
	//Bomb Type: Two starting lives, six bombs each
	public bool Percieving
	{
		get { return !JP; }
		set { JP = !value; }
	}

	public bool invincible = false;

	public float deathInvincibilityTime;
	public float invincibilityFlashInterval;

	public AudioClip DeathClip;
	public AudioClip GrazeClip;
	public AudioClip PickupClip;
	public AudioClip ExtendClip;
	public AudioClip BombUpClip;

	public Timer MainShotTimer;
	public int MainShotDamage;

	public float baseOptionFireDelay;
	public float baseOptionShotDamage;

	//Private Variables
	private Renderer hitboxRenderer;
	private AudioSource audioSource;
	[HideInInspector]
	public Option[] options;
	[HideInInspector]
	public bool[] atMovementLimit;
	[HideInInspector]
	public Bomb bomb;
	private Timer OptionShotTimer;

	void Start () 
	{
		//Cache commonly accessed components of player
		instance = this;
		playerTransform = transform;
		audioSource = audio;
		hitboxRenderer = playerTransform.FindChild("Death Hitbox").renderer;
		atMovementLimit = new bool[4];
		options = playerTransform.GetComponentsInChildren<Option>();
		bomb = playerTransform.GetComponentInChildren<Bomb> ();
		foreach(Option o in options)
		{
			o.gamObj.SetActive(false);
		}
		bomb.Active = false;
		OptionShotTimer = new Timer ();
		OptionShotTimer.totalTime = ((Introvert) ? 0.125f : 1f) * baseOptionFireDelay;
		OptionShotTimer.Start ();
		MainShotTimer.Start ();
	}

	private int Sign(float x)
	{
		return (x == 0) ? 0 : (x > 0) ? 1 : -1;
	}

	void FixedUpdate () 
	{
		bool focused;
		float deltat, speed;
		Vector3 movementVector;

		deltat = Time.fixedDeltaTime;
		focused = hitboxRenderer.enabled = Input.GetButton("Focus");
		speed = (focused) ? focusedSpeed : unfocusedSpeed;
		
		OptionShotTimer.totalTime = ((Introvert) ? 1f/8f : 8f) * baseOptionFireDelay;
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

		//Bombing
		if(!bomb.Active)
		{
			if(Input.GetButtonDown("Bomb"))
			{
				//Instantiate Bomb at character location
				StartCoroutine(Invincibility(false, bomb.Duration, deltat));
				bomb.Active = true;
			}
		}

		//Shooting
		if(Input.GetButton("Shoot"))
		{
			OptionShotTimer.Start();
			if(MainShotTimer.Done)
			{
				Vector3 offset = new Vector3(1.5f,0,0);
				GameObjectManager.PlayerShots.Spawn(playerTransform.position + offset, true);
				GameObjectManager.PlayerShots.Spawn(playerTransform.position - offset, true);
				MainShotTimer.Reset();
			}
			if(OptionShotTimer.Done)
			{
				for(int i = 0; i < options.Length; i++)
				{
					if(options[i].gamObj.activeSelf)
					{
						options[i].Fire();
					}
				}
				OptionShotTimer.Reset();
			}
		}
		else
		{
			MainShotTimer.remainingTime = 0;
			OptionShotTimer.Pause();
		}

		bool optActive;
		for(int i = 0; i < options.Length; i++)
		{
			optActive = (float)i <= power - 1;
			options[i].gamObj.SetActive(optActive);
			if(optActive)
			{
				float distance = (focused) ? optionDistance * (2f/3f) : optionDistance;
				float angle = -(Mathf.PI / (Mathf.FloorToInt(power) + 1)) * (i + 1);
				Vector3 position = new Vector3(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance, 0);
				options[i].trans.localPosition = position;
			}
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
				elapsedTime += intervalTime;
				index++;
				yield return new WaitForSeconds(intervalTime);
			}
			mat.color = colors[1];
		}
		else
		{
			while(elapsedTime < time)
			{
				elapsedTime += intervalTime;
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
				Global.Score += 10000000;
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
				Global.gameState = GameState.GameOver;
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
