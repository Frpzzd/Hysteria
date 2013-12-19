using UnityEngine;

public abstract class PlayerHitbox : MonoBehaviour
{
	protected Player master;
	
	void Awake()
	{
		master = transform.parent.gameObject.GetComponent<Player> ();
	}
}

