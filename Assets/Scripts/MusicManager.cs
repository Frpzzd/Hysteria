using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour 
{
	private static AudioSource audSource;

	// Use this for initialization
	void Awake()
	{
		if(audSource != null)
		{
			Destroy(gameObject);
			return;
		}
		audSource = audio;
	}

	public static void Play(AudioClip bgm)
	{
		audSource.Stop();
		audSource.clip = bgm;
		audSource.Play();
	}
}
