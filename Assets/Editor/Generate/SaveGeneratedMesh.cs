#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;

/// <summary>
/// SaveGeneratedMesh Description
/// </summary>
[System.Serializable]
public class SaveGeneratedMesh
{
    #region Attributes
    [FoldoutGroup("Save"), Tooltip("type"), SerializeField]
    private Transform selectedGameObject;
    #endregion

    #region Initialization
    public SaveGeneratedMesh(Transform selected)
    {
        this.selectedGameObject = selected;
    }
    #endregion

    #region Core
    public void SaveAsset(string pathToSave, string saveName, Transform newMesh)
    {
        var mf = selectedGameObject.GetComponent<MeshFilter>();
        if (mf)
        {
            string savePath = pathToSave + saveName + ".asset";
            
            if (AssetDatabase.GetAssetPath(mf.mesh) == null || AssetDatabase.GetAssetPath(mf.mesh) == "")
            {
                Debug.Log("Saved Mesh to:" + savePath);
                AssetDatabase.CreateAsset(mf.mesh, savePath);
            }
            else
            {
                Debug.LogWarning("Mesh already created");
                //string newPath = ParsingExt.GetNextFileName(savePath);
                //savePath = ParsingExt.GetNextFileName(savePath);
                //Debug.Log("name: " + newPath);


                //AssetDatabase.CreateAsset(newMesh.GetComponent<MeshFilter>().mesh, newPath);
            }
        }
    }
    #endregion

    #region Unity ending functions

    #endregion
}
#endif