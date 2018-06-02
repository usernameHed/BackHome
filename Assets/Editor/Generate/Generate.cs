using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;

/// <summary>
/// Generate Description
/// </summary>
public abstract class Generate : MonoBehaviour
{
    #region Attributes

    public enum TypeMesh
    {
        Plane,         //tout les objets du décors actif (les boules)
        Box,         //les balls
        Cone,           //les link sont dans ce layer
        Tube,
        Torus,
        Sphere,
        IcoSphere,
    }

    [FoldoutGroup("GamePlay"), Tooltip("type"), SerializeField]
    protected TypeMesh typeMesh;

    [FoldoutGroup("GamePlay"), Tooltip("type"), OnValueChanged("changeDisplay"), SerializeField]
    private bool diplayText = true;

    [FoldoutGroup("GamePlay"), Tooltip("text"), SerializeField]
    protected TextMesh textprefabs;

    [FoldoutGroup("Save"), Tooltip("name"), SerializeField]
    protected string saveName = "SavedMesh";

    [FoldoutGroup("Save"), Tooltip("name"), SerializeField]
    protected string pathToSave = "Assets/_Scripts/Generate/Generated/";


    protected SaveGeneratedMesh saveMesh;
    protected MeshFilter meshFilter;
    protected MeshRenderer meshRenderer;

    protected Vector3[] verticesObject;           //verticle of object
    protected Vector3[] normalesObject;           //normals of all verticles
    protected Vector2[] uvsObject;                //uvs of points;
    protected int[] trianglesObject;              //then save triangle of objects

    protected Mesh meshObject;

    #endregion

    #region Initialization
    private void Awake()
    {
        saveMesh = new SaveGeneratedMesh(transform);
    }

    private void Start()
    {
        InitMesh();
    }

    /// <summary>
    /// affiche ou cache le texte des vertices
    /// </summary>
    private void changeDisplay()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(diplayText);
        }
    }
    #endregion

    #region Core

    /// <summary>
    /// génère le mesh
    /// </summary>
    [Button("GeneratePlease")]
    private void GeneratePlease()
    {
        transform.ClearChildImediat();  //supprime les anciens enfants

        meshFilter = gameObject.transform.GetOrAddComponent<MeshFilter>();
        meshRenderer = gameObject.transform.GetOrAddComponent<MeshRenderer>();
        if (Application.isPlaying)
            meshObject = meshFilter.mesh;
        else
            meshObject = meshFilter.sharedMesh;
        meshObject.Clear();

        GenerateMesh();

        meshObject.vertices = verticesObject;
        meshObject.normals = normalesObject;
        meshObject.uv = uvsObject;
        meshObject.triangles = trianglesObject;

        meshObject.RecalculateBounds();
        //mesh.Optimize();

        //MeshUtility.Optimize(mesh);
        DisplayText();
    }

    [Button("Save")]
    private void SaveMesh()
    {
        saveMesh.SaveAsset(pathToSave, saveName, transform);
    }

    abstract protected void InitMesh(); //appelé à l'initialisation
    abstract protected void GenerateMesh(); //appelé à l'initialisation

    /// <summary>
    /// affiche les textes sur les vertices
    /// </summary>
    protected void DisplayText()
    {
        int j = 0;
        foreach (Vector3 pos in verticesObject)
        {
            //Vector3 globalPos = new Vector3(transform.position.x - pos.x, transform.position.x - pos.y, transform.position.x - pos.z);
            TextMesh tm = Instantiate(textprefabs, pos, Quaternion.identity/*, transform*/) as TextMesh;
            tm.name = (j).ToString();
            tm.text = (j++).ToString();
            tm.transform.SetParent(transform);
            tm.transform.position = new Vector3(tm.transform.position.x + transform.position.x, tm.transform.position.y + transform.position.y, tm.transform.position.z + transform.position.z);
        }
    }
    #endregion

    #region Unity ending functions

    #endregion
}
