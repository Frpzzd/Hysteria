using UnityEngine;
using System.Collections;

public interface IStageAction : Action
{

}

public class StageAction
{
	public class Repeat : SharedAction.Repeat<IStageAction, SharedAction.Wait>, IStageAction
	{
		//Copy of SharedAction.Repeat for StageActions
	}
}