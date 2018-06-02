using UnityEditor;
using UnityEngine;

using Sirenix.OdinInspector;
using TMPro;

/// <summary>
/// Plane Description
/// </summary>
public class ProceduralCone : Generate
{
    #region Attributes
    [FoldoutGroup("GamePlay"), Tooltip("resX"), SerializeField]
    float height = 1f;
    [FoldoutGroup("GamePlay"), Tooltip("resX"), SerializeField]
    float bottomRadius = .25f;
    [FoldoutGroup("GamePlay"), Tooltip("resX"), SerializeField]
    float topRadius = .05f;
    [FoldoutGroup("GamePlay"), Tooltip("resX"), OnValueChanged("ChangeSides"), SerializeField]
    int nbSides = 18;

    int nbHeightSeg = 1; // Not implemented yet

    int nbVerticesCap;

    int vert = 0;
    float _2pi = Mathf.PI * 2f;
    int v = 0;
    #endregion

    #region Initialization
    private void ChangeSides()
    {
        nbVerticesCap = nbSides + 1;
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
        verticesObject = new Vector3[nbVerticesCap + nbVerticesCap + nbSides * nbHeightSeg * 2 + 2];
        vert = 0;
        _2pi = Mathf.PI * 2f;

        // Bottom cap
        verticesObject[vert++] = new Vector3(0f, 0f, 0f);
        while (vert <= nbSides)
        {
            float rad = (float)vert / nbSides * _2pi;
            verticesObject[vert] = new Vector3(Mathf.Cos(rad) * bottomRadius, 0f, Mathf.Sin(rad) * bottomRadius);
            vert++;
        }

        // Top cap
        verticesObject[vert++] = new Vector3(0f, height, 0f);
        while (vert <= nbSides * 2 + 1)
        {
            float rad = (float)(vert - nbSides - 1) / nbSides * _2pi;
            verticesObject[vert] = new Vector3(Mathf.Cos(rad) * topRadius, height, Mathf.Sin(rad) * topRadius);
            vert++;
        }

        // Sides
        v = 0;
        while (vert <= verticesObject.Length - 4)
        {
            float rad = (float)v / nbSides * _2pi;
            verticesObject[vert] = new Vector3(Mathf.Cos(rad) * topRadius, height, Mathf.Sin(rad) * topRadius);
            verticesObject[vert + 1] = new Vector3(Mathf.Cos(rad) * bottomRadius, 0, Mathf.Sin(rad) * bottomRadius);
            vert += 2;
            v++;
        }
        verticesObject[vert] = verticesObject[nbSides * 2 + 2];
        verticesObject[vert + 1] = verticesObject[nbSides * 2 + 3];
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
        while (vert <= nbSides)
        {
            normalesObject[vert++] = Vector3.down;
        }

        // Top cap
        while (vert <= nbSides * 2 + 1)
        {
            normalesObject[vert++] = Vector3.up;
        }

        // Sides
        v = 0;
        while (vert <= verticesObject.Length - 4)
        {
            float rad = (float)v / nbSides * _2pi;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);

            normalesObject[vert] = new Vector3(cos, 0f, sin);
            normalesObject[vert + 1] = normalesObject[vert];

            vert += 2;
            v++;
        }
        normalesObject[vert] = normalesObject[nbSides * 2 + 2];
        normalesObject[vert + 1] = normalesObject[nbSides * 2 + 3];
    }

    /// <summary>
    /// calculate UV of each points;
    /// </summary>
    private void CalculateUvs()
    {
        uvsObject = new Vector2[verticesObject.Length];

        // Bottom cap
        int u = 0;
        uvsObject[u++] = new Vector2(0.5f, 0.5f);
        while (u <= nbSides)
        {
            float rad = (float)u / nbSides * _2pi;
            uvsObject[u] = new Vector2(Mathf.Cos(rad) * .5f + .5f, Mathf.Sin(rad) * .5f + .5f);
            u++;
        }

        // Top cap
        uvsObject[u++] = new Vector2(0.5f, 0.5f);
        while (u <= nbSides * 2 + 1)
        {
            float rad = (float)u / nbSides * _2pi;
            uvsObject[u] = new Vector2(Mathf.Cos(rad) * .5f + .5f, Mathf.Sin(rad) * .5f + .5f);
            u++;
        }

        // Sides
        int u_sides = 0;
        while (u <= uvsObject.Length - 4)
        {
            float t = (float)u_sides / nbSides;
            uvsObject[u] = new Vector3(t, 1f);
            uvsObject[u + 1] = new Vector3(t, 0f);
            u += 2;
            u_sides++;
        }
        uvsObject[u] = new Vector2(1f, 1f);
        uvsObject[u + 1] = new Vector2(1f, 0f);
    }

    /// <summary>
    /// then save triangls of objects;
    /// </summary>
    private void CalculateTriangle()
    {
        int nbTriangles = nbSides + nbSides + nbSides * 2;
        trianglesObject = new int[nbTriangles * 3 + 3];

        // Bottom cap
        int tri = 0;
        int i = 0;
        while (tri < nbSides - 1)
        {
            trianglesObject[i] = 0;
            trianglesObject[i + 1] = tri + 1;
            trianglesObject[i + 2] = tri + 2;
            tri++;
            i += 3;
        }
        trianglesObject[i] = 0;
        trianglesObject[i + 1] = tri + 1;
        trianglesObject[i + 2] = 1;
        tri++;
        i += 3;

        // Top cap
        //tri++;
        while (tri < nbSides * 2)
        {
            trianglesObject[i] = tri + 2;
            trianglesObject[i + 1] = tri + 1;
            trianglesObject[i + 2] = nbVerticesCap;
            tri++;
            i += 3;
        }

        trianglesObject[i] = nbVerticesCap + 1;
        trianglesObject[i + 1] = tri + 1;
        trianglesObject[i + 2] = nbVerticesCap;
        tri++;
        i += 3;
        tri++;

        // Sides
        while (tri <= nbTriangles)
        {
            trianglesObject[i] = tri + 2;
            trianglesObject[i + 1] = tri + 1;
            trianglesObject[i + 2] = tri + 0;
            tri++;
            i += 3;

            trianglesObject[i] = tri + 1;
            trianglesObject[i + 1] = tri + 2;
            trianglesObject[i + 2] = tri + 0;
            tri++;
            i += 3;
        }
    }

    /// <summary>
    /// here generate the mesh...
    /// </summary>
    protected override void GenerateMesh()
    {
        Debug.Log("generate Cone...");
        CalculateVerticle();
        CalculateNormals();
        CalculateUvs();
        CalculateTriangle();
    }
    #endregion

    #region Unity ending functions

    #endregion
}
