using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DanmakuEngine.Core;
using DanmakuEngine.Actions;

[System.Serializable]
public class BossGUI : StaticGameObject<BossGUI>
{
	public GameObject container;
	public Transform currentHPBar;
	public GUIText timeout;
	public GUIText bossName;
	public GUIText attackName;
	public GameObject additionalHpBarPrefab;
	public GameObject adiitionalHpBarContainer;
	public GUIText splashName;
	public GUITexture splashLine;
	public GUIText splashTitle;
	public Color splashColor;
	public float splashTime;
	public float slideDownStart;
	public AudioClip BossDeath;

	private List<GameObject> additionalLifeBars;

	void Start()
	{
		container.SetActive (false);
		splashLine.enabled = false;
		splashName.enabled = false;
		splashTitle.enabled = false;
	}

	public IEnumerator BossBattle(Boss boss, AbstractStage stage)
	{
		float deltat = Time.fixedDeltaTime;
		float lerpValue = 0f;
		Color finalColor = splashColor, intermediate;
		finalColor.a = 0f;
		Vector3 startVector = slideDownStart * Vector3.up;
		Vector3 bossStart = boss.Transform.position;
		Vector3 bossEnd = boss.startVector;
		container.transform.localPosition = startVector;
		container.SetActive(true);
		bossName.text = splashName.text = boss.Name;
		splashTitle.text = boss.Title;
		splashLine.enabled = true;
		splashName.enabled = true;
		splashTitle.enabled = true;
		splashLine.color = splashColor;
		splashName.color = splashColor;
		splashTitle.color = splashColor;
		foreach(AbstractEnemy enemy in GameObject.FindObjectsOfType<AbstractEnemy>())
		{
			if(enemy != boss)
			{
				enemy.Die();
			}
		}
		SoundManager.PlayMusic (boss.theme);
		IEnumerator pause;
		while(lerpValue <= 1f)
		{
			pause = Global.WaitForUnpause();
			while(pause.MoveNext())
			{
				yield return pause.Current;
			}
			intermediate = Color.Lerp(splashColor, finalColor, lerpValue);
			container.transform.localPosition = Vector3.Lerp(startVector, Vector3.zero, lerpValue);
			boss.Transform.position = Vector3.Lerp(bossStart, bossEnd, lerpValue);
			splashLine.color = intermediate;
			splashName.color = intermediate;
			splashTitle.color = intermediate;
			yield return new WaitForFixedUpdate();
			lerpValue += deltat / splashTime;
		}
		boss.Spawn ();
		while(!boss.Dead)
		{
			if(boss.currentAttackPattern != null)
			{
				attackName.text = ((boss.currentAttackPattern.Title != "") ? boss.currentAttackPattern.Title + " : "  : "") + boss.currentAttackPattern.Name;
				Vector3 hpScale = currentHPBar.localScale;
				hpScale.x = (float)boss.currentAttackPattern.currentHealth / (float)boss.currentAttackPattern.health * 0.99f;
				currentHPBar.localScale = hpScale;
				
				timeout.text = boss.currentAttackPattern.remainingTime.ToString("0.00");
			}
			yield return new WaitForEndOfFrame();
		}
		//SoundManager.PlaySoundEffect(BossDeath);
		splashLine.enabled = false;
		splashName.enabled = false;
		splashTitle.enabled = false;
		container.SetActive(false);
	}
}
