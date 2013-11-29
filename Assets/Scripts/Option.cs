using UnityEngine;
using System.Collections;

public class Option : MonoBehaviour 
{
	[HideInInspector]
	public Transform trans;
	[HideInInspector]
	public GameObject gamObj;

	// Use this for initialization
	void Start () 
	{
		trans = transform;
		gamObj = gameObject;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
