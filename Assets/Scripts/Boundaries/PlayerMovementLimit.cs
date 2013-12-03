using UnityEngine;
using System.Collections;

public class PlayerMovementLimit : MonoBehaviour 
{
	public int limitIndex;

	void OnTriggerEnter2D(Collider2D other)
	{
		Player p = other.gameObject.GetComponent<Player> ();
		if(p != null)
		{
			p.atMovementLimit[limitIndex] = true;
		}
	}

	void OnTriggerExit2D(Collider2D other)
	{
		Player p = other.gameObject.GetComponent<Player> ();
		if(p != null)
		{
			p.atMovementLimit[limitIndex] = false;
		}
	}
}
