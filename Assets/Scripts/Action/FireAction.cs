using UnityEngine;
using System;
using System.Collections;

public interface IFireAction : Action
{
}

public abstract class FireAction : AttackPatternAction, IFireAction
{
	//	public enum Type { Wait, Fire, CallFireTag, Repeat }

	public class Repeat : SharedAction.Repeat<IFireAction, SharedAction.Wait>, IFireAction
	{

	}
}
