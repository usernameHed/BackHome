using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Rope Description
/// </summary>
public class Rope : MonoBehaviour, IKillable
{
    #region Attributes
    [FoldoutGroup("Rope"), OnValueChanged("ChangeValueSpring"), Tooltip("Position de l'anchor pour les 2 balles"), SerializeField]
    private Vector3 ropeAnchors = Vector3.zero;

    [FoldoutGroup("Rope"), OnValueChanged("ChangeValueSpring"), Tooltip("Force de l'élastique (case 0: valeur de base, case 1: valeur à ajouter quand +un nouveau link)"), SerializeField]
    private float[] spring = new float[3];
    private float safeSpring;

    [FoldoutGroup("Rope"), OnValueChanged("ChangeValueSpring"), Tooltip("Force du damper (case 0: valeur de base, case 1: valeur à ajouter quand +un nouveau link)"), SerializeField]
    private float[] damper = new float[3];
    private float safeDamper;

    [FoldoutGroup("Rope"), OnValueChanged("ChangeValueSpring"), Tooltip("mass des links (case 0: valeur de base, case 1: valeur à ajouter quand +un nouveau link, case 2: valeur fixe quand il ne reste qu'une ball)"), SerializeField]
    private float[] massLink = new float[3];
    private float safeMassLink;

    [FoldoutGroup("Rope"), OnValueChanged("ChangeValueSpring"), Tooltip("Force de l'amortissement"), SerializeField]
    private float[] dragLink = new float[3];
    private float safeDragLink;

    [FoldoutGroup("Rope"), OnValueChanged("ChangeValueSpring"), Tooltip("Force de l'amortissement"), SerializeField]
    private bool useGravityLink = true;

    [FoldoutGroup("Rope"), OnValueChanged("ChangeValueSpring"), Tooltip("Force de l'amortissement"), SerializeField]
    private float min = 0.5f;

    [FoldoutGroup("Rope"), OnValueChanged("ChangeValueSpring"), Tooltip("Force de l'amortissement"), SerializeField]
    private float max = 0.5f;

    [FoldoutGroup("Rope"), Range(0, 100), OnValueChanged("LinkCountAdd"), OnValueChanged("InitPhysicRope"), Tooltip("Nombre de maillons"), SerializeField]
    private int linkCount = 3;
    private void LinkCountAdd() { linkCount++; if (linkCount > linkCountMax) linkCount = linkCountMax; }
    private int safeLinkCount;

    [FoldoutGroup("Rope"), Tooltip("Maillons max"), SerializeField]
    private int linkCountMax = 30;

    [FoldoutGroup("Death"), Tooltip("force lors du breack de link"), SerializeField]
    private float forceWhenExplode = 30f;

    [FoldoutGroup("Death"), Tooltip("Ajout du drag à la ball"), SerializeField]
    private float dragWhenExplode = 10f;

    [FoldoutGroup("Death"), Tooltip("temps avant destructions"), SerializeField]
    private float timeToBecomeHarmLess = 1.5f;
    public float TimeToBecomeHarmLess { get { return timeToBecomeHarmLess; } }

    [FoldoutGroup("Death"), Tooltip("temps avant destructions"), SerializeField]
    private float rationRandom = 0.1f;
    public float RationRandom { get { return rationRandom; } }

    [FoldoutGroup("Objects"), OnValueChanged("InitPhysicRope"), Tooltip("Les 2 objets à relié"), SerializeField]
    private GameObject[] objectToConnect = new GameObject[2];
    [FoldoutGroup("Objects"), Tooltip("le parent où mettre les Link"), SerializeField]
    private Transform parentLink;
    [FoldoutGroup("Objects"), Tooltip("le parent où mettre les Link"), SerializeField]
    private PlayerController playerController;
    public PlayerController RefPlayer { get { return (playerController); } }

    [FoldoutGroup("Debug"), Tooltip("points des link"), SerializeField]
    private Color colorRope;

    [OnValueChanged("CreateFakeListForDebug")]
    private CircularLinkedList<GameObject> listCircular = new CircularLinkedList<GameObject>();
    public CircularLinkedList<GameObject> LinkCircular { get { return listCircular; } }

    [Tooltip("points des link"), SerializeField]
    private List<GameObject> listDebug;


    private bool linkBreaked = false;
    private bool enabledScript = true;
    private bool onlyOneLeft = false;
    #endregion

    #region Initialization
    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        Debug.Log("awake dabord ??");
        InitPrefabsValue(true);
        InitPhysicRope();
    }

    /// <summary>
    /// ici save les valeurs de base
    /// init = true: save les valeurs en awake
    /// init = false: récupère les valeur sauvé en awake
    /// </summary>
    public void InitPrefabsValue(bool init)
    {
        if (init)
        {
            safeSpring = spring[0];
            safeDamper = damper[0];
            safeMassLink = massLink[0];
            safeDragLink = dragLink[0];
            safeLinkCount = linkCount;
        }
        else
        {
            spring[0] = safeSpring;
            damper[0] = safeDamper;
            massLink[0] = safeMassLink;
            dragLink[0] = safeDragLink;
            linkCount = safeLinkCount;
        }
    }

    /// <summary>
    /// Setup les 2 objet à relié
    /// </summary>
    public void InitObjects(GameObject obj1, GameObject obj2, Transform parentRope)
    {
        objectToConnect[0] = obj1;
        objectToConnect[1] = obj2;
        parentLink = parentRope;
    }
    #endregion

    #region Core
    /// <summary>
    /// initialise les springJoints; appelé depuis player
    /// </summary>
    public void InitPhysicRope()
    {
        Debug.Log("init rope ensuite !");
        InitPrefabsValue(true);

        enabledScript = true;
        OnlyOneMainLeft(false);
        ClearJoints(false);  //clear les joints précédents

        //cree un sprintjoint sur la première ball si il n'y en a pas déja
        objectToConnect[0].transform.GetOrAddComponent<SpringJoint>();
        listCircular.AddLast(objectToConnect[0]);

        //cree les balls intermédiaire à la bonne position par rapport aux 2 balls
        for (int i = 0; i < linkCount; i++)
        {
            if (i > linkCountMax)
                break;

            //cherche la position que devrais prendre ce joints
            float maxMid = linkCount + 1;

            float x1 = ((((maxMid - 1.0f) - i) / maxMid) * objectToConnect[0].transform.position.x)
                    + (((1.0f + i) / maxMid) * objectToConnect[1].transform.position.x);
            float y1 = ((((maxMid - 1.0f) - i) / maxMid) * objectToConnect[0].transform.position.y)
                    + (((1.0f + i) / maxMid) * objectToConnect[1].transform.position.y);
            float z1 = ((((maxMid - 1.0f) - i) / maxMid) * objectToConnect[0].transform.position.z)
                    + (((1.0f + i) / maxMid) * objectToConnect[1].transform.position.z);

            Vector3 posJoint = new Vector3(x1, y1, z1);
            GameObject newLink = ObjectsPooler.Instance.SpawnFromPool(GameData.PoolTag.Link, posJoint, Quaternion.identity, parentLink);

            SetupLink(newLink, i);
            listCircular.AddLast(newLink);
        }
        //détruit le springJoint de la dernière ball si il y a
        objectToConnect[1].transform.DestroyComponent<SpringJoint>();
        listCircular.AddLast(objectToConnect[1]);

        //connecte tout les liens ensemble avec des springs joints;
        ChangeValueSpring();
        ChangeColorLink(colorRope);  //change color

        CreateFakeListForDebug();
    }

    /// <summary>
    /// est appelé quand on est la derniere ball restante...
    /// </summary>
    public void OnlyOneMainLeft(bool active)
    {
        onlyOneLeft = active;
        if (active)
        {
            ChangeParamJointWhenAdding(0);
        }
    }

    /// <summary>
    /// applique une force sur TOUT les objets
    /// - force sur les link (et les autres... les balls)
    /// </summary>
    public void ApplyForceOnAll(GameObject obj, Vector3 dir, float force, float forceLink)
    {
        StartCoroutine(ApplyForceOnAllTime(obj, dir, force, forceLink));
    }
    private IEnumerator ApplyForceOnAllTime(GameObject obj, Vector3 dir, float force, float forceLink)
    {
        if (listCircular[0].Value && listCircular[0].Value.GetInstanceID() == obj.GetInstanceID())
        {
            for (int i = 0; i < listCircular.Count; i++)
            {
                ApplyForce(i, dir, force, forceLink);
                yield return new WaitForEndOfFrame();
            }
        }
        else if (listCircular[listCircular.Count - 1].Value && listCircular[listCircular.Count - 1].Value.GetInstanceID() == obj.GetInstanceID())
        {
            for (int i = listCircular.Count - 1; i >= 0; i--)
            {
                ApplyForce(i, dir, force, forceLink);
                yield return new WaitForEndOfFrame();
            }
        }
    }
    private void ApplyForce(int i, Vector3 dir, float force, float forceLink)
    {
        if (!listCircular[i].Value)
            return;
        Rigidbody rbObject = listCircular[i].Value.GetComponent<Rigidbody>();
        if (!rbObject)
            return;

        if (i > 0 && i < listCircular.Count - 1 || listCircular[i].Value.HasComponent<Link>())
        {
            rbObject.AddForce(dir * forceLink, ForceMode.Impulse);
        }
        else
        {
            rbObject.AddForce(dir * force, ForceMode.Impulse);
        }
    }

    /// <summary>
    /// une fois créé, setup le link
    /// </summary>
    private void SetupLink(GameObject newLink, int index)
    {
        Link linkScript = newLink.GetComponent<Link>();
        linkScript.RopeScript = this;
        linkScript.IdFromRope = index;

        newLink.transform.GetOrAddComponent<SpringJoint>();
    }

    /// <summary>
    /// ajoute un link, proche de la ball [index] voulu
    /// index = 0 = le début de la list, index = 1 = la fin de la liste
    /// </summary>
    /// <param name="indexObject"></param>
    public void AddLinkFromExtremity(int indexObject)
    {
        Debug.Log("ici ajoute un link :" + indexObject);
        if (indexObject == 0)
            AddLink(0);
        else
            AddLink(listCircular.Count - 1);
    }

    /// <summary>
    /// est appelé pour BREAK les joints entre les link (sans détruire les link !)
    /// </summary>
    public void JustBreakUpLink(Vector3 positionBallExploded)
    {
        Debug.Log("BREAK link !");
        linkBreaked = true;
        for (int i = 0; i < listCircular.Count; i++)
        {
            if (!listCircular[i].Value)
                continue;

            listCircular[i].Value.transform.DestroyComponent<SpringJoint>();

            if (listCircular[i].Value)
            {
                Rigidbody rbLink = listCircular[i].Value.GetComponent<Rigidbody>();
                rbLink.drag = dragWhenExplode;
                rbLink.AddForce(rbLink.velocity * forceWhenExplode, ForceMode.Impulse);
            }
        }
        StartCoroutine(WaitUntilBecomeHarmLess());
    }
    private IEnumerator WaitUntilBecomeHarmLess()
    {
        yield return new WaitForSeconds(timeToBecomeHarmLess);

        int countList = listCircular.Count;
        int[] deck = new int[countList];

        for (int i = 0; i < countList; i++)
        {
            int j = Random.Range(0, i + 1);

            deck[i] = deck[j];
            deck[j] = 0 + i;
        }

        for (int i = 0; i < deck.Length; i++)
        {
            yield return new WaitForSeconds(rationRandom);

            //get le link du deck random
            if (deck[i] >= listCircular.Count || deck[i] < 0)
                continue;
            GameObject link = listCircular[deck[i]].Value;

            if (!link)
                continue;

            //créé un effet de particule
            //ObjectsPooler.Instance.SpawnFromPool(GameData.Prefabs.DesactiveLink, link.transform.position, Quaternion.identity, ObjectsPooler.Instance.transform);
            link.GetComponent<MeshRenderer>().enabled = false;
            link.GetComponent<Collider>().enabled = false;
        }

        Kill(); //détruit la rope !
    }

    /// <summary>
    /// test si le link passé en paramettre est contenue dans la rope
    /// </summary>
    /// <param name="link">Link est l'objet link à tester</param>
    public bool IsContainingThisLink(GameObject link)
    {
        Link linkScript = link.GetComponent<Link>();
        if (!linkScript)
            return (false);

        if (linkScript.RopeScript == this)
        {
            return (true);
        }
        return (false);
    }

    /// <summary>
    /// ici est appelé directement depuis l'un des link
    /// </summary>
    public void AddLink(int index)
    {
        if (linkCount == linkCountMax || linkBreaked)
            return;
        if (index > listCircular.Count)
        {
            Debug.LogError("index out of range: " + index);
            return;
        }
        LinkCountAdd();

        GameObject closestLink = listCircular[index].Value;

        if (!closestLink)
        {
            listCircular.RemoveAllEmpty();
            closestLink = listCircular[index].Value;
        }


        GameObject newLink = ObjectsPooler.Instance.SpawnFromPool(GameData.PoolTag.Link, closestLink.transform.position, Quaternion.identity, parentLink);
        /*SpringJoint jointLink = */newLink.transform.GetOrAddComponent<SpringJoint>();

        ChangeMeshRenrered(newLink.GetComponent<MeshRenderer>());

        //si l'index n'est pas le dernier (l'un des 2 gros objet), on peut le créé après
        if (index != listCircular.Count - 1)
        {
            listCircular.AddAfter(listCircular[index], newLink);

            SetupLink(newLink, index + 1);

            ChangeThisPring(index);
            ChangeThisPring(index + 1);
            
        }
        else
        {
            Debug.Log("ICI on ajoute sur le dernier ???");
            listCircular.AddBefore(listCircular[index], newLink);

            SetupLink(newLink, index - 0);
            ChangeThisPring(index - 1);
            ChangeThisPring(index - 0);
        }
        ChangeParamJointWhenAdding(1);
        CreateFakeListForDebug();
    }

    /// <summary>
    /// à noter qu'il ne sagit pas de l'index dans le tableau, mais l'index noté dans le script Link.
    /// </summary>
    /// <param name="index"></param>
    public void DeleteLink(int index)
    {
        for (int i = 0; i < listCircular.Count; i++)
        {
            if (!listCircular[i].Value)
                continue;
            Link link = listCircular[i].Value.GetComponent<Link>();
            if (link && link.IdFromRope == index)
            {
                listCircular.Remove(link.gameObject);
                ChangeThisPring(i - 1);
                link.RealyKill();
                ChangeParamJointWhenAdding(-1);
                return;
            }
        }
    }

    /// <summary>
    /// clear la list des joints (et supprime les objets dedant)
    /// clean = true: supprimer cleannement en passant par mes fonctions kill
    /// clean = false: just Desactive !
    /// </summary>
    public void ClearJoints(bool clean)
    {
        Debug.Log("Clear la rope !!");
        for (int i = 0; i < listCircular.Count; i++)
        {
            GameObject linkTmp = listCircular[i].Value;
            if (!linkTmp || !linkTmp.GetComponent<Link>())
                continue;
            if (clean)
                linkTmp.GetComponent<Link>().Kill();
            else
                linkTmp.GetComponent<Link>().RealyKill();
        }
        listCircular.Clear();
    }

    /// <summary>
    /// change la couleur de tout les links (pas le premier / dernier !)
    /// </summary>
    public void ChangeColorLink(Color color)
    {
        colorRope = color;
        for (int i = 1; i < listCircular.Count - 1; i++)
        {
            ChangeMeshRenrered(i);
        }
    }
    private void ChangeMeshRenrered(MeshRenderer meshLink)
    {
        if (!meshLink)
            return;
        meshLink.material.color = colorRope;
    }
    private void ChangeMeshRenrered(int index)
    {
        MeshRenderer meshLink = listCircular[index].Value.GetComponent<MeshRenderer>();
        if (!meshLink)
            return;
        meshLink.material.color = colorRope;
    }

    /// <summary>
    /// ici gère le changements des parametres quand on ajoute (ou supprime ?) un link
    /// </summary>
    private void ChangeParamJointWhenAdding(int add = 1)
    {
        spring[0] += spring[1] * add;
        damper[0] += damper[1] * add;
        massLink[0] += massLink[1] * add;
        dragLink[0] += dragLink[1] * add;

        if (onlyOneLeft)
        {
            //spring[0] = spring[2];
            //damper[0] = damper[2];
            massLink[0] = massLink[2];
            //dragLink[0] = dragLink[2];
        }

        ChangeValueSpring();
    }

    /// <summary>
    /// remplie les infos dans les springs joints
    /// </summary>
    private void ChangeValueSpring()
    {
        for (int i = 0; i < listCircular.Count; i++)
        {
            ChangeThisPring(i);
        }
    }
    private void ChangeThisPring(int index)
    {
        if (index < 0 || index + 1 >= listCircular.Count || !listCircular[index + 1].Value || !listCircular[index].Value)
        {
            listCircular.RemoveAllEmpty();
            return;
        }
        Rigidbody linkBody = listCircular[index].Value.GetComponent<Rigidbody>();
        Rigidbody next = listCircular[index + 1].Value.GetComponent<Rigidbody>();
        SpringJoint joint = listCircular[index].Value.GetComponent<SpringJoint>();

        if (!linkBody || !next || !joint)
        {
            Debug.Log("problèem ici ??");
            return;
        }
            
        joint.connectedBody = next;

        if (index != 0 || index == 0 && linkBody.HasComponent<Link>())
        {
            linkBody.mass = massLink[0];
            linkBody.drag = dragLink[0];
            linkBody.useGravity = useGravityLink;
        }

        joint.minDistance = min;
        joint.maxDistance = max;
        joint.anchor = ropeAnchors;
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = ropeAnchors;
        joint.spring = spring[0];
        joint.damper = damper[0];
        joint.enableCollision = false;
    }

    #endregion

    #region Unity ending functions

    [Button("clearListOfNull")]
    private void clearListOfNull()
    {
        listCircular.RemoveAllEmpty();
        CreateFakeListForDebug();
    }

    private void CreateFakeListForDebug()
    {
        Debug.Log("ici reset... PLUS !");
        //return;
        listDebug.Clear();
        for (int i = 0; i < listCircular.Count; i++)
        {
            listDebug.Add(listCircular[i].Value);
        }
    }

    [FoldoutGroup("Debug"), Button("Kill")]
    public void Kill()
    {
        if (!enabledScript)
            return;
        enabledScript = false;
        Debug.Log("Death Rope ! handle link bien sur");

        ClearJoints(false);

        //InitPrefabsValue(false);
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        
        InitPrefabsValue(false);
    }
    #endregion
}