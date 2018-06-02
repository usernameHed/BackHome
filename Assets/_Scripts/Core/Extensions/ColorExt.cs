using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ColorExt
{
    /// <summary>
    /// get une string et essai de renvoyer une couleur à partir de cette string...
    /// </summary>
    public static Color ColorHTML(string color)
    {
        Color newCol;
        ColorUtility.TryParseHtmlString(color, out newCol);
        return (newCol);
    }

    /// <summary>
    /// renvoi une couoleur basé sur des parametre RGBA en 0-255
    /// </summary>
    public static Color Color255(float r, float g, float b, float a)
    {
        return new Color(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);
    }

    /// <summary>
    /// only change the alpha of a color
    /// GUI.color = desiredColor.WithAlpha(currentAlpha);
    /// </summary>
    public static Color WithAlpha(this Color color, float alpha)
    {
        return new Color(color.r, color.g, color.b, alpha);
    }
}
