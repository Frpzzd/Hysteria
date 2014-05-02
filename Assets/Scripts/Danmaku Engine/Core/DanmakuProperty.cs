using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class DanmakuProperty
{
	public float[] Lower;
	public float[] Upper;

	public DanmakuProperty()
	{
		Lower = new float[Enum.GetNames (typeof(Rank)).Length];
		Upper = new float[Enum.GetNames (typeof(Rank)).Length];
	}

	private static Dictionary<Rank, int> indexes;

	static DanmakuProperty()
	{
		indexes = new Dictionary<Rank, int> ();
		Rank[] values = (Rank[])Enum.GetValues (typeof(Rank));
		for(int i = 0; i < values.Length; i++)
		{
			indexes.Add(values[i], i); 
		}
	}

	public bool rank = false;
	public bool random = false;
	public bool sequence = false;

	public float Value
	{
		get 
		{
			int index = indexes[Global.Rank];
			if(random)
			{
				return UnityEngine.Random.Range(Lower[index], Upper[index]);
			}
			else
			{
				return Lower[index];
			}
		}
	}
}

public class DanmakuPropertyAttribute : PropertyAttribute
{
	public string name;
	public bool isInt;
	public bool useSequence;

	public DanmakuPropertyAttribute(string name, bool isInt, bool useSequence)
	{
		this.name = name;
		this.isInt = isInt;
		this.useSequence = useSequence;
	}
}

