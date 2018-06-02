using UnityEditor;
using UnityEngine;

using Sirenix.OdinInspector;
using TMPro;

/// <summary>
/// Plane Description
/// </summary>
public class ProceduralSphere : Generate
{
    #region Attributes
    [FoldoutGroup("GamePlay"), Tooltip("radius"), SerializeField]
    float radius = 1f;
    [FoldoutGroup("GamePlay"), Tooltip("longitude"), SerializeField]
    int nbLong = 24;
    [FoldoutGroup("GamePlay"), Tooltip("latitude"), SerializeField]
    int nbLat = 16;

    float _pi = Mathf.PI;
    float _2pi = Mathf.PI * 2f;
    #endregion

    #region Initialization

    protected override void InitMesh()
    {
        typeMesh = TypeMesh.Sphere;
    }
    #endregion

    #region Core
    /// <summary>
    /// calculate verticle
    /// </summary>
    private void CalculateVerticle()
    {
        verticesObject = new Vector3[(nbLong + 1) * nbLat + 2];
        float _pi = Mathf.PI;
        float _2pi = _pi * 2f;

        verticesObject[0] = Vector3.up * radius;
        for (int lat = 0; lat < nbLat; lat++)
        {
            float a1 = _pi * (float)(lat + 1) / (nbLat + 1);
            float sin1 = Mathf.Sin(a1);
            float cos1 = Mathf.Cos(a1);

            for (int lon = 0; lon <= nbLong; lon++)
            {
                float a2 = _2pi * (float)(lon == nbLong ? 0 : lon) / nbLong;
                float sin2 = Mathf.Sin(a2);
                float cos2 = Mathf.Cos(a2);

                verticesObject[lon + lat * (nbLong + 1) + 1] = new Vector3(sin1 * cos2, cos1, sin1 * sin2) * radius;
            }
        }
        verticesObject[verticesObject.Length - 1] = Vector3.up * -radius;
    }

    /// <summary>
    /// after having verticle, calculate normals of each points
    /// </summary>
    private void CalculateNormals()
    {
        normalesObject = new Vector3[verticesObject.Length];
        for (int n = 0; n < verticesObject.Length; n++)
            normalesObject[n] = verticesObject[n].normalized;
    }

    /// <summary>
    /// calculate UV of each points;
    /// </summary>
    private void CalculateUvs()
    {
        uvsObject = new Vector2[verticesObject.Length];
        uvsObject[0] = Vector2.up;
        uvsObject[uvsObject.Length - 1] = Vector2.zero;
        for (int lat = 0; lat < nbLat; lat++)
            for (int lon = 0; lon <= nbLong; lon++)
                uvsObject[lon + lat * (nbLong + 1) + 1] = new Vector2((float)lon / nbLong, 1f - (float)(lat + 1) / (nbLat + 1));
    }

    /// <summary>
    /// then save triangls of objects;
    /// </summary>
    private void CalculateTriangle()
    {
        int nbFaces = verticesObject.Length;
        int nbTriangles = nbFaces * 2;
        int nbIndexes = nbTriangles * 3;
        trianglesObject = new int[nbIndexes];

        //Top Cap
        int i = 0;
        for (int lon = 0; lon < nbLong; lon++)
        {
            trianglesObject[i++] = lon + 2;
            trianglesObject[i++] = lon + 1;
            trianglesObject[i++] = 0;
        }

        //Middle
        for (int lat = 0; lat < nbLat - 1; lat++)
        {
            for (int lon = 0; lon < nbLong; lon++)
            {
                int current = lon + lat * (nbLong + 1) + 1;
                int next = current + nbLong + 1;

                trianglesObject[i++] = current;
                trianglesObject[i++] = current + 1;
                trianglesObject[i++] = next + 1;

                trianglesObject[i++] = current;
                trianglesObject[i++] = next + 1;
                trianglesObject[i++] = next;
            }
        }

        //Bottom Cap
        for (int lon = 0; lon < nbLong; lon++)
        {
            trianglesObject[i++] = verticesObject.Length - 1;
            trianglesObject[i++] = verticesObject.Length - (lon + 2) - 1;
            trianglesObject[i++] = verticesObject.Length - (lon + 1) - 1;
        }
    }

    /// <summary>
    /// here generate the mesh...
    /// </summary>
    protected override void GenerateMesh()
    {
        Debug.Log("generate Sphere...");
        CalculateVerticle();
        CalculateNormals();
        CalculateUvs();
        CalculateTriangle();
    }
    #endregion

    #region Unity ending functions

    #endregion
}
