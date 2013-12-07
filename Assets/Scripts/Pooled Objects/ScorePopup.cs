using UnityEngine;
using System.Collections;

public class ScorePopup : PooledGameObject<ScorePopup.Params>
{
	private GUIText gt;
	public float fadeSpeed;
	public float alphaDespawnThreshold;
	private Color startColor;
	private Color endColor;
	private float transition;
	public float upVel;

	public override void Awake()
	{
		base.Awake();
		gt = guiText;
	}
	
	// Update is called once per frame
	void FixedUpdate () 
	{
		float deltat = Time.deltaTime;
		trans.position += Vector3.up * upVel * deltat;
		transition += fadeSpeed * deltat;
		gt.color = Color.Lerp(startColor, endColor, transition);
		if(gt.color.a <= alphaDespawnThreshold)
		{
			GameObjectManager.ScorePopups.Return(this);
		}
	}

	public override void Activate (Params param)
	{
		gt.text = param.text;
		startColor = endColor = param.colorMask;
		endColor.a = alphaDespawnThreshold;
		transition = 0;
	}

	public static Params SpawnParams(Color colorMask, int value)
	{
		Params p = new Params ();
		p.colorMask = colorMask;
		p.text = value.ToString ("n0");
		return p;
	}

	public class Params
	{
		public Color colorMask;
		public string text;
	}
}
