using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ComponentExt
{
    /// <summary>
    /// Gets or add a component. Usage example:
    /// BoxCollider boxCollider = transform.GetOrAddComponent<BoxCollider>();
    /// </summary>
    static public T GetOrAddComponent<T>(this Component child) where T : Component
    {
        T result = child.GetComponent<T>();
        if (result == null)
        {
            result = child.gameObject.AddComponent<T>();
        }
        return result;
    }

    /// <summary>
    /// Checks whether a component's game object has a component of type T attached.
    /// </summary>
    /// <param name="component">Component.</param>
    /// <returns>True when component is attached.</returns>
    public static bool HasComponent<T>(this Component component) where T : Component
    {
        return component.GetComponent<T>() != null;
    }
    /// <summary>
    /// Checks whether a game object has a component of type T attached.
    /// </summary>
    /// <param name="gameObject">Game object.</param>
    /// <returns>True when component is attached.</returns>
    public static bool HasComponent<T>(this GameObject gameObject) where T : Component
    {
        return gameObject.GetComponent<T>() != null;
    }

    /// <summary>
    /// Détruit le component s'il est présent
    /// </summary>
    /// <typeparam name="T">component à détruire</typeparam>
    /// <param name="child">component à détruire</param>
    /// <returns>retourne vrai si il a été trouvé et détruit</returns>
    static public bool DestroyComponent<T>(this Component child) where T : Component
    {
        T result = child.GetComponent<T>();
        if (result != null)
        {
            GameObject.Destroy(child.gameObject.GetComponent<T>());
            return (true);
        }
        return false;
    }

    /// <summary>
    /// GameObject.GetComponentsInChildren(), only with a certain tag
    /// use: m_renderers = gameObject.GetComponentsInChildrenWithTag<Renderer>("Shield");
    /// </summary>
    public static T[] GetComponentsInChildrenWithTag<T>(this GameObject gameObject, string tag)
        where T : Component
    {
        List<T> results = new List<T>();

        if (gameObject.CompareTag(tag))
            results.Add(gameObject.GetComponent<T>());

        foreach (Transform t in gameObject.transform)
            results.AddRange(t.gameObject.GetComponentsInChildrenWithTag<T>(tag));

        return results.ToArray();
    }

    /// <summary>
    /// Get a script on the parent
    /// Player player = collider.gameObject.GetComponentInParents<Player>();
    /// </summary>
    public static T GetComponentInParents<T>(this GameObject gameObject)
        where T : Component
    {
        for (Transform t = gameObject.transform; t != null; t = t.parent)
        {
            T result = t.GetComponent<T>();
            if (result != null)
                return result;
        }

        return null;
    }
    public static T[] GetComponentsInParents<T>(this GameObject gameObject)
        where T : Component
    {
        List<T> results = new List<T>();
        for (Transform t = gameObject.transform; t != null; t = t.parent)
        {
            T result = t.GetComponent<T>();
            if (result != null)
                results.Add(result);
        }

        return results.ToArray();
    }

}
