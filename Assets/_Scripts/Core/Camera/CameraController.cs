using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

/// <summary>
/// Manage camera
/// Smoothly move camera to m_DesiredPosition
/// m_DesiredPosition is the barycenter of target list
/// </summary>
public class CameraController : MonoBehaviour
{
    #region Attributes

    // Time before next camera move
    [FoldoutGroup("GamePlay"), Tooltip("Le smooth de la caméra"), SerializeField]
    private float smoothTime = 0.2f;
    private float tmpSmoothTime;
    [FoldoutGroup("GamePlay"), Tooltip("Le smooth de la caméra"), SerializeField]
    private float smoothLittleTime = 0.2f;
    [FoldoutGroup("GamePlay"), Tooltip("Le smooth de la caméra"), SerializeField]
    private float speedLerpSmooth = 1f;

    private bool isAlways = false;
    public void SetAlways(bool always)
    {
        if (always)
        {
            Debug.Log("little");
            
            isAlways = true;
            smoothTime = smoothLittleTime;
        }
        else
        {
            Debug.Log("revenir à la normal");
            //smoothTime = tmpSmoothTime;
            isAlways = false;
        }
    }
    private void LerpSmooth()
    {
        if (!isAlways && smoothTime != tmpSmoothTime)
        {
            smoothTime = Mathf.Lerp(smoothTime, tmpSmoothTime, Time.deltaTime * speedLerpSmooth);
        }
    }

    [FoldoutGroup("GamePlay"), Tooltip("le zoom minimum de la camera"), SerializeField]
    private float minZoom = 4.0f;

    [FoldoutGroup("GamePlay"), Tooltip("le zoom maximum de la camera (default)"), SerializeField]
    private float maxZoom = 15.0f;

    // Zoom applied with only one target
    [FoldoutGroup("GamePlay"), Tooltip("Zoom appliqué lorsqu'il y a une seul target"), SerializeField]
    private float defaultZoom = 6.0f;

    // Target approximation threshold
    [FoldoutGroup("GamePlay"), Tooltip("Approximation: la caméra est-elle sur sa cible ?"), SerializeField]
    private float focusThreshold = 0f;

    // Border margin before unzoom
    [FoldoutGroup("GamePlay"), Tooltip("Border margin de l'axe Z du zoom"), SerializeField]
    private float borderMargin = 4f;

    //Fallback target if target list is empty
    [FoldoutGroup("GamePlay"), Space(10), Tooltip("objet que la caméra doit focus s'il n'y a plus de target"), SerializeField]
    private Transform fallBackTarget;

    [FoldoutGroup("GamePlay"), Space(10), Tooltip("En combien de temps la caméra se décide à focus le fallBack lors du game over......"), SerializeField]
    private float timeBeforeFallBack = 1f;

    [FoldoutGroup("GamePlay"), Space(10), Tooltip("En combien de temps la caméra se décide à focus le fallBack lors du game over......"), SerializeField]
    private float smoothTimeWhenFallBack = 1f;


    //Target list
    [FoldoutGroup("Debug"), Tooltip("list de target"), SerializeField]
    private List<CameraTarget> targetList = new List<CameraTarget>();
    [FoldoutGroup("Debug"), Tooltip("Des que c'est vrai, la camera focus la cameraTarget quand elle n'a plus de cible"), SerializeField]
    private bool fallBack = false;

	public float offset = 1; //Rajoute par guilaume pour décentrer la caméra sur y :)

    [SerializeField]
	private FrequencyTimer updateTimer;

	private Vector3 currentVelocity;
	private bool freezeCamera = false;
	private Vector3 averageTargetPosition;
	public Vector3 TargetPosition
	{
		get { return averageTargetPosition; }
	}
    private float holdSmooth = 0;

    #endregion

    #region Init
    private void OnEnable()
    {
        EventManager.StartListening(GameData.Event.GameOver, GameOver);
    }

    private void Start()
    {
        InitCamera();
    }

    private void InitCamera()
    {
        CancelInvoke("FallBack");
        if (holdSmooth == 0)
            holdSmooth = smoothTime;
        else
            smoothTime = holdSmooth;

        tmpSmoothTime = smoothTime;
    }
    #endregion

    #region Core
    /// <summary>
    /// fonction appelé lorsque la partie est fini
    /// </summary>
    private void GameOver()
    {
        Invoke("FallBack", timeBeforeFallBack);
    }

    private void FallBack()
    {
        fallBack = true;
        ClearTarget();
        smoothTime = smoothTimeWhenFallBack;
        Debug.Log("la camera bouge au fallback !");
    }

    /// <summary>
    /// Add target to camera
    /// </summary>
    public void AddTarget(CameraTarget other)
    {
		// Check object is not already a target
        if (targetList.IndexOf(other) < 0)
        {
            freezeCamera = false;
            targetList.Add(other);
        }
    }

    /// <summary>
    /// Remove target from target list
    /// </summary>
	public void RemoveTarget(CameraTarget other)
    {
        for (int i = 0; i < targetList.Count; i++)
        {
            if (targetList[i].GetInstanceID() == other.GetInstanceID())
            {
                targetList.RemoveAt(i);
                return;
            }
        }
    }

    /// <summary>
    /// clean la list des targets
    /// </summary>
    private void CleanListTarget()
    {
        for (int i = 0; i < targetList.Count; i++)
        {
            if (!targetList[i])
                targetList.RemoveAt(i);
        }
        SetFreez();
    }

    /// <summary>
    /// Clear targets
    /// </summary>
    public void ClearTarget()
    {
        targetList.Clear();
    }

    /// <summary>
    /// Calculate new camera position
    /// </summary>
    private void FindAveragePosition()
    {
		// Final position
        Vector3 averagePos = new Vector3();
		int activeTargetAmount = 0;
        float minX = 0; 
        float maxX = 0;
        float minY = 0;
        float maxY = 0;

        // For each target
        for (int i = 0; i < targetList.Count; i++)
        {
			CameraTarget target = targetList[i];

			// Check target is active
			if (!target || !target.gameObject.activeSelf)
			{
				continue;
			}

			// Set first target as min max position
            if (i == 0)
            {
                minX = maxX = target.transform.position.x;
                minY = maxY = target.transform.position.y;
            }
            else
            {
				// Extends min max bounds
                minX = (target.transform.position.x < minX) ? target.transform.position.x : minX;
                maxX = (target.transform.position.x > maxX) ? target.transform.position.x : maxX;
                minY = (target.transform.position.y < minY) ? target.transform.position.y : minY;
                maxY = (target.transform.position.y > maxY) ? target.transform.position.y : maxY;
            }
				
            activeTargetAmount++;
        }

        // Find middle point for all targets
        if (activeTargetAmount > 0)
        {
            averagePos.x = (minX + maxX) / 2.0F;
            averagePos.y = (minY + maxY) / 2.0F;
        }

        // If no targets, select fallback focus
        if (targetList.Count == 0)
        {
            if (fallBackTarget && fallBack)
            {
                averagePos = fallBackTarget.position;
            }
        }

        // Calculate zoom
        float dist = Mathf.Max(Mathf.Abs(maxX - minX), Mathf.Abs(maxY - minY));
        averagePos.z = (targetList.Count > 1) ? -Mathf.Min(Mathf.Max(minZoom, dist + borderMargin), maxZoom) : -defaultZoom;

        // Change camera target
		averageTargetPosition = averagePos + new Vector3(0,offset,0);
    }

    /// <summary>
    /// Initialize camera
    /// </summary>
    public void InitializeCamera()
    {
        FindAveragePosition();
		transform.position = averageTargetPosition;
    }

    /// <summary>
    /// Check camera is placed on target position
    /// </summary>
    /// <returns></returns>
    private bool HasReachedTargetPosition()
    {
		float x = transform.position.x;
		float y = transform.position.y;
       
		return x > averageTargetPosition.x - focusThreshold && x < averageTargetPosition.x + focusThreshold && y > averageTargetPosition.y - focusThreshold && y < averageTargetPosition.y + focusThreshold;
    }

    /// <summary>
    /// setup le freez de la camera...
    /// </summary>
    private void SetFreez()
    {
        freezeCamera = targetList.Count == 0 && ((fallBackTarget && !fallBack) || fallBackTarget == null);
    }

    #endregion

    #region unity ending
    private void Update()
    {
        SetFreez();
        LerpSmooth();

        if (updateTimer.Ready())
        {
            CleanListTarget();

            if (freezeCamera)
			{
				return;
			}

            FindAveragePosition();
        }
    }

    /// <summary>
    /// Smoothly move camera toward averageTargetPosition
    /// </summary>
    private void FixedUpdate()
    {
        if (freezeCamera || HasReachedTargetPosition())
        {
            return;
        }

        // Move to desired position
        transform.position = Vector3.SmoothDamp(transform.position, averageTargetPosition, ref currentVelocity, smoothTime);
        //posLisener = transform.position;    //change listenerPosition
    }

    private void OnDisable()
    {
        EventManager.StopListening(GameData.Event.GameOver, GameOver);
    }
    #endregion
}