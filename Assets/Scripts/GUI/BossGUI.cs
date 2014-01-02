using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossGUI : StaticGameObject<BossGUI>
{
	public Enemy boss;

	public GameObject container;
	public Transform currentHPBar;
	public GUIText timeout;
	public GUIText bossName;
	public GameObject additionalHpBarPrefab;
	public GameObject adiitionalHpBarContainer;
	public GUIText splashName;
	public GUITexture splashLine;
	public GUIText splashTitle;
	public Color splashColor;
	public float splashTime;
	public float slideDownStart;

	private List<GameObject> additionalLifeBars;

	void Start()
	{
		container.SetActive (false);
		splashLine.enabled = false;
		splashName.enabled = false;
		splashTitle.enabled = false;
	}

	void Update()
	{
		if(boss == null)
		{
			container.SetActive(false);
		}
		else
		{
			if(boss.currentAttackPattern != null)
			{
				Vector3 hpScale = currentHPBar.localScale;
				hpScale.x = boss.currentAttackPattern.currentHealth / boss.currentAttackPattern.health;
				currentHPBar.localScale = hpScale;

				timeout.text = boss.remainingTime.ToString("00.00");
			}
			else
			{
				currentHPBar.gameObject.SetActive(false);
			}
		}
	}

	public IEnumerator StartBossBattle(Enemy boss)
	{
		if(!boss.boss)
		{
			throw new System.InvalidOperationException("Cannot start a boss battle with a normal enemy as a boss");
		}
		else
		{
			this.boss = boss;
			float deltat = Time.fixedDeltaTime;
			float lerpValue = 0f;
			Color finalColor = splashColor, intermediate;
			finalColor.a = 0f;
			Vector3 startVector = slideDownStart * Vector3.up;
			container.transform.localPosition = startVector;
			container.SetActive(true);
			bossName.text = splashName.text = boss.Name;
			splashTitle.text = boss.Title;
			splashLine.enabled = true;
			splashName.enabled = true;
			splashTitle.enabled = true;
			while(lerpValue <= 1f)
			{
				yield return StartCoroutine (Global.WaitForUnpause());
				intermediate = Color.Lerp(splashColor, finalColor, lerpValue);
				container.transform.localPosition = Vector3.Lerp(startVector, Vector3.zero, lerpValue);
				splashLine.color = intermediate;
				splashName.color = intermediate;
				splashTitle.color = intermediate;
				yield return new WaitForFixedUpdate();
				lerpValue += deltat / splashTime;
			}
			splashLine.enabled = false;
			splashName.enabled = false;
			splashTitle.enabled = false;
			container.SetActive(false);
		}
	}
}
