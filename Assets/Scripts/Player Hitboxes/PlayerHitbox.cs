using UnityEngine;

public abstract class PlayerHitbox : MonoBehaviour
{
	protected Player master;
	
	public virtual void Start()
	{
		master = transform.parent.gameObject.GetComponent<Player> ();
	}
}

