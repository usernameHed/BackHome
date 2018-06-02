using UnityEditor;
using UnityEngine;

using Sirenix.OdinInspector;
using TMPro;

/// <summary>
/// Plane Description
/// </summary>
public class ProceduralTube : Generate
{
    #region Attributes
    [FoldoutGroup("GamePlay"), Tooltip("resX"), SerializeField]
    float height = 1f;
    [FoldoutGroup("GamePlay"), Tooltip("resX"), OnValueChanged("ChangeSides"), SerializeField]
    int nbSides = 24;

    // Outter shell is at radius1 + radius2 / 2, inner shell at radius1 - radius2 / 2
    [FoldoutGroup("GamePlay"), Tooltip("resX"), SerializeField]
    float bottomRadius1 = .5f;
    [FoldoutGroup("GamePlay"), Tooltip("resX"), SerializeField]
    float bottomRadius2 = .15f;
    [FoldoutGroup("GamePlay"), Tooltip("resX"), SerializeField]
    float topRadius1 = .5f;
    [FoldoutGroup("GamePlay"), Tooltip("resX"), SerializeField]
    float topRadius2 = .15f;

    int nbVerticesCap;
    int nbVerticesSides;

    int vert = 0;
    float _2pi = Mathf.PI * 2f;
    int sideCounter = 0;
    //int v = 0;
    #endregion

    #region Initialization
    private void ChangeSides()
    {
        nbVerticesCap = nbSides * 2 + 2;
        nbVerticesSides = nbSides * 2 + 2;
    }

    protected override void InitMesh()
    {
        typeMesh = TypeMesh.Cone;
        ChangeSides();
    }
    #endregion

    #region Core
    /// <summary>
    /// calculate verticle
    /// </summary>
    private void CalculateVerticle()
    {
        // bottom + top + sides
        verticesObject = new Vector3[nbVerticesCap * 2 + nbVerticesSides * 2];
        vert = 0;
        _2pi = Mathf.PI * 2f;

        // Bottom cap
        sideCounter = 0;
        while (vert < nbVerticesCap)
        {
            sideCounter = sideCounter == nbSides ? 0 : sideCounter;

            float r1 = (float)(sideCounter++) / nbSides * _2pi;
            float cos = Mathf.Cos(r1);
            float sin = Mathf.Sin(r1);
            verticesObject[vert] = new Vector3(cos * (bottomRadius1 - bottomRadius2 * .5f), 0f, sin * (bottomRadius1 - bottomRadius2 * .5f));
            verticesObject[vert + 1] = new Vector3(cos * (bottomRadius1 + bottomRadius2 * .5f), 0f, sin * (bottomRadius1 + bottomRadius2 * .5f));
            vert += 2;
        }

        // Top cap
        sideCounter = 0;
        while (vert < nbVerticesCap * 2)
        {
            sideCounter = sideCounter == nbSides ? 0 : sideCounter;

            float r1 = (float)(sideCounter++) / nbSides * _2pi;
            float cos = Mathf.Cos(r1);
            float sin = Mathf.Sin(r1);
            verticesObject[vert] = new Vector3(cos * (topRadius1 - topRadius2 * .5f), height, sin * (topRadius1 - topRadius2 * .5f));
            verticesObject[vert + 1] = new Vector3(cos * (topRadius1 + topRadius2 * .5f), height, sin * (topRadius1 + topRadius2 * .5f));
            vert += 2;
        }

        // Sides (out)
        sideCounter = 0;
        while (vert < nbVerticesCap * 2 + nbVerticesSides)
        {
            sideCounter = sideCounter == nbSides ? 0 : sideCounter;

            float r1 = (float)(sideCounter++) / nbSides * _2pi;
            float cos = Mathf.Cos(r1);
            float sin = Mathf.Sin(r1);

            verticesObject[vert] = new Vector3(cos * (topRadius1 + topRadius2 * .5f), height, sin * (topRadius1 + topRadius2 * .5f));
            verticesObject[vert + 1] = new Vector3(cos * (bottomRadius1 + bottomRadius2 * .5f), 0, sin * (bottomRadius1 + bottomRadius2 * .5f));
            vert += 2;
        }

        // Sides (in)
        sideCounter = 0;
        while (vert < verticesObject.Length)
        {
            sideCounter = sideCounter == nbSides ? 0 : sideCounter;

            float r1 = (float)(sideCounter++) / nbSides * _2pi;
            float cos = Mathf.Cos(r1);
            float sin = Mathf.Sin(r1);

            verticesObject[vert] = new Vector3(cos * (topRadius1 - topRadius2 * .5f), height, sin * (topRadius1 - topRadius2 * .5f));
            verticesObject[vert + 1] = new Vector3(cos * (bottomRadius1 - bottomRadius2 * .5f), 0, sin * (bottomRadius1 - bottomRadius2 * .5f));
            vert += 2;
        }
    }

    /// <summary>
    /// after having verticle, calculate normals of each points
    /// </summary>
    private void CalculateNormals()
    {
        // bottom + top + sides
        normalesObject = new Vector3[verticesObject.Length];
        vert = 0;

        // Bottom cap
        while (vert < nbVerticesCap)
        {
            normalesObject[vert++] = Vector3.down;
        }

        // Top cap
        while (vert < nbVerticesCap * 2)
        {
            normalesObject[vert++] = Vector3.up;
        }

        // Sides (out)
        sideCounter = 0;
        while (vert < nbVerticesCap * 2 + nbVerticesSides)
        {
            sideCounter = sideCounter == nbSides ? 0 : sideCounter;

            float r1 = (float)(sideCounter++) / nbSides * _2pi;

            normalesObject[vert] = new Vector3(Mathf.Cos(r1), 0f, Mathf.Sin(r1));
            normalesObject[vert + 1] = normalesObject[vert];
            vert += 2;
        }

        // Sides (in)
        sideCounter = 0;
        while (vert < verticesObject.Length)
        {
            sideCounter = sideCounter == nbSides ? 0 : sideCounter;

            float r1 = (float)(sideCounter++) / nbSides * _2pi;

            normalesObject[vert] = -(new Vector3(Mathf.Cos(r1), 0f, Mathf.Sin(r1)));
            normalesObject[vert + 1] = normalesObject[vert];
            vert += 2;
        }
    }

    /// <summary>
    /// calculate UV of each points;
    /// </summary>
    private void CalculateUvs()
    {
        uvsObject = new Vector2[verticesObject.Length];

        vert = 0;
        // Bottom cap
        sideCounter = 0;
        while (vert < nbVerticesCap)
        {
            float t = (float)(sideCounter++) / nbSides;
            uvsObject[vert++] = new Vector2(0f, t);
            uvsObject[vert++] = new Vector2(1f, t);
        }

        // Top cap
        sideCounter = 0;
        while (vert < nbVerticesCap * 2)
        {
            float t = (float)(sideCounter++) / nbSides;
            uvsObject[vert++] = new Vector2(0f, t);
            uvsObject[vert++] = new Vector2(1f, t);
        }

        // Sides (out)
        sideCounter = 0;
        while (vert < nbVerticesCap * 2 + nbVerticesSides)
        {
            float t = (float)(sideCounter++) / nbSides;
            uvsObject[vert++] = new Vector2(t, 0f);
            uvsObject[vert++] = new Vector2(t, 1f);
        }

        // Sides (in)
        sideCounter = 0;
        while (vert < verticesObject.Length)
        {
            float t = (float)(sideCounter++) / nbSides;
            uvsObject[vert++] = new Vector2(t, 0f);
            uvsObject[vert++] = new Vector2(t, 1f);
        }
    }

    /// <summary>
    /// then save triangls of objects;
    /// </summary>
    private void CalculateTriangle()
    {
        int nbFace = nbSides * 4;
        int nbTriangles = nbFace * 2;
        int nbIndexes = nbTriangles * 3;
        trianglesObject = new int[nbIndexes];

        // Bottom cap
        int i = 0;
        sideCounter = 0;
        while (sideCounter < nbSides)
        {
            int current = sideCounter * 2;
            int next = sideCounter * 2 + 2;

            trianglesObject[i++] = next + 1;
            trianglesObject[i++] = next;
            trianglesObject[i++] = current;

            trianglesObject[i++] = current + 1;
            trianglesObject[i++] = next + 1;
            trianglesObject[i++] = current;

            sideCounter++;
        }

        // Top cap
        while (sideCounter < nbSides * 2)
        {
            int current = sideCounter * 2 + 2;
            int next = sideCounter * 2 + 4;

            trianglesObject[i++] = current;
            trianglesObject[i++] = next;
            trianglesObject[i++] = next + 1;

            trianglesObject[i++] = current;
            trianglesObject[i++] = next + 1;
            trianglesObject[i++] = current + 1;

            sideCounter++;
        }

        // Sides (out)
        while (sideCounter < nbSides * 3)
        {
            int current = sideCounter * 2 + 4;
            int next = sideCounter * 2 + 6;

            trianglesObject[i++] = current;
            trianglesObject[i++] = next;
            trianglesObject[i++] = next + 1;

            trianglesObject[i++] = current;
            trianglesObject[i++] = next + 1;
            trianglesObject[i++] = current + 1;

            sideCounter++;
        }


        // Sides (in)
        while (sideCounter < nbSides * 4)
        {
            int current = sideCounter * 2 + 6;
            int next = sideCounter * 2 + 8;

            trianglesObject[i++] = next + 1;
            trianglesObject[i++] = next;
            trianglesObject[i++] = current;

            trianglesObject[i++] = current + 1;
            trianglesObject[i++] = next + 1;
            trianglesObject[i++] = current;

            sideCounter++;
        }
    }

    /// <summary>
    /// here generate the mesh...
    /// </summary>
    protected override void GenerateMesh()
    {
        Debug.Log("generate Tube...");
        CalculateVerticle();
        CalculateNormals();
        CalculateUvs();
        CalculateTriangle();
    }
    #endregion

    #region Unity ending functions

    #endregion
}
