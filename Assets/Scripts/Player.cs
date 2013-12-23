using UnityEngine;
using System;
using System.Collections;

public class Player : StaticGameObject<Player>
{
	private static Transform respawnLocation;

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

	public float powerLostOnDeath;
	public int powerItemsExpelledOnDeath;

	public static float Power
	{
		get { return instance.power; }
	}

	public static bool Focused
	{
		get { return instance.focused; }
	}

	public static int MainShotDamage
	{
		get { return mainShotDamage; }
	}

	public static int OptionShotDamage
	{
		get { return (int)(((Extravert) ? 8f : 1f) * ((Intuitive) ? 2f : 1f) * baseOptionShotDamage); }
	}

	public static bool MaxPower
	{
		get { return Power / instance.options.Length > 1; }
	}

	public static int MaxOptions
	{
		get { return instance.options.Length; }
	}

	public static string ShotType
	{
		get { return ((Extravert) ? "E" : "I") + ((Sensing) ? "S" : "N") + ((Thinking) ? "T" : "F") + ((Judging) ? "J" : "P"); }
	}

	public static Transform PlayerTransform
	{
		get { return instance.Transform; }
	}

	//TO-DO: Implement Bomb Functionality
	//Shot Type: 8x Shot Damage
	//Bomb Type: Shield, Absorbs incoming bullets, high bullet to point ratio, does not damage enemies
	public static bool Extravert
	{
		get { return instance.EI; }
		set { instance.EI = value; }
	}
	
	//TO-DO: Implement Bomb Functionality
	//Shot Type: 8x shot speed
	//Bomb Type: Linear laser, Cancels bullets that collide with it, low bullet to point ratio, instantly kills non-boss enemies
	public static bool Introvert
	{
		get { return !instance.EI; }
		set { instance.EI = !value; }
	}
	
	//TO-DO: Implement Bomb Functionality
	//Shot Type: Wide Spread, focusing narrows wide spread
	//Bomb Type: 2x Area of Effect
	public static bool Sensing
	{
		get { return instance.SN; }
		set { instance.SN = value; }
	}
	
	//TO-DO: Implement Bomb Functionality
	//Shot Type: Forward Focus, focusing increases fire rate
	//Bomb Type: 2x as effective
	public static bool Intuitive
	{
		get { return !instance.SN; }
		set { instance.SN = !value; }
	}
	
	//TO-DO: Implement Functionality
	//Shot Type: 
	//Bomb Type: Shorter bomb duration, longer death bomb window
	public static bool Thinking
	{
		get { return instance.TF; }
		set { instance.TF = !value; }
	}
	
	//TO-DO: Implement Functionality
	//Shot Type:
	//Bomb Type: Longer Bomb Duration, shorter death bomb window
	public static bool Feeling
	{
		get { return !instance.TF; }
		set { instance.TF = value; }
	}
	
	//TO-DO: Implement Functionality
	//Shot Type: Makes Player shots homing
	//Bomb Type: Six starting lives, two bombs each
	public static bool Judging
	{
		get { return instance.JP; }
		set { instance.JP = value; }
	}
	
	//TO-DO: Implement Functionality
	//Shot Type: Makes Player Shots piercing
	//Bomb Type: Two starting lives, six bombs each
	public static bool Percieving
	{
		get { return !instance.JP; }
		set { instance.JP = !value; }
	}

	private bool invincible = false;

	public float deathInvincibilityTime;
	public float invincibilityFlashInterval;

	public AudioClip DeathClip;
	public AudioClip GrazeClip;
	public AudioClip PickupClip;
	public AudioClip ExtendClip;
	public AudioClip BombUpClip;
	public AudioClip fireClip;

	public Timer mainShotDelay;
	public const int mainShotDamage = 1;

	public float baseOptionFireDelay;
	public const int baseOptionShotDamage = 1;

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
	private Timer OptionShotTimer;

	public override void Awake()
	{
		base.Awake ();
		//Cache commonly accessed components of player
		respawnLocation = GameObject.Find ("Player Respawn Location").transform;
		hitboxRenderer = Transform.FindChild("Death Hitbox").renderer as SpriteRenderer;
		spriteRenderer = Transform.FindChild ("Sprite").renderer as SpriteRenderer;
		atMovementLimit = new bool[4];
		options = Transform.GetComponentsInChildren<Option>();
		bomb = Transform.GetComponentInChildren<Bomb> ();
		foreach(Option o in options)
		{
			o.GameObject.SetActive(false);
		}
		bomb.Active = false;
		OptionShotTimer = new Timer ();
		OptionShotTimer.totalTime = ((Introvert) ? 1f : 8f) * baseOptionFireDelay;
		OptionShotTimer.Start ();
		mainShotDelay.Start ();
	}

	private int Sign(float x)
	{
		return (x == 0) ? 0 : (x > 0) ? 1 : -1;
	}

	void FixedUpdate () 
	{
		float deltat, speed;
		Vector3 movementVector;

		deltat = Time.fixedDeltaTime;
		focused = hitboxRenderer.enabled = Input.GetButton("Focus");
		speed = (focused) ? focusedSpeed : unfocusedSpeed;
		OptionShotTimer.totalTime = ((Introvert) ? 1f : 8f) * ((Intuitive && focused) ? 0.5f : 1f) * baseOptionFireDelay;
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
			if(mainShotDelay.Done)
			{
				Vector3 offset = new Vector3(1.5f,0,0);
				GameObjectManager.PlayerShots.Spawn(Transform.position + offset, true);
				GameObjectManager.PlayerShots.Spawn(Transform.position - offset, true);
				SoundManager.PlaySoundEffect(fireClip, Transform.position);
				mainShotDelay.Reset();;
			}
			if(OptionShotTimer.Done && power >= 1)
			{
				for(int i = 0; i < options.Length; i++)
				{
					if(options[i].GameObject.activeSelf)
					{
						options[i].Fire();
					}
				}
				OptionShotTimer.Reset();
			}
		}
		else
		{
			mainShotDelay.remainingTime = 0;
			OptionShotTimer.Pause();
		}

		bool optActive;
		for(int i = 0; i < options.Length; i++)
		{
			optActive = (float)i <= power - 1;
			options[i].GameObject.SetActive(optActive);
			if(optActive)
			{
				float distance = (focused) ? optionDistance * (2f/3f) : optionDistance;
				float angle = -(Mathf.PI / (Mathf.FloorToInt(power) + 1)) * (i + 1);
				Vector3 position = new Vector3(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance, 0);
				options[i].Transform.localPosition = position;
			}
		}
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
			ScoreManager.GrazeBullet();
			bullet.grazed = true;
		}
	}

	public static void Pickup(PickupType type)
	{
		switch(type)
		{
			case PickupType.Point:
				ScoreManager.PointPickup();
				break;
			case PickupType.PointValue:
				ScoreManager.PointValuePickup();
				break;
			case PickupType.Power:
				instance.ChangePower(0.01f);
				ScoreManager.PowerPickup();
				break;
			case PickupType.Bomb:
				instance.bombs++;
				if(instance.bombs > instance.maxBombs)
				{
					instance.bombs = instance.maxBombs;
					ScoreManager.ExtraBomb();
				}
				SoundManager.PlaySoundEffect(instance.BombUpClip, instance.Transform.position);
				break;
			case PickupType.Life:
				instance.lives++;
				if(instance.lives > instance.maxLives)
				{
					instance.lives = instance.maxLives;
					ScoreManager.ExtraLife();
				}
				SoundManager.PlaySoundEffect(instance.ExtendClip, instance.Transform.position);
				break;
		}
		SoundManager.PlaySoundEffect(instance.PickupClip, instance.Transform.position);
	}

	private void ChangePower(float amount)
	{
		power = (power + amount < options.Length) ? power + amount : options.Length;
	}

	public void Die()
	{
		if(!invincible)
		{
			lives--;
			//TO-DO: Play Player death effect at player's location
			SoundManager.PlaySoundEffect(instance.DeathClip, instance.Transform.position);
			Transform.position = respawnLocation.position;
			if(lives <= 0)
			{
				Global.GameStateChange(GameState.GameOver);
			}
			else
			{
				ChangePower(-powerLostOnDeath);
				GameObject[] pickups = GameObject.FindGameObjectsWithTag("Pickup");
				foreach(GameObject go in pickups)
				{
					go.GetComponent<Pickup>().state = global::Pickup.PickupState.Normal;
				}
				StartCoroutine(Invincibility(true, deathInvincibilityTime, invincibilityFlashInterval));
			}
		}
	}
}
