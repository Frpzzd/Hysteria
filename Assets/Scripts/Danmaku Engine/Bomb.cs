using UnityEngine;
using System.Collections;
using DanmakuEngine.Core;
using JamesLib;

[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Bomb : CachedObject
{
	public float upwardMove;
	public float rotations;
	public float maxScale;
	public AudioClip bombClip;

	public override void Awake()
	{
		base.Awake ();
		//hideFlags = HideFlags.HideInHierarchy;
	}

	public IEnumerator UseBomb(Transform location, float duration, Player parent)
	{
		Debug.Log ("Bomb Deployed");
		float lerpValue = 0.0f;
		float deltat = Time.fixedDeltaTime;

		Transform.position = location.position;
		Vector3 originalPosition = Transform.position;
		Vector3 targetPosition = originalPosition + upwardMove * Vector3.up;

		Transform.localScale = Vector3.zero;
		Vector3 targetScale = Vector3.one * maxScale;

		SpriteRenderer rend = GetComponent<SpriteRenderer> ();
		Color referenceColor = rend.color;
		referenceColor.a = 0f;
		Color targetColor = referenceColor;
		referenceColor.a = 1f;
		rend.color = referenceColor;

		collider2D.enabled = true;
		rend.enabled = true;
		parent.bombDeployed = true;
		parent.invincible = true;

		SoundManager.PlaySoundEffect (bombClip);

		while(lerpValue <= 1f)
		{
			IEnumerator pause = Global.WaitForUnpause();
			while(pause.MoveNext())
			{
				yield return pause.Current;
			}
			yield return new WaitForFixedUpdate();
			lerpValue += deltat / duration;
			Transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, (lerpValue == 0f) ? 0f : Mathf.Log(lerpValue) * 0.5f + 1f);
			Transform.position = Vector3.Lerp(originalPosition, targetPosition, Mathf.Pow(lerpValue, 6f));
			rend.color = Color.Lerp(referenceColor, targetColor, Mathf.Pow(lerpValue, 6f));
		}
		parent.bombDeployed = false;
		parent.invincible = false;
		Destroy (GameObject);
	}

	void OnTriggerEnter2D(Collider2D col)
	{
		Debug.Log ("Bullet Canceled");
		Bullet bullet = col.gameObject.GetComponent<Bullet> ();
		if(bullet != null)
		{
			bullet.Cancel();
		}
	}
}
