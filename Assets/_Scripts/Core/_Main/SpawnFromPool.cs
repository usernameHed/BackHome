using UnityEngine;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;

/// <summary>
/// SpawnFromPool Description
/// </summary>
public class SpawnFromPool : MonoBehaviour
{
    #region Attributes

    [Serializable]
    public struct SpawnsObjects
    {
        [Tooltip("Le type d'objet qui est contenue dans le transform")]
        public GameData.PoolTag objectToSpawn;
        [Tooltip("la parent contenant tout les objets")]
        public Transform parentOfChilds;
    }

    [FoldoutGroup("Objects"), Tooltip("list spawn enemy dans le child"), SerializeField]
    private List<SpawnsObjects> spawnObjects = new List<SpawnsObjects>();
    #endregion

    #region Initialization
    /// <summary>
    /// est appelé pour spawn tout les objets
    /// </summary>
    public void SpawnContent()
    {
        Debug.Log("Spawn the content of the level !");
        SpawnObjects();
    }

    #endregion

    #region Core
    /// <summary>
    /// spawn tout les objets (sauf les players...)
    /// </summary>
    private void SpawnObjects()
    {
        //parcourt la list des types d'objects
        for (int i = 0; i < spawnObjects.Count; i++)
        {
            //pour chaque type, parcourt tout ses enfants, et créé les objets du bon type à l aposition des enfants
            //si ces enfants sont actif (change la position selon un raycast ?)
            int childs = spawnObjects[i].parentOfChilds.childCount;
            for (int j = 0; j < childs; j++)
            {
                Transform child = spawnObjects[i].parentOfChilds.GetChild(j);

                if (!child.gameObject.activeSelf)
                    continue;

                ObjectsPooler.Instance.SpawnFromPool(spawnObjects[i].objectToSpawn, child.position, child.rotation, ObjectsPooler.Instance.transform);
            }
        }
    }
    #endregion

    #region Unity ending functions

	#endregion
}
