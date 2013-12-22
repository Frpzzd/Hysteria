using UnityEngine;
using System.Collections;

public class SoundManager : CachedObject
{
	private static SoundManager instance;
	private static AudioSource musicSource;
	private static Transform sfxSourceTransform;
	private static AudioSource sfxSource;
	
	// Use this for initialization
	public override void Awake()
	{
		base.Awake ();
		if(instance != null)
		{
			Destroy(gameObject);
			return;
		}
		instance = this;
		sfxSourceTransform = Transform.FindChild ("Source");
		musicSource = audio;
		sfxSource = sfxSourceTransform.audio;
	}
	
	public static void PlayMusic(AudioClip bgm)
	{
		musicSource.Stop();
		musicSource.clip = bgm;
		musicSource.Play();
	}

	public static void PauseMusic()
	{
		musicSource.Pause ();
	}

	public static void UnpauseMusic()
	{
		musicSource.Play ();
	}
	
	public static void PlaySoundEffect(AudioClip sfx, Vector3 location)
	{
		sfxSourceTransform.position =  new Vector3 (location.x, instance.Transform.position.y + 5, -2);
		sfxSource.PlayOneShot (sfx);
	}
	
	public static void PlaySoundEffect(AudioClip sfx, float volume, Vector3 location, bool inWorld)
	{
		sfxSourceTransform.position = new Vector3 ((inWorld) ? location.x : instance.Transform.position.x, instance.Transform.position.y + 5, -2);
		sfxSource.PlayOneShot (sfx);
	}
}
