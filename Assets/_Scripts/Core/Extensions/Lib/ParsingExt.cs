using UnityEngine;
using System.IO;
using System;
using System.Text.RegularExpressions;
using System.Text;
using System.Collections.Generic;

/// <summary>
/// Fonctions utile
/// <summary>
public static class ParsingExt
{
    #region core script
    
    /// <summary>
    /// si TOTO532, retourne "532"
    /// </summary>
    public static int GetLastNumberFromString(string lastNNumber)
    {
        var x = Regex.Match(lastNNumber, @"([0-9]+)[^0-9]*$");

        if (x.Success && x.Groups.Count > 0)
        {
            int foundNumber = Int32.Parse(x.Groups[1].Captures[0].Value);
            return (foundNumber);
        }
        return (0);
    }

    /// <summary>
    /// prend en parametre un fileName, et renvoi le prochain numéro
    /// </summary>
    public static string GetNextFileName(string fileName)
    {
        string extension = Path.GetExtension(fileName);
        string pathName = Path.GetDirectoryName(fileName);
        string fileNameOnly = Path.Combine(pathName, Path.GetFileNameWithoutExtension(fileName));
        int i = 0;
        // If the file exists, keep trying until it doesn't
        while (File.Exists(fileName))
        {
            i += 1;
            fileName = string.Format("{0}({1}){2}", fileNameOnly, i, extension);
        }
        return fileName.Replace("\\", "/");
    }

    /// <summary>
    /// prend en parametre une liste d'enum, et renvoi un array de string de ces enum !
    /// </summary>
    /// <param name="layers"></param>
    /// <returns></returns>
    public static string[] GetStringsFromEnum<T>(T[] layers)
    {
        string[] toPush = new string[layers.Length];
        for (int i = 0; i < toPush.Length; i++)
        {
            toPush[i] = layers[i].ToString();
        }
        return (toPush);
    }
    public static string[] GetStringsFromEnum<T>(List<T> layers)
    {
        string[] toPush = new string[layers.Count];
        for (int i = 0; i < toPush.Length; i++)
        {
            toPush[i] = layers[i].ToString();
        }
        return (toPush);
    }

    /// <summary>
    /// Converts a string to bytes, in a Unity friendly way
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static byte[] UnityStringToBytes(this string source)
    {
        // exit if null
        if (string.IsNullOrEmpty(source))
            return null;

        // convert to bytes
        using (MemoryStream compMemStream = new MemoryStream())
        {
            using (StreamWriter writer = new StreamWriter(compMemStream, Encoding.UTF8))
            {
                writer.Write(source);
                writer.Close();

                return compMemStream.ToArray();
            }
        }
    }

    /// <summary>
    /// Converts a byte array to a Unicode string, in a Unity friendly way
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static string UnityBytesToString(this byte[] source)
    {
        // exit if null
        if (source.IsNullOrEmpty())
            return string.Empty;

        // read from bytes
        using (MemoryStream compMemStream = new MemoryStream(source))
        {
            using (StreamReader reader = new StreamReader(compMemStream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }

    /// <summary>
    /// prend un string, et renvoi la chaine tronqué
    /// </summary>
    /// <param name="value"></param>
    /// <param name="maxLength"></param>
    /// <returns></returns>
    public static string Truncate(this string value, int start, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
            return value;
        return (value.Substring(start, maxLength));
    }
    #endregion
}
