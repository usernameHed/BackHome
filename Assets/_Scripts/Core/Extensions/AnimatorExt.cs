using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AnimatorExt
{
	/// <summary>
	/// Fetch an animation from animator
	/// </summary>
	public static AnimationClip GetAnimationClipFromAnimatorByName(this Animator animator, string name)
	{
		//Get the animation name from the animator
		for (int i = 0; i < animator.runtimeAnimatorController.animationClips.Length; i++)
		{
			if (animator.runtimeAnimatorController.animationClips [i].name == name)
			{
				return animator.runtimeAnimatorController.animationClips [i];
			}
		}

		Debug.LogError("Animation clip: " + name + " not found");
		return null;
	}
}
