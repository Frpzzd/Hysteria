using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using DanmakuEngine.Core;

[RequireComponent(typeof(Collider2D))]
public class Player : StaticGameObject<Player>
{
	public static Transform respawnLocation;

	public float optionDistance;

	public int maxLives = 10;
	public int lives;
	public float power = 0f;
	public uint point;

	public float unfocusedSpeed;
	public float focusedSpeed;

	public bool EI;
	public bool SN;
	public bool TF;
	public bool JP;

	public float powerLostOnDeath;
	public int powerItemsExpelledOnDeath;

	public static float Power
	{
		get { return Instance.power; }
	}

	public static bool Focused
	{
		get { return Instance.focused; }
	}

	public static int MainShotDamage
	{
		get { return Instance.mainShotDamage; }
	}

	public static int OptionShotDamage
	{
		get { return (int)(((Extravert) ? 2f : 1f) * Instance.baseOptionShotDamage); }
	}

	public static float MaxPower
	{
		get { return Instance.options.Length + 1; }
	}

	public static float BombCost
	{
		get { return (Instance.EI) ? 1.0f : 0.5f; }
	}

	public static int MaxOptions
	{
		get { return Instance.options.Length; }
	}

	public static string ShotType
	{
		get { return ((Extravert) ? "E" : "I") + ((Sensing) ? "S" : "N") + ((Thinking) ? "T" : "F") + ((Judging) ? "J" : "P"); }
	}

	public static Transform PlayerTransform
	{
		get { return Instance.Transform; }
	}

	//TO-DO: Implement Bomb Functionality
	//Shot Type: 2x Shot Damage
	//Bomb Type: Shield, Absorbs incoming bullets, high bullet to point ratio, does not damage enemies
	public static bool Extravert
	{
		get { return Instance.EI; }
		set { Instance.EI = value; }
	}
	
	//TO-DO: Implement Bomb Functionality
	//Shot Type: 2x shot speed
	//Bomb Type: Linear laser, Cancels bullets that collide with it, low bullet to point ratio, instantly kills non-boss enemies
	public static bool Introvert
	{
		get { return !Instance.EI; }
		set { Instance.EI = !value; }
	}
	
	//TO-DO: Implement Bomb Functionality
	//Shot Type: Wide Spread, focusing narrows wide spread
	//Bomb Type: 2x Area of Effect
	public static bool Sensing
	{
		get { return Instance.SN; }
		set { Instance.SN = value; }
	}
	
	//TO-DO: Implement Bomb Functionality
	//Shot Type: Forward Focus, focusing increases fire rate
	//Bomb Type: 2x as effective
	public static bool Intuitive
	{
		get { return !Instance.SN; }
		set { Instance.SN = !value; }
	}
	
	//TO-DO: Implement Functionality
	//Shot Type: 
	//Bomb Type: Shorter bomb duration, longer death bomb window
	public static bool Thinking
	{
		get { return Instance.TF; }
		set { Instance.TF = !value; }
	}
	
	//TO-DO: Implement Functionality
	//Shot Type:
	//Bomb Type: Longer Bomb Duration, shorter death bomb window
	public static bool Feeling
	{
		get { return !Instance.TF; }
		set { Instance.TF = value; }
	}
	
	//TO-DO: Implement Functionality
	//Shot Type: Makes Player shots homing
	//Bomb Type: Six starting lives, two bombs each
	public static bool Judging
	{
		get { return Instance.JP; }
		set { Instance.JP = value; }
	}
	
	//TO-DO: Implement Functionality
	//Shot Type: Makes Player Shots piercing
	//Bomb Type: Two starting lives, six bombs each
	public static bool Percieving
	{
		get { return !Instance.JP; }
		set { Instance.JP = !value; }
	}

	public bool invincible = false;

	public float deathInvincibilityTime;
	public float invincibilityFlashInterval;

	public AudioClip DeathClip;
	public AudioClip GrazeClip;
	public AudioClip PickupClip;
	public AudioClip ExtendClip;
	public AudioClip BombUpClip;
	public AudioClip fireClip;

	public float mainShotDelay;
	private float mainShotTime;
	public int mainShotDamage;

	public float baseOptionFireDelay;
	private float optionFireDelay;
	private float optionShotTime;
	public int baseOptionShotDamage = 1;

	public GameObject bombPrefab;
	public float baseBombDuration;
	[System.NonSerialized]
	public bool bombDeployed;

	public GameObject optionPrefab;

	//Private Variables

	private SpriteRenderer hitboxRenderer;
	private SpriteRenderer spriteRenderer;
	private Option[] options;
	[HideInInspector]
	public bool[] atMovementLimit;
	[HideInInspector]
	public Bomb bomb;
	[NonSerialized]
	public bool focused;

	public override void Awake()
	{
		base.Awake ();
		//Cache commonly accessed components of player
		hitboxRenderer = Transform.FindChild("Death Hitbox").renderer as SpriteRenderer;
		spriteRenderer = Transform.FindChild ("Sprite").renderer as SpriteRenderer;
		atMovementLimit = new bool[4];
	}

	public void Initialize(int maxOptions)
	{
		options = new Option[maxOptions];
		for(int i = 0; i < maxOptions; i++)
		{
			options[i] = ((GameObject)Instantiate(optionPrefab)).GetComponent<Option>();
			options[i].Transform.parent = Transform;
			options[i].GameObject.SetActive(i == 0);
		}
	}

	private int Sign(float x)
	{
		return (x == 0) ? 0 : (x > 0) ? 1 : -1;
	}

	void FixedUpdate () 
	{
		float deltat, speed;
		Vector3 movementVector;
		#if UNITY_EDITOR
		if(Input.GetKeyDown(KeyCode.F12))
		{
			invincible = !invincible;
		}
		#endif
		deltat = Time.fixedDeltaTime;
		focused = hitboxRenderer.enabled = Input.GetButton("Focus");
		speed = (focused) ? focusedSpeed : unfocusedSpeed;
		optionFireDelay = ((Extravert) ? 2f : 1f) * ((Intuitive && focused) ? 0.5f : 1f) * baseOptionFireDelay;
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
		Transform.position += movementVector * deltat;

		//Bombing
		if(!bombDeployed)
		{
			if(Input.GetButtonDown("Bomb"))
			{
				if(power - BombCost >= 0f)
				{
					//Instantiate Bomb at character location
					ChangePower(-BombCost);
					bomb = ((GameObject)Instantiate (bombPrefab)).GetComponent<Bomb> ();
					bomb.StartCoroutine(bomb.UseBomb(Transform, baseBombDuration, this));
				}
			}
		}

		//Shooting
		if(Input.GetButton("Shoot"))
		{
			mainShotTime -= deltat;
			optionShotTime -= deltat;
			List<PlayerShot> shots = new List<PlayerShot>();
			if(mainShotTime < 0)
			{
				Vector3 offset = new Vector3(0.75f,0,0);
				shots.Add(PlayerShotPool.Spawn(Transform.position + offset, true));
				shots.Add(PlayerShotPool.Spawn(Transform.position - offset, true));
				SoundManager.PlaySoundEffect(fireClip, Transform.position);
				mainShotTime = mainShotDelay;
			}
			if(optionShotTime < 0 && power >= 1)
			{
				for(int i = 0; i < options.Length; i++)
				{
					if(options[i].GameObject.activeSelf)
					{
						shots.Add(options[i].Fire());
					}
				}
				optionShotTime = optionFireDelay;
			}
			foreach(PlayerShot ps in shots)
			{
				ps.GameObject.SetActive(true);
			}
		}
		else
		{
			mainShotTime = 0;
		}

		bool optActive;
		for(int i = 0; i < options.Length; i++)
		{
			optActive = (float)i <= power - 1;
			options[i].GameObject.SetActive(optActive);
			if(optActive)
			{
				float distance = (focused) ? optionDistance * (2f/3f) : optionDistance;
				float angle = -(Mathf.PI / (Mathf.FloorToInt((power <= options.Length) ? power : options.Length) + 1)) * (i + 1);
				Vector3 position = new Vector3(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance, 0);
				options[i].Transform.localPosition = position;
			}
		}
	}

	void OnLevelWasLoaded(int level)
	{
		respawnLocation = GameObject.Find ("Player Respawn Location").transform;
	}

	private IEnumerator Invincibility(bool flash, float time, float intervalTime)
	{
		invincible = true;
		float elapsedTime = 0f;
		if(flash)
		{
			Color[] colors = new Color[]{Color.clear, spriteRenderer.color};
			int index = 0;
			while(elapsedTime < time)
			{
				spriteRenderer.color = colors[index % 2];
				elapsedTime += intervalTime;
				index++;
				yield return new WaitForSeconds(intervalTime);
			}
			spriteRenderer.color = colors[1];
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
			Debug.Log("Bullet Grazed");
			SoundManager.PlaySoundEffect(GrazeClip);
			ScoreManager.GrazeBullet();
			bullet.grazed = true;
		}
	}

	public static void PickupItem(Pickup.Type type)
	{
		switch(type)
		{
			case Pickup.Type.Point:
				ScoreManager.PointPickup();
				break;
			case Pickup.Type.PointValue:
				ScoreManager.PointValuePickup();
				break;
			case Pickup.Type.Power:
				Instance.ChangePower(0.05f);
				ScoreManager.PowerPickup();
				break;
			case Pickup.Type.Life:
				Instance.lives++;
				if(Instance.lives > Instance.maxLives)
				{
					Instance.lives = Instance.maxLives;
					ScoreManager.ExtraLife();
				}
				SoundManager.PlaySoundEffect(Instance.ExtendClip, Instance.Transform.position);
				break;
			case Pickup.Type.BigPower:
				Instance.ChangePower(1.0f);
				for(int i = 0; i < 20; i++)
				{
					ScoreManager.PowerPickup();
				}
				break;
			case Pickup.Type.MaxPower:
				Instance.ChangePower(MaxPower);
				for(int i = 0; i < 20; i++)
				{
					ScoreManager.PowerPickup();
				}
				break;
			case Pickup.Type.BigPoint:
				for(int i = 0; i < 20; i++)
				{
					ScoreManager.PointPickup();
				}
				break;
		}
		SoundManager.PlaySoundEffect(Instance.PickupClip, Instance.Transform.position);
	}

	private void ChangePower(float amount)
	{
		power = (power + amount < options.Length + 1) ? power + amount : options.Length + 1;
	}

	public void Die()
	{
		if(!invincible)
		{
			lives--;
			//TODO: Play Player death effect at player's location
			SoundManager.PlaySoundEffect(Instance.DeathClip, Instance.Transform.position);
			Transform.position = respawnLocation.position;
			if(lives < 0)
			{
				Global.GameStateChange(GameState.ZeroLives);
			}
			else
			{
				power = 0f;
				GameObject[] pickups = GameObject.FindGameObjectsWithTag("Pickup");
				foreach(GameObject go in pickups)
				{
					go.GetComponent<Pickup>().state = Pickup.State.Normal;
				}
				StartCoroutine(Invincibility(true, deathInvincibilityTime, invincibilityFlashInterval));
			}
		}
	}

	public void DebugInvincibility(bool value)
	{
		invincible = value;
	}
}
