using System;
using UnityEngine;

public static class Extensions
{
	public static Vector2 XY(this Vector3 v)
	{
		return new Vector2(v.x, v.y);
	}

	public static Vector2 XZ(this Vector3 v)
	{
		return new Vector2(v.x, v.z);
	}

	public static Vector2 YZ(this Vector3 v)
	{
		return new Vector2(v.y, v.z);
	}

	public static Vector3 ToVector3(this Vector2 v)
	{
		return new Vector3(v.x, v.y, 0f);
	}
}
