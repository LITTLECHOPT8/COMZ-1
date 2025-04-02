using UnityEngine;

[RequireComponent(typeof(Camera))]
public class TUICamera : MonoBehaviour
{
    public Rect m_viewRect;
    public float scrollSpeed = 0.05f;  // Scroll speed
    public float snapThreshold = 100f;  // Snap threshold
    public float snapSpeed = 5f;  // Speed of snapping
    private Vector3 initialCameraPosition;
    private Vector3 dragStartPosition;
    private bool isSnapping = false;
    private float targetPositionX;

    public void Initialize(bool landscape, int layer, int depth)
    {
        float height;
        bool hd;
        float width;
        GetScreenInfo(out width, out height, out hd);

        if (landscape)
        {
            float num = width;
            width = height;
            height = num;
        }

        base.transform.localPosition = Vector3.zero;
        base.transform.localRotation = Quaternion.identity;
        base.transform.localScale = Vector3.one;
        base.GetComponent<Camera>().transform.localPosition = new Vector3(1f / ((!hd) ? 2f : 4f), -1f / ((!hd) ? 2f : 4f), 0f);
        base.GetComponent<Camera>().clearFlags = CameraClearFlags.Nothing;
        base.GetComponent<Camera>().backgroundColor = Color.white;
        base.GetComponent<Camera>().nearClipPlane = -128f;
        base.GetComponent<Camera>().farClipPlane = 128f;
        base.GetComponent<Camera>().orthographic = true;
        base.GetComponent<Camera>().depth = depth;
        base.GetComponent<Camera>().cullingMask = 1 << layer;
        m_viewRect = new Rect(0f, 0f, Screen.width, Screen.height);
        base.GetComponent<Camera>().pixelRect = m_viewRect;

        if (Application.loadedLevelName.StartsWith("Zombie3D_"))
        {
            base.GetComponent<Camera>().aspect = m_viewRect.width / m_viewRect.height;
            if (hd)
            {
                base.GetComponent<Camera>().orthographicSize = m_viewRect.height / 4f;
            }
            else
            {
                base.GetComponent<Camera>().orthographicSize = height / 2f;
            }
        }
        else if (Screen.width >= 960 && Screen.height >= 640)
        {
            float left = ((float)Screen.width - 960f) / 2f;
            float top = ((float)Screen.height - 640f) / 2f;
            m_viewRect = new Rect(left, top, 960f, 640f);
            base.GetComponent<Camera>().aspect = m_viewRect.width / m_viewRect.height;
            base.GetComponent<Camera>().orthographicSize = m_viewRect.height / ((!hd) ? 2f : 4f);
        }
        else if (Screen.width >= 640 && Screen.height >= 960)
        {
            float left2 = ((float)Screen.width - 640f) / 2f;
            float top2 = ((float)Screen.height - 960f) / 2f;
            m_viewRect = new Rect(left2, top2, 640f, 960f);
            base.GetComponent<Camera>().aspect = m_viewRect.width / m_viewRect.height;
            base.GetComponent<Camera>().orthographicSize = m_viewRect.height / ((!hd) ? 2f : 4f);
        }
        else
        {
            base.GetComponent<Camera>().aspect = m_viewRect.width / m_viewRect.height;
            base.GetComponent<Camera>().orthographicSize = height / ((!hd) ? 2f : 4f);
        }
    }

    void Update()
    {
        // Use Application.loadedLevel to check the scene name
        if (Application.loadedLevelName == "MainMapTUI")
        {
            HandleCameraScrolling();
        }
    }

    private void HandleCameraScrolling()
    {
        if (isSnapping)
        {
            // Smooth transition to the target snap position
            transform.position = Vector3.Lerp(transform.position, new Vector3(targetPositionX, transform.position.y, transform.position.z), snapSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, new Vector3(targetPositionX, transform.position.y, transform.position.z)) < 0.1f)
            {
                isSnapping = false;
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0)) // Detect mouse or touch down
            {
                dragStartPosition = Input.mousePosition;
                initialCameraPosition = transform.position;
            }
            else if (Input.GetMouseButton(0)) // Detect mouse or touch drag
            {
                // Calculate drag distance
                float deltaX = (Input.mousePosition.x - dragStartPosition.x) * scrollSpeed;

                // Move camera smoothly based on drag distance
                Vector3 newCameraPosition = initialCameraPosition;
                newCameraPosition.x -= deltaX;
                transform.position = newCameraPosition;
            }
            else if (Input.GetMouseButtonUp(0)) // When drag ends, check for snap
            {
                CheckForSnap();
            }
        }
    }

    private void CheckForSnap()
    {
        float currentPositionX = transform.position.x;

        // Check if the camera is close enough to snap to the next position
        if (Mathf.Abs(currentPositionX % m_viewRect.width) > snapThreshold)
        {
            // Snap to the next position
            targetPositionX = Mathf.Round(currentPositionX / m_viewRect.width) * m_viewRect.width;
            isSnapping = true;
        }
    }

    private void GetScreenInfo(out float width, out float height, out bool hd)
    {
        width = 0f;
        height = 0f;
        hd = false;
        if (Application.isPlaying)
        {
            if (Mathf.Max(Screen.width, Screen.height) > 1000)
            {
                if (Mathf.Min(Screen.width, Screen.height) > 700)
                {
                    width = 768f;
                    height = 1024f;
                }
                else
                {
                    width = 640f;
                    height = 960f;
                }
                hd = true;
            }
            else if (Mathf.Max(Screen.width, Screen.height) > 900)
            {
                width = 640f;
                height = 960f;
                hd = true;
            }
            else
            {
                width = 320f;
                height = 480f;
                hd = false;
            }
        }
        else
        {
            width = 320f;
            height = 480f;
            hd = false;
        }
    }
}
