using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformExt
{
	/// <summary>
	/// Remove every childs in a transform
	/// </summary>
	public static Transform ClearChild(this Transform transform)
	{
		foreach (Transform child in transform)
		{
			GameObject.Destroy(child.gameObject);
		}
		return (transform);
	}
    public static Transform ClearChildImediat(this Transform transform)
    {
        int children = transform.childCount;

        for (int i = children - 1; i >= 0; i--)
        {
            GameObject.DestroyImmediate(transform.GetChild(i).gameObject);
        }
        return (transform);
    }

    //Even though they are used like normal methods, extension
    //methods must be declared static. Notice that the first
    //parameter has the 'this' keyword followed by a Transform
    //variable. This variable denotes which class the extension
    //method becomes a part of.
    //use: transform.ResetTransformation();
    public static void ResetTransform(this Transform trans)
    {
        trans.position = Vector3.zero;
        trans.localRotation = Quaternion.identity;
        trans.localScale = Vector3.one;
    }
    public static void SetX(this Transform transform, float x)
    {
        transform.position = new Vector3(x, transform.position.y, transform.position.z);
    }
    public static void SetXY(this Transform transform, float x, float y)
    {
        transform.position = new Vector3(x, y, transform.position.z);
    }
    public static void SetXZ(this Transform transform, float x, float z)
    {
        transform.position = new Vector3(x, transform.position.y, z);
    }
    public static void SetY(this Transform transform, float y)
    {
        transform.position = new Vector3(transform.position.x, y, transform.position.z);
    }
    public static void SetYZ(this Transform transform, float y, float z)
    {
        transform.position = new Vector3(transform.position.x, y, z);
    }
    public static void SetZ(this Transform transform, float z)
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, z);
    }

    /// <summary>
    /// Sets the position of a transform's children to zero.
    /// </summary>
    /// <param name="transform">Parent transform.</param>
    /// <param name="recursive">Also reset ancestor positions?</param>
    public static void ResetChildPositions(this Transform transform, bool recursive = false)
    {
        foreach (Transform child in transform)
        {
            child.position = Vector3.zero;

            if (recursive)
            {
                child.ResetChildPositions(recursive);
            }
        }
    }

    /// <summary>
    /// Makes the given game objects children of the transform.
    /// </summary>
    /// <param name="transform">Parent transform.</param>
    /// <param name="children">Game objects to make children.</param>
    public static void AddChildren(this Transform transform, GameObject[] children)
    {
        Array.ForEach(children, child => child.transform.parent = transform);
    }
    public static void AddChildren(this Transform transform, Component[] children)
    {
        Array.ForEach(children, child => child.transform.parent = transform);
    }
}
