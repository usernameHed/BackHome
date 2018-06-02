using UnityEditor;
using UnityEngine;

using Sirenix.OdinInspector;
using TMPro;

/// <summary>
/// Plane Description
/// </summary>
public class ProceduralBox : Generate
{
    #region Attributes
    [FoldoutGroup("GamePlay"), Tooltip("resX"), SerializeField]
    float length = 1f;
    [FoldoutGroup("GamePlay"), Tooltip("resX"), SerializeField]
    float width = 1f;
    [FoldoutGroup("GamePlay"), Tooltip("resX"), SerializeField]
    float height = 1f;
    #endregion

    #region Initialization
    protected override void InitMesh()
    {
        typeMesh = TypeMesh.Box;
    }
    #endregion

    #region Core
    /// <summary>
    /// calculate verticle
    /// </summary>
    private void CalculateVerticle()
    {
        Vector3 p0 = new Vector3(-length * .5f, -width * .5f, height * .5f);
        Vector3 p1 = new Vector3(length * .5f, -width * .5f, height * .5f);
        Vector3 p2 = new Vector3(length * .5f, -width * .5f, -height * .5f);
        Vector3 p3 = new Vector3(-length * .5f, -width * .5f, -height * .5f);

        Vector3 p4 = new Vector3(-length * .5f, width * .5f, height * .5f);
        Vector3 p5 = new Vector3(length * .5f, width * .5f, height * .5f);
        Vector3 p6 = new Vector3(length * .5f, width * .5f, -height * .5f);
        Vector3 p7 = new Vector3(-length * .5f, width * .5f, -height * .5f);

        verticesObject = new Vector3[]
        {
	        p0, p1, p2, p3, // Bottom 
	        p7, p4, p0, p3, // Left
	        p4, p5, p1, p0, // Front
	        p6, p7, p3, p2, // Back
	        p5, p6, p2, p1, // Right
	        p7, p6, p5, p4  // Top
        };
    }

    /// <summary>
    /// after having verticle, calculate normals of each points
    /// </summary>
    private void CalculateNormals()
    {
        Vector3 up = Vector3.up;
        Vector3 down = Vector3.down;
        Vector3 front = Vector3.forward;
        Vector3 back = Vector3.back;
        Vector3 left = Vector3.left;
        Vector3 right = Vector3.right;

        normalesObject = new Vector3[]
        {
	        down, down, down, down,     // Bottom
	        left, left, left, left,     // Left
	        front, front, front, front, // Front
	        back, back, back, back,     // Back
	        right, right, right, right, // Right
	        up, up, up, up              // Top
        };
    }

    /// <summary>
    /// calculate UV of each points;
    /// </summary>
    private void CalculateUvs()
    {
        Vector2 _00 = new Vector2(0f, 0f);
        Vector2 _10 = new Vector2(1f, 0f);
        Vector2 _01 = new Vector2(0f, 1f);
        Vector2 _11 = new Vector2(1f, 1f);

        uvsObject = new Vector2[]
        {
	        _11, _01, _00, _10,     // Bottom
	        _11, _01, _00, _10,     // Left
	        _11, _01, _00, _10,     // Front
	        _11, _01, _00, _10,     // Back
	        _11, _01, _00, _10,     // Right
	        _11, _01, _00, _10,     // Top
        };
    }

    /// <summary>
    /// then save triangls of objects;
    /// </summary>
    private void CalculateTriangle()
    {
        trianglesObject = new int[]
        {
	        // Bottom
	        3, 1, 0,
            3, 2, 1,			
 
	        // Left
	        3 + 4 * 1, 1 + 4 * 1, 0 + 4 * 1,
            3 + 4 * 1, 2 + 4 * 1, 1 + 4 * 1,
 
	        // Front
	        3 + 4 * 2, 1 + 4 * 2, 0 + 4 * 2,
            3 + 4 * 2, 2 + 4 * 2, 1 + 4 * 2,
 
	        // Back
	        3 + 4 * 3, 1 + 4 * 3, 0 + 4 * 3,
            3 + 4 * 3, 2 + 4 * 3, 1 + 4 * 3,
 
	        // Right
	        3 + 4 * 4, 1 + 4 * 4, 0 + 4 * 4,
            3 + 4 * 4, 2 + 4 * 4, 1 + 4 * 4,
 
	        // Top
	        3 + 4 * 5, 1 + 4 * 5, 0 + 4 * 5,
            3 + 4 * 5, 2 + 4 * 5, 1 + 4 * 5,
        };
    }

    /// <summary>
    /// here generate the mesh...
    /// </summary>
    protected override void GenerateMesh()
    {
        Debug.Log("generate box...");
        CalculateVerticle();
        CalculateNormals();
        CalculateUvs();
        CalculateTriangle();
    }
    #endregion

    #region Unity ending functions

    #endregion
}
