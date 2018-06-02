using UnityEngine;

/// <summary>
/// Check object is on camera
/// <summary>
public class IsOnCamera : MonoBehaviour
{
	[SerializeField]
	private FrequencyTimer updateTimer = new FrequencyTimer(1.0F);

	[SerializeField]
	private float xMargin;

	[SerializeField]
	private float yMargin;

	public bool isOnScreen = false;
	private Renderer objectRenderer;
    private Camera cam;


    #region Initialization
    private void Awake()
	{
        cam = GameManager.Instance.CameraMain;
        objectRenderer = GetComponent<Renderer> ();
	}
	#endregion

    #region Core

	/// <summary>
	/// Check object is on screen
	/// <summary>
	bool CheckOnCamera()
	{
		if (!cam)
		{
			return false;
		}

		Vector3 bottomCorner = cam.WorldToViewportPoint(gameObject.transform.position - objectRenderer.bounds.extents);
		Vector3 topCorner = cam.WorldToViewportPoint(gameObject.transform.position + objectRenderer.bounds.extents);

		return (topCorner.x >= -xMargin && bottomCorner.x <= 1 + xMargin && topCorner.y >= -yMargin && bottomCorner.y <= 1 + yMargin);
	}

	// Unity functions

    private void Update()
    {
		if (updateTimer.Ready())
        {
			isOnScreen = CheckOnCamera();
        }
    }
	#endregion
}
