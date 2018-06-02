using UnityEditor;
using UnityEngine;

using Sirenix.OdinInspector;
using TMPro;

/// <summary>
/// Plane Description
/// </summary>
public class ProceduralPlane : Generate
{
    #region Attributes
    [FoldoutGroup("GamePlay"), Tooltip("Length"), SerializeField]
    private float length = 1f;
    [FoldoutGroup("GamePlay"), Tooltip("width"), SerializeField]
    private float width = 1f;
    [FoldoutGroup("GamePlay"), Range(2, 100), OnValueChanged("ChangeRes"), Tooltip("resX"), SerializeField]
    private int res = 2;


    private int resX = 2; // 2 minimum
    private int resZ = 2;
    #endregion

    #region Initialization
    private void ChangeRes()
    {
        resX = resZ = res;
    }

    protected override void InitMesh()
    {
        typeMesh = TypeMesh.Plane;
    }
    #endregion

    #region Core
    /// <summary>
    /// calculate verticle
    /// </summary>
    private void CalculateVerticle()
    {
        verticesObject = new Vector3[resX * resZ];    //setup size of verticle

        for (int z = 0; z < resZ; z++)          //loop thought all vecticle
        {
            // [ -length / 2, length / 2 ]
            float zPos = ((float)z / (resZ - 1) - .5f) * length;
            for (int x = 0; x < resX; x++)
            {
                // [ -width / 2, width / 2 ]
                float xPos = ((float)x / (resX - 1) - .5f) * width;
                verticesObject[x + z * resX] = new Vector3(xPos, 0f, zPos);
            }
        }
    }

    /// <summary>
    /// after having verticle, calculate normals of each points
    /// </summary>
    private void CalculateNormals()
    {
        normalesObject = new Vector3[verticesObject.Length];
        for (int n = 0; n < normalesObject.Length; n++)
            normalesObject[n] = Vector3.up;
    }

    /// <summary>
    /// calculate UV of each points;
    /// </summary>
    private void CalculateUvs()
    {
        uvsObject = new Vector2[verticesObject.Length];
        for (int v = 0; v < resZ; v++)
        {
            for (int u = 0; u < resX; u++)
            {
                uvsObject[u + v * resX] = new Vector2((float)u / (resX - 1), (float)v / (resZ - 1));
            }
        }
    }

    /// <summary>
    /// then save triangls of objects;
    /// </summary>
    private void CalculateTriangle()
    {
        int nbFaces = (resX - 1) * (resZ - 1);
        trianglesObject = new int[nbFaces * 6];
        int t = 0;
        for (int face = 0; face < nbFaces; face++)
        {
            // Retrieve lower left corner from face ind
            int i = face % (resX - 1) + (face / (resZ - 1) * resX);

            trianglesObject[t++] = i + resX;
            trianglesObject[t++] = i + 1;
            trianglesObject[t++] = i;

            trianglesObject[t++] = i + resX;
            trianglesObject[t++] = i + resX + 1;
            trianglesObject[t++] = i + 1;
        }
    }

    /// <summary>
    /// here generate the mesh...
    /// </summary>
    protected override void GenerateMesh()
    {
        Debug.Log("generate plane...");
        CalculateVerticle();
        CalculateNormals();
        CalculateUvs();
        CalculateTriangle();
    }
    #endregion

    #region Unity ending functions

    #endregion
}
