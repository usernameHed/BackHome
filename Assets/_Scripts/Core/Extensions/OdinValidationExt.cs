using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// OdinValidationExt Description
/// </summary>
public static class OdinValidationExt
{
    public static bool GameObjectValid(GameObject value)
    {
        if (value != null)
            return (true);
        return false;
    }

    public static bool ScriptValid<T>(T value)
    {
        if (EqualityComparer<T>.Default.Equals(value, default(T)))
        {
            return (true);
        }
        return false;
    }
}
