using UnityEditor;
using UnityEngine;

using Sirenix.OdinInspector;
using TMPro;

/// <summary>
/// Plane Description
/// </summary>
public class ProceduralTorus : Generate
{
    #region Attributes
    [FoldoutGroup("GamePlay"), Tooltip("resX"), SerializeField]
    float radius1 = 1f;
    [FoldoutGroup("GamePlay"), Tooltip("resX"), SerializeField]
    float radius2 = .3f;
    [FoldoutGroup("GamePlay"), Tooltip("resX"), SerializeField]
    int nbRadSeg = 24;
    [FoldoutGroup("GamePlay"), Tooltip("resX"), SerializeField]
    int nbSides = 18;

    float _2pi = Mathf.PI * 2f;

    //int v = 0;
    #endregion

    #region Initialization

    protected override void InitMesh()
    {
        typeMesh = TypeMesh.Cone;
    }
    #endregion

    #region Core
    /// <summary>
    /// calculate verticle
    /// </summary>
    private void CalculateVerticle()
    {
        verticesObject = new Vector3[(nbRadSeg + 1) * (nbSides + 1)];
        _2pi = Mathf.PI * 2f;
        for (int seg = 0; seg <= nbRadSeg; seg++)
        {
            int currSeg = seg == nbRadSeg ? 0 : seg;

            float t1 = (float)currSeg / nbRadSeg * _2pi;
            Vector3 r1 = new Vector3(Mathf.Cos(t1) * radius1, 0f, Mathf.Sin(t1) * radius1);

            for (int side = 0; side <= nbSides; side++)
            {
                int currSide = side == nbSides ? 0 : side;

                Vector3 normale = Vector3.Cross(r1, Vector3.up);
                float t2 = (float)currSide / nbSides * _2pi;
                Vector3 r2 = Quaternion.AngleAxis(-t1 * Mathf.Rad2Deg, Vector3.up) * new Vector3(Mathf.Sin(t2) * radius2, Mathf.Cos(t2) * radius2);

                verticesObject[side + seg * (nbSides + 1)] = r1 + r2;
            }
        }
    }

    /// <summary>
    /// after having verticle, calculate normals of each points
    /// </summary>
    private void CalculateNormals()
    {
        normalesObject = new Vector3[verticesObject.Length];
        for (int seg = 0; seg <= nbRadSeg; seg++)
        {
            int currSeg = seg == nbRadSeg ? 0 : seg;

            float t1 = (float)currSeg / nbRadSeg * _2pi;
            Vector3 r1 = new Vector3(Mathf.Cos(t1) * radius1, 0f, Mathf.Sin(t1) * radius1);

            for (int side = 0; side <= nbSides; side++)
            {
                normalesObject[side + seg * (nbSides + 1)] = (verticesObject[side + seg * (nbSides + 1)] - r1).normalized;
            }
        }
    }

    /// <summary>
    /// calculate UV of each points;
    /// </summary>
    private void CalculateUvs()
    {
        uvsObject = new Vector2[verticesObject.Length];
        for (int seg = 0; seg <= nbRadSeg; seg++)
            for (int side = 0; side <= nbSides; side++)
                uvsObject[side + seg * (nbSides + 1)] = new Vector2((float)seg / nbRadSeg, (float)side / nbSides);
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

        int i = 0;
        for (int seg = 0; seg <= nbRadSeg; seg++)
        {
            for (int side = 0; side <= nbSides - 1; side++)
            {
                int current = side + seg * (nbSides + 1);
                int next = side + (seg < (nbRadSeg) ? (seg + 1) * (nbSides + 1) : 0);

                if (i < trianglesObject.Length - 6)
                {
                    trianglesObject[i++] = current;
                    trianglesObject[i++] = next;
                    trianglesObject[i++] = next + 1;

                    trianglesObject[i++] = current;
                    trianglesObject[i++] = next + 1;
                    trianglesObject[i++] = current + 1;
                }
            }
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
