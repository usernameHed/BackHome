using Sirenix.OdinInspector;
using UnityEngine;

public class DisableOnEndAnimation : MonoBehaviour
{
    [FoldoutGroup("GamePlay"), Tooltip("desactiver l'objet courant ou le parent ?"), SerializeField]
    public bool destroyObject = false;

    [FoldoutGroup("GamePlay"), Tooltip("desactiver l'objet courant ou le parent ?"), SerializeField]
    public bool selectParent = false;

    //[FoldoutGroup("Debug"), Tooltip("opti fps"), SerializeField]
    //private FrequencyTimer updateTimer;

    //private bool wantToDisable = false;
    private GameObject parent;

    private void Awake()
    {
        if (selectParent)
        {
            parent = transform.parent.gameObject; 
        }
    }

    public void DisableObject()
    {
        if (!destroyObject)
        {
            //wantToDisable = true;
            if (selectParent && parent)
                parent.SetActive(false);
            else
                gameObject.SetActive(false);
        }
        else
        {
            //wantToDisable = true;
            if (selectParent && parent)
                Destroy(parent.gameObject);
            else
                Destroy(gameObject);
        }
    }
	
	// Update is called once per frame
	/*void Update ()
    {
        if (updateTimer.Ready())
        {
            if (wantToDisable)
            {
                wantToDisable = false;
                if (selectParent && parent)
                    parent.SetActive(false);
                else
                    gameObject.SetActive(false);
            }
        }
	}*/
}
