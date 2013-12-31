using UnityEngine;
using System.Collections.Generic;

public class SoundManager : StaticGameObject<SoundManager>
{
	private static AudioSource musicSource;
	private static Dictionary<AudioClip, AudioObject> sfxSources;

	public Vector3 offset;
	public GameObject audioSourcePrefab;

	private static Vector3 Offset
	{
		get { return Instance.offset; }
	}

	private static GameObject prefab
	{
		get { return Instance.audioSourcePrefab; }
	}

	private class AudioObject
	{
		public AudioSource source;
		public Transform transform;

		public AudioObject(AudioSource source, Transform transform)
		{
			this.source = source;
			this.transform = transform;
		}
	}

	public override void Awake()
	{
		base.Awake ();
		musicSource = audio;
		sfxSources = new Dictionary<AudioClip, AudioObject> ();
	}
	
	public static void PlayMusic(AudioClip bgm)
	{
		if(musicSource == null)
		{
			musicSource = Instance.audio;
		}
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

	private static AudioObject GetSource(AudioClip sfx)
	{
		if(sfxSources.ContainsKey(sfx))
		{
			return sfxSources[sfx];
		}
		else
		{
			GameObject newSource = (GameObject)Instantiate(prefab);
			newSource.transform.parent = Instance.Transform;
			newSource.transform.localPosition = Vector3.zero;
			newSource.name = sfx.name + " Source";
			newSource.audio.clip = sfx;
			AudioObject audObj = new AudioObject(newSource.audio, newSource.transform);
			sfxSources.Add(sfx, audObj);
			return audObj;
		}
	}
	
	public static void PlaySoundEffect(AudioClip sfx, Vector3 location)
	{
		if(sfx != null)
		{
			AudioObject source = GetSource (sfx);
			source.transform.position =  new Vector3 (location.x + Offset.x, Instance.Transform.position.y + Offset.y, Offset.z);
			source.source.PlayOneShot (sfx);
		}
	}

	public static void PlaySoundEffect(AudioClip sfx)
	{
		PlaySoundEffect (sfx, 1f, Vector3.zero, false);
	}
	
	public static void PlaySoundEffect(AudioClip sfx, float volume, Vector3 location, bool inWorld)
	{
		AudioObject source = GetSource (sfx);
		source.transform.position = new Vector3 ((inWorld) ? location.x : Instance.Transform.position.x + Offset.x, Instance.Transform.position.y + Offset.y, Offset.z);
		source.source.PlayOneShot (sfx);
	}
}
