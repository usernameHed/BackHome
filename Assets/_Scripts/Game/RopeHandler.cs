using UnityEngine;
using Sirenix.OdinInspector;
using Obi;
using System.Collections;
using WorldCollisionNamespace;

/// <summary>
/// RopeHandler Description
/// </summary>
public class RopeHandler : MonoBehaviour
{
    #region Attributes
    public float actualTensity;
    public bool activeRopeChange = true;

    [FoldoutGroup("GamePlay"), Tooltip("vitesse d'ajout/suppression"), SerializeField]
    private bool isTenseForJump = false;
    public bool IsTenseForJump { get { return (isTenseForJump); } }
    [FoldoutGroup("GamePlay"), Tooltip("vitesse d'ajout/suppression"), SerializeField]
    private bool isTenseForAdd = false;
    [FoldoutGroup("GamePlay"), Tooltip("vitesse d'ajout/suppression"), SerializeField]
    private bool isTenseForLess = false;
    [FoldoutGroup("GamePlay"), Tooltip("nombre de particule actuel dans la rope"), SerializeField]
    private int particleInRope;
    public int ParticleInRope { get { return (particleInRope); } }
    [FoldoutGroup("GamePlay"), Tooltip("valeur de stretch avant d'ajouter"), SerializeField]
    private float stretchForJump = 1.5f;
    [FoldoutGroup("GamePlay"), Tooltip("suppression min de aprticule"), SerializeField]
    private int maxParticleWhenGrip = 120;
    [FoldoutGroup("GamePlay"), Tooltip("points à récupérer pour créé le vecteur directeur player / rope"), SerializeField]
    private int getPointOfRopeFromPlayer = 2;

    [FoldoutGroup("GamePlay"), Tooltip("vibration à l'ajout"), SerializeField]
    private Vibration onAdd;
    [FoldoutGroup("GamePlay"), Tooltip("vibration à la suppression"), SerializeField]
    private Vibration onRemove;


    [FoldoutGroup("Move In Air"), Tooltip("valeur de stretch avant d'ajouter"), SerializeField]
    private float stretchForAirMove = 1.5f;
    [FoldoutGroup("Move In Air"), Tooltip("vitesse d'ajout/suppression"), SerializeField]
    private bool isTenseForAirMove = false;
    public bool IsTenseForAirMove { get { return (isTenseForAirMove); } }

    [FoldoutGroup("Add and Remove"), Tooltip("vitesse d'ajout/suppression"), SerializeField]
    private float hookExtendRetractSpeed = 2;
    [FoldoutGroup("Add and Remove"), Tooltip("valeur de stretch avant d'ajouter"), SerializeField]
    private float stretchAdd = 1.5f;
    [FoldoutGroup("Add and Remove"), Tooltip("valeur de stretch avant de supprimer"), SerializeField]
    private float stretchRemove = 0.5f;
    [FoldoutGroup("Add and Remove"), Tooltip("ajout max de particule"), SerializeField]
    private int minParticle = 80;
    [FoldoutGroup("Add and Remove"), Tooltip("suppression min de aprticule"), SerializeField]
    private int maxParticle = 105;


    [FoldoutGroup("Grip"), Tooltip("nombre de particule minimum à mettre quand on est grip"), SerializeField]
    private int resetToMinParticleNumberWhenGrip = 19;
    [FoldoutGroup("Grip"), Tooltip("nombre de particule minimum à mettre quand on est grip"), SerializeField]
    private int resetToMinParticleNumberWhenGripStretch = 19;
    [FoldoutGroup("Grip"), Tooltip("valeur de stretch avant de supprimer"), SerializeField]
    private float stretchRemoveWhenGrip = 1.1f;
    [FoldoutGroup("Grip"), Tooltip("vitesse d'ajout/suppression"), SerializeField]
    private float speedRemoveWhenGrip = 6;

    [FoldoutGroup("After Grip"), Tooltip("vitesse d'ajout/suppression"), SerializeField]
    private float speedAddAfterGrip = 5;


    [FoldoutGroup("Object"), Tooltip("liens de la rope"), SerializeField]
    private ObiRope rope;
    [FoldoutGroup("Object"), Tooltip("liens du curseur (pour ajouter/enleverl des particule)"), SerializeField]
    private ObiRopeCursor cursor;

    [FoldoutGroup("Debug"), Tooltip("ref"), ShowInInspector]
    private int numberParticleBase;         //nombre de particule de base
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayersManager playerManager;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private ObiActor actor;

    [FoldoutGroup("Debug"), Tooltip("est-on grippé ?"), SerializeField]
    private bool isGripped = false;         //est-on grippé ?
    [FoldoutGroup("Debug"), Tooltip("est-on en raccourcicement ?"), SerializeField]
    private bool grippedPrepareLessSetup = false;
    [FoldoutGroup("Debug"), Tooltip("est-on en élongation ?"), SerializeField]
    private bool onGroundLenghting = false;
    
    private ObiDistanceConstraintBatch dbatch;
    #endregion

    #region Initialization
    /// <summary>
    /// 
    /// </summary>
    private void Start()
    {
        Init();
    }

    private void Init()
    {
        dbatch = rope.DistanceConstraints.GetFirstBatch();  //ref rope
        SetNumberParticle();                                //set nombre particle initial
        numberParticleBase = particleInRope;                //save nombre particle initial
        grippedPrepareLessSetup = false;
    }
    #endregion

    #region Core
    /// <summary>
    /// set le nombre courrant de aprticule
    /// </summary>
    private void SetNumberParticle()
    {
        particleInRope = (dbatch.ConstraintCount - 1) * 2 + 1;
    }

    public int GetSizeConstrain()
    {
        return ((dbatch.ConstraintCount - 1) * 2 + 1);
    }
    private int GetIndexParticle(int index)
    {
        index = (index < 0) ? 0 : index;
        index = (index > GetSizeConstrain()) ? GetSizeConstrain() : index;
        return (dbatch.springIndices[index]);
    }
    private Vector3 GetPosParticle(int index)
    {
        return (actor.GetParticlePosition(GetIndexParticle(index)));
    }

    /// <summary>
    /// retourne le vercteur directeur droite / gauche du player / de la rope
    /// </summary>
    /// <param name="indexPlayer"></param>
    /// <returns></returns>
    public Vector3 GetVectorFromPlayer(int indexPlayer)
    {
        Vector3 dir;
        if (indexPlayer == 0)
        {
            /*for (int i = 0; i < getPointOfRopeFromPlayer; i++)
            {
                DebugExtension.DebugWireSphere(GetPosParticle(i), Color.yellow, 0.5f, 1f);
            }*/
            dir = GetPosParticle(0) - GetPosParticle(getPointOfRopeFromPlayer);
            return (-dir);
        }
        else
        {
            /*for (int i = GetSizeConstrain(); i > GetSizeConstrain() - getPointOfRopeFromPlayer; i--)
            {
                DebugExtension.DebugWireSphere(GetPosParticle(i), Color.yellow, 0.5f, 1f);
            }*/

            dir = GetPosParticle(GetSizeConstrain()) - GetPosParticle(GetSizeConstrain() - getPointOfRopeFromPlayer);
            return (-dir);
        }
    }

    /// <summary>
    /// ici cherche le premier point de pivot, puis assigne la distance, et le nombre de particule player - point de pivot.
    /// </summary>
    public void GetPivotPoint(int indexPlayer, float angleDifferenceMarginForPivotPoint, float marginAngleConsideredAsEqual, out float distance, out int numberParticleFromPlayerToPivot, out Vector3 pointPivot, out bool onRope)
    {
        int startPoint = 4; //ici commence au Xeme point...
        float diffAdd = 0;  //ici la somme des différence d'angles à chaque test
        int numberParticleTested = startPoint;   //ici le nombre de particule testé
        onRope = false;

        Vector3 lastDir = Vector3.zero; //défini le vecteur directeur précédent
        Vector3 currentDir = Vector3.zero;  //défini le vecteur à tester courrament dans la boucle

        if (indexPlayer == 0)
        {
            Vector3 posFirst = GetPosParticle(0);
            Vector3 posSecond = GetPosParticle(0 + startPoint);
            lastDir = posSecond - posFirst;

            for (int i = startPoint; i <= GetSizeConstrain() - 1; i++)
            {
                Vector3 posFirstTmp = posFirst;

                posFirst = GetPosParticle(i);
                posSecond = GetPosParticle(i + 1);
                currentDir = posSecond - posFirst;

                //Debug.Log("posActualPartivcleTested(current): " + posFirst);
                //Debug.Log("pos next: " + posSecond);
                //Debug.Log("current Direction: " + currentDir);

                if (currentDir == Vector3.zero)
                {
                    //ici ok même angle, on continue
                    //DebugExtension.DebugWireSphere(GetPosParticle(i), Color.blue, 1f, 1f);
                    numberParticleTested++;

                    //lastDir = currentDir;
                    //Debug.Log("next");

                    continue;
                }

//Debug.DrawRay(posFirstTmp, lastDir, Color.red, 1f);
//Debug.DrawRay(posFirst, currentDir, Color.cyan, 1f);

                float anglePrevious = QuaternionExt.GetAngleFromVector(lastDir);
                float angleCurrent = QuaternionExt.GetAngleFromVector(currentDir);
                float diffAngleDir;
                QuaternionExt.IsAngleCloseToOtherByAmount(anglePrevious, angleCurrent, angleDifferenceMarginForPivotPoint, out diffAngleDir);

                if (diffAngleDir > marginAngleConsideredAsEqual)
                    diffAdd += diffAngleDir;
                //Debug.Log("anglePrevious: " + anglePrevious + ", angleCurrent: " + angleCurrent);
                //Debug.Log("diffAdd: " + diffAdd + " (+" + diffAngleDir + ")");

                if (diffAdd < angleDifferenceMarginForPivotPoint)
                {
                    //ici ok même angle, on continue
                    //DebugExtension.DebugWireSphere(GetPosParticle(i), Color.blue, 1f, 1f);
                    numberParticleTested++;

                    lastDir = currentDir;
                    //Debug.Log("next");
                }
                else
                {
                    //ici on a changé d'angle ! (soit brutalement, soit avec le temps)
                    //ici prendre... la différence entre le premier et ce dernier point testé
                    pointPivot = GetPosParticle(i);
                    distance = Vector3.Distance(playerManager.GetPosPlayer(indexPlayer), pointPivot);
                    numberParticleFromPlayerToPivot = numberParticleTested;
                    //Debug.Log("ici !");
                    onRope = true;
                    DebugExtension.DebugWireSphere(GetPosParticle(i), Color.blue, 1f, 1f);
                    //Debug.Break();
                    return;
                }


            }

            //Debug.Log("on prend le dernier");
            //ici prendre... le dernier player. (et la distance par rapport au premier player)
            pointPivot = playerManager.GetPosPlayer(1);
            distance = Vector3.Distance(playerManager.GetPosPlayer(0), pointPivot);
            numberParticleFromPlayerToPivot = numberParticleTested;
            return;
        }
        else
        {
            Vector3 posFirst = GetPosParticle(GetSizeConstrain());
            Vector3 posSecond = GetPosParticle(GetSizeConstrain() - startPoint);
            lastDir = posSecond - posFirst;

            for (int i = GetSizeConstrain() - startPoint; i >= 1; i--)
            {
                Vector3 posFirstTmp = posFirst;


                posFirst = GetPosParticle(i);
                posSecond = GetPosParticle(i - 1);
                currentDir = posSecond - posFirst;

                if (currentDir == Vector3.zero)
                {
                    //ici ok même angle, on continue
                    //DebugExtension.DebugWireSphere(GetPosParticle(i), Color.blue, 1f, 1f);
                    numberParticleTested++;

                    //lastDir = currentDir;
                    //Debug.Log("next");

                    continue;
                }

                //Debug.DrawRay(posFirstTmp, lastDir, Color.red, 1f);
                //Debug.DrawRay(posFirst, currentDir, Color.cyan, 1f);


                float anglePrevious = QuaternionExt.GetAngleFromVector(lastDir);
                float angleCurrent = QuaternionExt.GetAngleFromVector(currentDir);
                float diffAngleDir;
                QuaternionExt.IsAngleCloseToOtherByAmount(anglePrevious, angleCurrent, angleDifferenceMarginForPivotPoint, out diffAngleDir);

                diffAdd += diffAngleDir;

                if (diffAdd < angleDifferenceMarginForPivotPoint)
                {
                    //ici ok même angle, on continue
                    //DebugExtension.DebugWireSphere(GetPosParticle(i), Color.red, 1f, 1f);
                    numberParticleTested++;
                    lastDir = currentDir;
                }
                else
                {
                    //ici on a changé d'angle ! (soit brutalement, soit avec le temps)
                    //ici prendre... la différence entre le dernier et ce dernier point testé
                    pointPivot = GetPosParticle(i);
                    distance = Vector3.Distance(playerManager.GetPosPlayer(indexPlayer), pointPivot);
                    numberParticleFromPlayerToPivot = numberParticleTested;

                    onRope = true;
                    return;
                }
            }
            //ici prendre... le premier player. (et la distance par rapport au dernier player)
            pointPivot = playerManager.GetPosPlayer(0);
            distance = Vector3.Distance(playerManager.GetPosPlayer(1), pointPivot);
            numberParticleFromPlayerToPivot = numberParticleTested;
            return;
        }
    }

    /// <summary>
    /// change la rope
    /// </summary>
    private void ModifyRope(float strain)
    {
        //ici quelqu'un est gripped, (ou pret pour ungrip), ne pas ajouter / diminuer pour l'instant
        //si on a commencé a raccourcir
        //si on a commencer a ralonger
        //   -> NON return;
        if (playerManager.IsSomeOneGripped() || grippedPrepareLessSetup || onGroundLenghting || !playerManager.AreBothGrounded())
        {
            
            //Debug.Log("ici quelqu'un est gripped, (ou pret pour ungrip), ne pas ajouter / diminuer pour l'instant");
            /*if (isGripped)
            {
                isGripped = false;
                Debug.Log("ici reset le grip");
            }*/
                
            return;
        }

        if (strain > stretchAdd && rope.PooledParticles > minParticle) //ici la rope est tendu !
        {
            /*
            if (playerManager.AreBothPlayerInOpositeInputDirection())
            {
                Debug.Log("add");
                ChangeParticleInRope(true, hookExtendRetractSpeed);
            }
            else
            {
                Debug.Log("ici n'ajoute pas si les 2 players sont dans la même direction...");
            }
            */
            Debug.Log("add");
            ChangeParticleInRope(true, hookExtendRetractSpeed);



            //cursor.normalizedCoord = 0.5f;
        }
        else if (strain < stretchRemove && rope.PooledParticles < maxParticle)
        {
            Debug.Log("less");
            ChangeParticleInRope(false, hookExtendRetractSpeed);
            //cursor.normalizedCoord = 0.5f;
        }
    }

    /// <summary>
    /// est appelé quand quelq'un s'est aggripé à un mur
    /// </summary>
    public void SomeOneGripToWall()
    {
        Debug.Log("ici quelqu'un s'est aggripé au mur !");
        isGripped = true;
    }

    /// <summary>
    /// ici est appelé dès qu'un player est gripped
    /// </summary>
    /// <param name="strain"></param>
    private void ShortCutRope(float strain)
    {
        if (!isGripped)
            return;

        grippedPrepareLessSetup = true;
        onGroundLenghting = false;          //reset le ralonge

        //ici il y a trop de particule, essay de diminuer
        if (particleInRope > resetToMinParticleNumberWhenGrip)
        {
            Debug.Log("less particle");
            ChangeParticleInRope(false, speedRemoveWhenGrip);
        }
        //si la corde n'est pas assez tendu
        else if (strain < stretchRemoveWhenGrip && rope.PooledParticles < maxParticleWhenGrip
                    && particleInRope > resetToMinParticleNumberWhenGripStretch)
        {
            Debug.Log("less tensity");
            ChangeParticleInRope(false, speedRemoveWhenGrip);
        }
        else
        {
            //ici stop quand on arrete le grip ! (il faut que les 2 player soit ungripped)

            isGripped = false;
        }
    }

    /// <summary>
    /// ajout ou supprime des particules de la rope, à un evitesse donné
    /// </summary>
    public void ChangeParticleInRope(bool addition, float speed, bool vibration = false)
    {
        if (addition)
        {
            if (vibration)
                AddVibration(true);
            cursor.ChangeLength(rope.RestLength + speed * Time.deltaTime);
        }
        else
        {
            if (vibration)
                AddVibration(false);
            cursor.ChangeLength(rope.RestLength - speed * Time.deltaTime);
        }
            
    }

    /// <summary>
    /// peut-on élargire la rope ?
    /// </summary>
    /// <returns></returns>
    private bool CanLenghening()
    {
        //si on a pas préparer le grip
        if (!grippedPrepareLessSetup)
        {
            //Debug.Log("grippedPrepareLessSetup est faux");
            return (false);
        }
            
        //ici on a grip, mais c'est pas fini
        if (isGripped)
        {
            //Debug.Log("on est gripped...");
            return (false);
        }
            

        //Debug.Log("ici.");
        //si quelqu'un est grippé, ne rien faire pour l'instant...
        if (playerManager.IsSomeOneGripped())
        {
            //Debug.Log("l'un des deux est gripped, ne rien faire..");
            return (false);
        }
            
        //Debug.Log("ici..");
        //ici le cooldown de l'un n'est pas pret
        if (!playerManager.AreBothGrippedEndAndCoolDownReady())
        {
            //Debug.Log("ici le cooldown de l'un n'est pas pret");
            return (false);
        }
        //Debug.Log("ici...");
        //ici les 2 players ne sont pas encore au sol, ne pas rallonger
        if (!playerManager.AreBothGroundedOn(CollisionSimple.Ground))
        {
            //Debug.Log("ici les 2 players ne sont pas encore au sol...");
            return (false);
        }
        //Debug.Log("ici.....");
        return (true);
    }

    /// <summary>
    /// essai de ralonger la rope en place lorsque le grip est fini et que
    /// les 2 joueurs sont grounded (ou groundedException)
    /// </summary>
    private void SetupLengtheningRope()
    {
        if (!CanLenghening())
            return;

        Debug.Log("ici setup onGroundLenghting");

        //ici on a gripped, raccourci, et on est maintenant les 2 jouerus sont au sol (ou l'un sur l'autre)
        //Debug.Log("ici ralonger yeah ! on est pas grip (et les cooldown sont ok), on est en mode grippedPrepareLess, et les 2 players sont sur le sol");
        grippedPrepareLessSetup = false;

        onGroundLenghting = true;
    }

    /// <summary>
    /// ici effectue une vibration, selon si on ajoute, on remove
    /// //cree un coolDown ?
    /// </summary>
    /// <param name="onAdd"></param>
    private void AddVibration(bool addition)
    {
        if (addition)
        {
            PlayerConnected.Instance.setVibrationPlayer(playerManager.PlayerControllerScript[0].IdPlayer, onAdd); //set vibration
            PlayerConnected.Instance.setVibrationPlayer(playerManager.PlayerControllerScript[1].IdPlayer, onAdd); //set vibration
        }
        else
        {
            PlayerConnected.Instance.setVibrationPlayer(playerManager.PlayerControllerScript[0].IdPlayer, onRemove); //set vibration
            PlayerConnected.Instance.setVibrationPlayer(playerManager.PlayerControllerScript[1].IdPlayer, onRemove); //set vibration
        }
    }

    /// <summary>
    /// ici on raccourci la rope ! (on a activé !)
    /// </summary>
    private void LenghteningRope()
    {
        if (!onGroundLenghting)
            return;
        
        Debug.Log("on est en train d'allonger !");
        //AddVibration(true);

        if (particleInRope < numberParticleBase)
        {
            Debug.Log("more particle Ralonge");
            cursor.ChangeLength(rope.RestLength + speedAddAfterGrip * Time.deltaTime);
        }
        else
        {
            onGroundLenghting = false;
            Debug.Log("ralonge fini !!!");
        }
    }

    #endregion

    #region Unity ending functions

    private void Update()
    {
        float strain = rope.CalculateLength() / rope.RestLength;
        actualTensity = strain;

        
        SetNumberParticle();



        if (!activeRopeChange)
            return;

        isTenseForJump = (strain > stretchForJump) ? true : false;
        isTenseForAirMove = (strain > stretchForAirMove) ? true : false;
        isTenseForAdd = (strain > stretchAdd) ? true : false;
        isTenseForLess = (strain < stretchRemove) ? true : false;

        //si l'un ou les 2 son gripped, ne pas modifier la rope
        
        ModifyRope(strain);

        ShortCutRope(strain);   //essai de raccourcir la rope lors du grip
        SetupLengtheningRope();      //essai de setup le ralongage la rope en place lorsque le grip est fini et que les 2 joueurs sont grounded (ou groundedException)
        LenghteningRope();      //ralonge la rope !
    }

	#endregion
}
