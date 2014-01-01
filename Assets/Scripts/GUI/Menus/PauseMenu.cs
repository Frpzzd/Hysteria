using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(Plane))]
[RequireComponent(typeof(MeshRenderer))]
public class PauseMenu : CachedObject
{
	[SerializeField]
	public AudioClip pauseClip;
	[SerializeField]
	public GameObject[] activeOnPause;
	[SerializeField]
	public GameObject[] inactiveOnPause;
	[NonSerialized]
	private float cachedTimeScale;
	[NonSerialized]
	private MeshRenderer render;

	public override void Awake()
	{
		base.Awake ();
		render = GameObject.GetComponent<MeshRenderer> ();
	}

	void Update()
	{
		if(Input.GetButtonDown("Pause"))
		{
			bool paused = Global.GameState == GameState.Paused;
			render.enabled = !	paused;
			if(!paused)
			{
				cachedTimeScale = Time.timeScale;
				Time.timeScale = 0f;
				SoundManager.PlaySoundEffect (pauseClip, 1f, Vector3.zero, false);
				SoundManager.PauseMusic ();
				Global.GameStateChange(GameState.Paused);
			}
			else
			{
				Time.timeScale = cachedTimeScale;
				SoundManager.UnpauseMusic ();
				Global.GameStateChange(GameState.InGame);
			}
		}
	}

	private void Toggle(bool value)
	{
	}
}
