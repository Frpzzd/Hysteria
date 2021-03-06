﻿using UnityEngine;
using System.Collections;
using JamesLib;

[RequireComponent(typeof(MeshRenderer))]
public class ScrollingBackground : CachedObject {

	private Renderer rend;
	public Vector2 differential;
	private Vector2 offset;

	void Start()
	{
		rend = renderer;
	}

	// Update is called once per frame
	void Update ()
	{
		offset += differential * Time.deltaTime;
		rend.material.SetTextureOffset("_MainTex", offset);
	}
}
