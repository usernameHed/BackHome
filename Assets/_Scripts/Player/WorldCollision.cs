using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using WorldCollisionNamespace;

namespace WorldCollisionNamespace
{
    /// <summary>
    /// collision détaillé
    /// </summary>
    public enum CollisionType
    {
        InAir = -1,

        Ground = 0,     //0
        GroundLeft,     //1
        GroundRight,    //2

        WallLeft,       //3
        WallRight,      //4

        CeilingLeft,    //5
        CeilingRight,   //6
        Ceilling,       //7
    };

    public enum CollisionSimple
    {
        InAir = -1,

        Ground = 0,
        Wall,
        Ceilling,
    }
}

public class WorldCollision : MonoBehaviour
{
    #region Attributes
    [FoldoutGroup("GamePlay"), Tooltip("Type de terrain"), ShowInInspector]
    private CollisionType collisionType;
    //public CollisionType CollisionExp { get { return (collisionType); } }
    [FoldoutGroup("GamePlay"), Tooltip("Type de terrain"), ShowInInspector]
    private CollisionType lastCollisionTypePersist;
    [FoldoutGroup("GamePlay"), Tooltip("Type de terrain"), ShowInInspector]
    private CollisionType lastCollisionTypeVolatyle;

    [FoldoutGroup("GamePlay"), Tooltip("grounded stable"), ShowInInspector]
    private bool groundedStaySomeFrame = false;
    [FoldoutGroup("GamePlay"), Tooltip("grounded stable"), ShowInInspector]
    private bool groundedExeptionStaySomeFrame = false;
    [FoldoutGroup("GamePlay"), Tooltip("grounded  non stable"), ShowInInspector]
    private bool groundedDedug = false;

    [FoldoutGroup("GamePlay"), Tooltip("coolDownChangementGrounded"), SerializeField]
    private FrequencyCoolDown debugCoolDownGroundedChange;

    [FoldoutGroup("GamePlay"), Tooltip("wallJump 180"), ShowInInspector]
    private static readonly float[] angleReference = { 90f, 45f, 135f, 0f, 180f, 315f, 225f, 270f };
    public float GetAngleReference(int index) { return (angleReference[index]); }
    private static readonly string[] angleReferenceDisplay = { "Ground", "GroundLeft", "GroundRight", "WallLeft", "WallRight", "CeilingLeft", "CeilingRight", "Ceilling" };
    private static readonly string[] angleReferenceDisplaySimple = { "ground", "wall", "ceilling" };

    [FoldoutGroup("GamePlay"), Tooltip("list des layer de collisions"), SerializeField]
    private List<GameData.Layers> listLayerToCollide;
    [FoldoutGroup("GamePlay"), Tooltip("list des layer où on peut sauter / move, mais PAS GROUNDED"), SerializeField]
    private List<GameData.Layers> listLayerExceptionGrounded;

    [FoldoutGroup("GamePlay"), Tooltip("est-t-on dans un coin ?"), SerializeField]
    private bool isOnCoinGround = false;
    public bool IsOnCoinGround() { return (isOnCoinGround); }

    [FoldoutGroup("Normal"), Tooltip("valeur où une normal est considéré comme trop proche d'une autre"), SerializeField]
    private float debugCloseValueNormal = 0.1f;
    [FoldoutGroup("Normal"), Tooltip("vecteur de la normal"), SerializeField]
    private Vector3 normalSumCollide = Vector3.zero;
    //public Vector3 NormalSumCollide { get { return (normalSumCollide); } }
    [FoldoutGroup("Normal"), Tooltip("vecteur persist previous ?"), SerializeField]
    private Vector3 normalSumCollidePrevious = Vector3.up;
    [FoldoutGroup("Normal"), Tooltip("vecteur de la normal SAFE"), SerializeField]
    private Vector3 normalSumCollideVolatyle = Vector3.up;

    [FoldoutGroup("Normal"), Tooltip("list des normals qui touche le joueur"), SerializeField]
    private Vector3[] colliderNormalArray = new Vector3[4];
    [FoldoutGroup("Normal"), Tooltip("list des normals qui touche le joueur"), SerializeField]
    private GameObject[] objectInCollision = new GameObject[4];
    public GameObject[] ObjectInCollision { get { return (objectInCollision); } }

    [FoldoutGroup("Debug"), Tooltip("cooldown du déplacement horizontal"), SerializeField]
    private FrequencyCoolDown coolDownGrounded;
    public FrequencyCoolDown CoolDownGrounded { get { return (coolDownGrounded); } }
    [FoldoutGroup("Debug"), Tooltip("cooldown du jump (influe sur le mouvements du perso)"), SerializeField]
    private FrequencyCoolDown coolDownGroundedJump; //O.2
    public FrequencyCoolDown CoolDownGroundedJump { get { return (coolDownGroundedJump); } }
    [FoldoutGroup("Debug"), Tooltip("cooldown de déplacement dans les air quand on est pendu ..."), SerializeField]
    private FrequencyCoolDown coolDownDesesperateAirMove; //1.5
    public FrequencyCoolDown CoolDownDesesperateAirMove { get { return (coolDownDesesperateAirMove); } }
    private bool isRestartedTimer = false;

    private const float angleDifference = 22.5f;    //défini la différence entre 2 type de mur

    private PlatformManager platformManager;
    public PlatformManager PlatformManager { get { return (platformManager); } }
    #endregion

    #region Initialize

    private void OnEnable()
    {
        InitCollision();
    }

    private void Awake()
    {
        platformManager = new PlatformManager(this);
    }

    /// <summary>
    /// init le player
    /// </summary>
    private void InitCollision()
    {
        ListExt.ClearArray(colliderNormalArray);
        ListExt.ClearArray(objectInCollision);
    }
    #endregion

    #region Core

    /// <summary>
    /// set un nouveau collider dans l'array
    /// </summary>
    private void SetNewCollider(Vector3 otherNormal)
    {
        otherNormal = otherNormal.normalized;

        if (ListExt.IsInArray(colliderNormalArray, otherNormal))
            return;

        //avant de commencer, vérifie encore si il y a une normal identique
        for (int i = 0; i < colliderNormalArray.Length; i++)
        {
            if (UtilityFunctions.IsClose(colliderNormalArray[i].x, otherNormal.x, debugCloseValueNormal)
                && UtilityFunctions.IsClose(colliderNormalArray[i].y, otherNormal.y, debugCloseValueNormal))
            {
                //Debug.Log("trop proche d'une autre !");
                return;
            }
        }

        for (int i = 0; i < colliderNormalArray.Length; i++)
        {
            if (colliderNormalArray[i] == Vector3.zero)
            {
                colliderNormalArray[i] = otherNormal;
                return;
            }
        }
    }
    /// <summary>
    /// set un nouveau gameObject dans l'array
    /// </summary>
    /// <param name="other"></param>
    private void SetNewObjectCollision(GameObject other)
    {
        //ne rien faire si l'objet est déja dans la list
        if (ListExt.IsInArray(objectInCollision, other))
            return;
        //charcher un emplacement vide
        for (int i = 0; i < colliderNormalArray.Length; i++)
        {
            if (objectInCollision[i] == null)
            {
                objectInCollision[i] = other;
                return;
            }
        }
    }

    /// <summary>
    /// retourne vrai si il y a un objet du type tag dans les collisions
    /// </summary>
    /// <param name="objectInArray"></param>
    /// <returns></returns>
    private bool IsThereCollisionWithThis(GameData.Prefabs objectInArray)
    {
        for (int i = 0; i < colliderNormalArray.Length; i++)
        {
            if (objectInCollision[i] && objectInCollision[i].CompareTag(objectInArray.ToString()))
                return (true);
        }
        return (false);
    }
    /// <summary>
    /// renvoi le nombre d'objet en collisions
    /// </summary>
    /// <returns></returns>
    private int HowManyObjectCollide()
    {
        int numberObject = 0;
        for (int i = 0; i < colliderNormalArray.Length; i++)
        {
            if (objectInCollision[i] != null)
                numberObject++;
        }
        return (numberObject);
    }

    /// <summary>
    /// renvoi vrai si il n'y a qu'un normal dans le tableau
    /// </summary>
    /// <returns></returns>
    private bool IsOnlyOneNormal()
    {
        int number = 0;
        for (int i = 0; i < colliderNormalArray.Length; i++)
        {
            if (colliderNormalArray[i] != Vector3.zero)
            {
                number++;
            }
        }
        if (number == 1)
            return (true);
        return (false);
    }

    /// <summary>
    /// affiche les X normals
    /// </summary>
    private void DisplayNormalArray()
    {
        for (int i = 0; i < colliderNormalArray.Length; i++)
        {
            if (colliderNormalArray[i] != Vector3.zero)
            {
                Debug.DrawRay(transform.position, colliderNormalArray[i], Color.magenta, 1f);
            }
        }
    }

    /// <summary>
    /// Ground,         //0        Wall,           //1        Ceilling        //2
    ///
    /// Ground,         //0    /// GroundLeft,     //1    /// GroundRight,    //2    /// WallLeft,       //3    /// WallRight,      //4
    /// CeilingLeft,    //5    /// CeilingRight,   //6    /// Ceilling,       //7
    ///
    /// <param name="normal">direction de la normal de collision</param>
    /// <returns>retourne un type de collision ( explicite)</returns>
    private int WhatKindOfNormalIsIt(Vector3 normal)
    {
        if (normal == Vector3.zero)
        {
            //Debug.Log("return -1, la normal fourni est 0");
            return (-1);
        }


        float angleNormal = QuaternionExt.GetAngleFromVector(normal);    //angle normal collision
        //si la normal de contact est proche de 0 ou 180, à 20° près, alors on est en mode wallJump...

        float diffAngle = 0;
        for (int i = 0; i < angleReference.Length; i++)
        {
            bool groundType = QuaternionExt.IsAngleCloseToOtherByAmount(angleNormal, angleReference[i], angleDifference, out diffAngle);
            if (groundType)
            {
                //Debug.Log(angleReferenceDisplay[i] + ", angleNormal: " + angleNormal + "(diff:" + diffAngle + ")");
                return (i);
            }
        }
        //Debug.Log("return -1");
        return (-1);
    }

    /// <summary>
    /// retourne le type de ground que défini le vecteur
    /// </summary>
    /// <param name="normal"></param>
    /// <returns></returns>
    public CollisionSimple WhatKindOfNormalItIs(Vector3 normal)
    {
        int collisionSimple = WhatKindOfNormalIsIt(normal);
        switch (collisionSimple)
        {
            case 0:
            case 1:
            case 2:
                return (CollisionSimple.Ground);
            case 3:
            case 4:
                return (CollisionSimple.Wall);
            case 5:
            case 6:
            case 7:
                return (CollisionSimple.Ceilling);
            case -1:
            default:
                return (CollisionSimple.InAir);
        }
    }

    /// <summary>
    /// s'il y a plusieurs normal dans le tableau... si on est dans un coin, prendre toujours le wallJump
    /// (sauf si c'est un player !)
    /// </summary>
    private Vector3 DefineWhichNormalToChose()
    {
        bool floor = false;
        bool wall = false;
        Vector3 wallTmp = Vector3.zero;
        int numberNormals = 0;

        for (int i = 0; i < colliderNormalArray.Length; i++)
        {
            if (colliderNormalArray[i] != Vector3.zero)
            {
                //int collisionSimple = WhatKindOfNormalIsIt(colliderNormalArray[i]);
                CollisionSimple collisionSimple = WhatKindOfNormalItIs(colliderNormalArray[i]);
                //Debug.Log("ici: normal:" + colliderNormalArray[i] + ", collision : " + collisionSimple);
                switch (collisionSimple)
                {
                    case CollisionSimple.Ground:
                        floor = true;
                        break;
                    case CollisionSimple.Wall:
                        if (objectInCollision[i] != null && GameData.IsInList(listLayerExceptionGrounded, objectInCollision[i].layer))
                        {
                            //Debug.Log("ici on est sur un player...");
                            break;
                        }
                        wall = true;
                        wallTmp = colliderNormalArray[i];
                        break;
                    case CollisionSimple.Ceilling:
                        //Debug.Log("ceilling");
                        break;
                    case CollisionSimple.InAir:
                    default:
                        break;
                }
                numberNormals++;
            }
        }
        if (floor && wall)
        {
            //Debug.Log("on est un wall");
            isOnCoinGround = true;
            return (wallTmp);
        }
        isOnCoinGround = false;

        if (numberNormals == 0)
        {
            //Debug.Log("i ln'y a aucune normal");
            return (Vector3.zero);
        }

        //Debug.Log("retourne juste la some des normals actuells: " + normalSumCollide);

        return (normalSumCollide);
    }

    /// <summary>
    /// ici parcourt les 4 normals, et si y'en a une sur le ground, ne pas appliquer de force qui colle au mur
    /// </summary>
    /// <returns></returns>
    public bool IsThereOneNormalOnGround()
    {
        for (int i = 0; i < colliderNormalArray.Length; i++)
        {
            if (colliderNormalArray[i] != Vector3.zero)
            {
                int collisionSimple = WhatKindOfNormalIsIt(colliderNormalArray[i]);
                if (collisionSimple == 0 || collisionSimple == 1 || collisionSimple == 2)
                {
                    //Debug.Log("ici on est au sol ! :)");
                    return (true);
                }
            }
        }
        return (false);
    }


    /*public CollisionType GetTheSafestCallision()
    {
        return ((CollisionType)WhatKindOfNormalIsIt(DefineWhichNormalToChose()));
    }*/

    /// <summary>
    /// ici défini la normal de la somme des 4 collisions,
    /// et le type de collision que cela implique
    /// </summary>
    private void DefineNormalAndCollision()
    {
        //set new normal / collision
        normalSumCollide = QuaternionExt.GetMiddleOfXVector(colliderNormalArray);
        if (normalSumCollide == Vector3.zero)
        {
            //Debug.Log("PUTAIN POURQUOI C'4EST ZERO");
            //    normalSumCollide = Vector3.up;
        }


        //save previous collision / normal
        Vector3 normalToChose = DefineWhichNormalToChose();

        CollisionType tmpCollision = (CollisionType)WhatKindOfNormalIsIt(normalToChose);
        if (tmpCollision == CollisionType.InAir && collisionType != CollisionType.InAir)
        {
            lastCollisionTypePersist = collisionType;
            normalSumCollidePrevious = normalSumCollide;
        }

        if (tmpCollision == CollisionType.Ground)
        {
            //Debug.Log("normalToCHose: " + normalToChose);
            for (int i = 0; i < colliderNormalArray.Length; i++)
            {
                //Debug.Log(colliderNormalArray[i]);
            }
            //Debug.Break();
        }

        collisionType = tmpCollision;

        
        
    }

    /// <summary>
    /// renvoi vrai si on peut slider !
    /// </summary>
    public bool CanISlide()
    {
        if (lastCollisionTypePersist == CollisionType.CeilingLeft || lastCollisionTypePersist == CollisionType.CeilingRight)
            return (true);
        return (false);
    }

    /// <summary>
    /// renvoi le type de collision (grounded, wall, ceilling)
    /// si rpevious est vrai, essai de get celui d'avant...
    /// </summary>
    public CollisionSimple GetSimpleCollisionSafe(/*bool previous = false*/)
    {
        //DefineNormalAndCollision(); //ici refédini les normal selons les collisions ???

        //CollisionType tmpCollision = (!previous) ? collisionType : lastCollisionTypePersist;
        CollisionType tmpCollision = GetCollisionSafe();

        switch (tmpCollision)
        {
            case CollisionType.Ceilling:
            case CollisionType.CeilingLeft:
            case CollisionType.CeilingRight:
                return (CollisionSimple.Ceilling);

            case CollisionType.WallLeft:
            case CollisionType.WallRight:
                return (CollisionSimple.Wall);

            case CollisionType.Ground:
            case CollisionType.GroundLeft:
            case CollisionType.GroundRight:
                return (CollisionSimple.Ground);

            default:
                return (CollisionSimple.InAir);
        }
    }
    public CollisionSimple GetPreviousPersistCollision(/*bool previous = false*/)
    {
        //DefineNormalAndCollision(); //ici refédini les normal selons les collisions ???

        //CollisionType tmpCollision = (!previous) ? collisionType : lastCollisionTypePersist;
        CollisionType tmpCollision = GetLastCollisionPersistSafe();

        switch (tmpCollision)
        {
            case CollisionType.Ceilling:
            case CollisionType.CeilingLeft:
            case CollisionType.CeilingRight:
                return (CollisionSimple.Ceilling);

            case CollisionType.WallLeft:
            case CollisionType.WallRight:
                return (CollisionSimple.Wall);

            case CollisionType.Ground:
            case CollisionType.GroundLeft:
            case CollisionType.GroundRight:
                return (CollisionSimple.Ground);

            default:
                return (CollisionSimple.InAir);
        }
    }

    /// <summary>
    /// display
    /// </summary>
    private void DisplayCollision()
    {
        for (int i = 0; i < angleReference.Length; i++)
        {
            if (collisionType.ToString() == angleReferenceDisplay[i])
            {
                Debug.Log(angleReferenceDisplay[i]);
                return;
            }
        }
    }

    /// <summary>
    /// ici reset tout à false
    /// </summary>
    private void ResetNormalAndCollision()
    {
        ListExt.ClearArray(colliderNormalArray);
        ListExt.ClearArray(objectInCollision);

        if (collisionType != CollisionType.InAir)
        {
            lastCollisionTypePersist = collisionType;
            normalSumCollidePrevious = normalSumCollide;
        }

        collisionType = CollisionType.InAir;
    }

    /// <summary>
    /// est appelé pour savoir si on est grounded
    /// </summary>
    /// <returns></returns>
    private bool IsGrounded()
    {
        return (groundedDedug);
    }
    /// <summary>
    /// ici on sait si on est grounded de manière safe (depuis les 0.1 seconde)
    /// </summary>
    public bool IsGroundedSafe()
    {
        return (groundedStaySomeFrame);
    }
    /// <summary>
    /// ici on sait si on est groundedExeption de manière safe (depuis les 0.1 seconde)
    /// groundedExeption = NON grounded, MAIS on fait comme si on était grounded pour les mouvements / jump
    /// </summary>
    public bool IsGroundeExeptionSafe()
    {
        return (groundedExeptionStaySomeFrame);
    }
    /// <summary>
    /// get la dernière collision safe connu
    /// </summary>
    public CollisionType GetCollisionSafe()
    {
        return (lastCollisionTypeVolatyle);
    }
    /// <summary>
    /// get la dernière collision safe connu
    /// </summary>
    private CollisionType GetLastCollisionPersistSafe()
    {
        return (lastCollisionTypePersist);
    }
    /// <summary>
    /// Get la dernière some de normal de manière safe
    /// </summary>
    public Vector3 GetSumNormalSafe()
    {
        return (normalSumCollideVolatyle);
    }

    /// <summary>
    /// est-il sur le sol ?
    /// </summary>
    /// <returns></returns>
    public bool IsOnFloor()
    {
        CollisionSimple tmpCollision = GetSimpleCollisionSafe();
        if (tmpCollision == CollisionSimple.Ground)
            return (true);
        return (false);
    }

    /// <summary>
    /// appelé depuis l'extérieur
    /// </summary>
    /// <param name="isGrounded">grounded ou pas ?</param>
    /// <param name="isGrounded">ici hard reset, ou on est pas sur, et on fait selon le cooldown ?</param>
    private void SetGrounded(bool isGrounded, bool hard, int layerObject = -1)
    {
        //si on est normalement grounded, MAIS que l'objet est dans la list des exeption (le player)
        if (isGrounded && layerObject != -1 && GameData.IsInList(listLayerExceptionGrounded, layerObject))
        {
            //Debug.Log("not grounded, but ok for move / jump");
            groundedExeptionStaySomeFrame = true;
            debugCoolDownGroundedChange.StartCoolDown();
            lastCollisionTypeVolatyle = lastCollisionTypePersist;
            normalSumCollideVolatyle = normalSumCollidePrevious;

            coolDownDesesperateAirMove.Reset();
            isRestartedTimer = false;
            return;
        }

        groundedDedug = isGrounded;
        if (groundedDedug)
        {
            groundedStaySomeFrame = groundedDedug;
            debugCoolDownGroundedChange.StartCoolDown();
            lastCollisionTypeVolatyle = lastCollisionTypePersist;
            normalSumCollideVolatyle = normalSumCollidePrevious;

            coolDownDesesperateAirMove.Reset();
            isRestartedTimer = false;
        }
        else
        {
            //si c'est faux, attendre X avant de vraimant metrre à faux !
            if (debugCoolDownGroundedChange.IsReady())
            {
                groundedStaySomeFrame = false;
                groundedExeptionStaySomeFrame = false;
                lastCollisionTypeVolatyle = CollisionType.InAir;
                normalSumCollideVolatyle = Vector3.zero;

                if (!isRestartedTimer)
                {
                    coolDownDesesperateAirMove.StartCoolDown();
                    isRestartedTimer = true;
                }
            }

        }
    }

    /// <summary>
    /// ici on a jump, gérer les cooldowns
    /// </summary>
    public void HasJustJump()
    {
        SetGrounded(false, true);
        coolDownGroundedJump.StartCoolDown();
    }

    /// <summary>
    /// est appelé à chaque onCollision/stay, et reset le cooldown grounded
    /// Ground,         //0    Wall,           //1        Ceilling        //2
    /// </summary>
    private void ResetCoolDownGroundedIfGrounded(Vector3 normal)
    {
        int onFloor = WhatKindOfNormalIsIt(normal);

        if (onFloor == 0 || onFloor == 1 || onFloor == 2)    //on est sur le sol !
        {
            
            //Debug.Log("ici on reset car on est sur le sol !");
            coolDownGrounded.Reset();
        }
    }

    /// <summary>
    /// trigger le collider à 0.7 (se produit avant le collisionStay)
    /// sauvegarde la direction player - point de contact
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerStay(Collider other)
    {
        if (coolDownGroundedJump.IsReady() && GameData.IsInList(listLayerToCollide, other.gameObject.layer))
        {
            Vector3 point = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
            Vector3 normal = (point - transform.position) * -1;

            ResetCoolDownGroundedIfGrounded(normal);    //set le cooldown pour éviter de se bloquer en wallJump quand on est au sol...
            SetNewCollider(normal);                     //set les normals des collisions
            SetNewObjectCollision(other.gameObject);    //set l'objet en collision
            SetGrounded(true, true, other.gameObject.layer);
            //groundedDedug = true;
        }
    }

    /// <summary>
    /// save la normal de la collision
    /// </summary>
    /// <param name="other"></param>
    private void OnCollisionStay(Collision other)
    {
        if (coolDownGroundedJump.IsReady() && GameData.IsInList(listLayerToCollide, other.gameObject.layer))
        {
            Vector3 normal = other.contacts[0].normal;

            ResetCoolDownGroundedIfGrounded(normal);    //set le cooldown pour éviter de se bloquer en wallJump quand on est au sol...
            SetNewCollider(normal);                     //set les normals des collisions
            SetNewObjectCollision(other.gameObject);    //set l'objet en collision
            SetGrounded(true, true, other.gameObject.layer);
            //groundedDedug = true;
        }
    }
    #endregion


    #region Unity ending functions

    private void FixedUpdate()
    {
        DefineNormalAndCollision();
        //DisplayCollision();

        DisplayNormalArray();

        EventManager.TriggerEvent(GameData.Event.Grounded); //c'est ici qu'on fait appelle à tout ce qui peut être grounded...

        //Debug.Log("iciiia: " + collisionType);

        ResetNormalAndCollision();
        //ListExt.ClearArray(colliderNormalArray);
        SetGrounded(false, false);
        //groundedDedug = false;
    }
    #endregion
}