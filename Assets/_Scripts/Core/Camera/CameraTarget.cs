using UnityEngine;

/// <summary>
/// Add target to camera
/// <summary>
public class CameraTarget : MonoBehaviour
{
    #region Attributes

	[SerializeField]
	private bool onEnableAdd = true;

	[SerializeField]
	private bool onDisableRemove = true;

    [SerializeField]
    private bool alwaysOnCamera = false;


    [SerializeField]
    private CameraController cameraController;

    #endregion

    #region Initialisation
    private void Awake()
    {
        cameraController = GameManager.Instance.CameraObject.GetComponent<CameraController>();
    }
    #endregion

    #region Core

    public void SetAlways(bool always)
    {
        //Debug.Log("ici active: " + always);
        if (always != alwaysOnCamera)
        {
            alwaysOnCamera = always;
            if (alwaysOnCamera)
            {
                AddTarget();
            }
            else
            {
                RemoveTarget();
            }
        }
    }

    public void TestAlways()
    {

    }

    public void AddTarget()
	{
        cameraController.AddTarget (this);
	}

	public void RemoveTarget()
	{
        if (cameraController)
		{
            cameraController.RemoveTarget (this);
		}
	}

	// Unity functions
	private void OnEnable()
	{
		if (onEnableAdd)
		{
			AddTarget ();
		}
	}

    private void OnDisable()
    {
		if (onDisableRemove)
		{
			RemoveTarget ();
		}
    }

	private void OnDestroy()
	{
		RemoveTarget ();
	}

    private void Update()
    {
        TestAlways();
    }

    #endregion
}
