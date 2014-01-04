using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class Player : StaticGameObject<Player>
{
	public static Transform respawnLocation;

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
		get { return Instance.power; }
	}

	public static bool Focused
	{
		get { return Instance.focused; }
	}

	public static int MainShotDamage
	{
		get { return mainShotDamage; }
	}

	public static int OptionShotDamage
	{
		get { return (int)(((Extravert) ? 2f : 1f) * baseOptionShotDamage); }
	}

	public static bool MaxPower
	{
		get { return Power / Instance.options.Length > 1; }
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

	private bool invincible = false;

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
	public const int mainShotDamage = 1;

	public float baseOptionFireDelay;
	private float optionFireDelay;
	private float optionShotTime;
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

	public override void Awake()
	{
		base.Awake ();
		//Cache commonly accessed components of player
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
			mainShotTime -= deltat;
			optionShotTime -= deltat;

			if(mainShotTime < 0)
			{
				Vector3 offset = new Vector3(0.5f,0,0);
				GameObjectManager.PlayerShots.Spawn(Transform.position + offset, true);
				GameObjectManager.PlayerShots.Spawn(Transform.position - offset, true);
				SoundManager.PlaySoundEffect(fireClip, Transform.position);
				mainShotTime = mainShotDelay;
			}
			if(optionShotTime < 0 && power >= 1)
			{
				for(int i = 0; i < options.Length; i++)
				{
					if(options[i].GameObject.activeSelf)
					{
						options[i].Fire();
					}
				}
				optionShotTime = optionFireDelay;
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
				float angle = -(Mathf.PI / (Mathf.FloorToInt(power) + 1)) * (i + 1);
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
			ScoreManager.GrazeBullet();
			bullet.grazed = true;
		}
	}

	public static void Pickup(Pickup.Type type)
	{
		switch(type)
		{
			case global::Pickup.Type.Point:
				ScoreManager.PointPickup();
				break;
			case global::Pickup.Type.PointValue:
				ScoreManager.PointValuePickup();
				break;
			case global::Pickup.Type.Power:
				Instance.ChangePower(0.01f);
				ScoreManager.PowerPickup();
				break;
			case global::Pickup.Type.Bomb:
				Instance.bombs++;
				if(Instance.bombs > Instance.maxBombs)
				{
					Instance.bombs = Instance.maxBombs;
					ScoreManager.ExtraBomb();
				}
				SoundManager.PlaySoundEffect(Instance.BombUpClip, Instance.Transform.position);
				break;
			case global::Pickup.Type.Life:
				Instance.lives++;
				if(Instance.lives > Instance.maxLives)
				{
					Instance.lives = Instance.maxLives;
					ScoreManager.ExtraLife();
				}
				SoundManager.PlaySoundEffect(Instance.ExtendClip, Instance.Transform.position);
				break;
		}
		SoundManager.PlaySoundEffect(Instance.PickupClip, Instance.Transform.position);
	}

	private void ChangePower(float amount)
	{
		power = (power + amount < options.Length) ? power + amount : options.Length;
	}

	public void Die()
	{
		Debug.Log (invincible);
		if(!invincible)
		{
			Debug.Log ("Hello");
			lives--;
			//TODO: Play Player death effect at player's location
			SoundManager.PlaySoundEffect(Instance.DeathClip, Instance.Transform.position);
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
					go.GetComponent<Pickup>().state = global::Pickup.State.Normal;
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
