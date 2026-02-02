using UnityEngine;


public class DragController : MonoBehaviour
{
    [SerializeField] private float dragDistance = 10f;
    [SerializeField] private LayerMask draggableLayer;

    private Camera cam;
    private Rigidbody draggedObject;
    private float objectDistance;

    private bool isDragging = false;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        // ������������ ������ �� R
        if (Input.GetKeyDown(KeyCode.R))
        {
            ToggleDragMode();
        }

        if (!Cursor.visible) return;

        HandleDragging();
    }

    private void ToggleDragMode()
    {
        if (Cursor.visible)
        {
            CursorManager.Instance.SetMode(InputMode.Gameplay);
            ReleaseObject();
        }
        else
        {
            CursorManager.Instance.SetMode(InputMode.UI);
        }
    }

    private void HandleDragging()
    {
        if (Input.GetMouseButtonDown(0))
            TryPickObject();

        if (Input.GetMouseButtonUp(0))
            ReleaseObject();

        if (isDragging)
            DragObject();
    }

    private void TryPickObject()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, draggableLayer))
        {
            if (hit.rigidbody != null)
            {
                draggedObject = hit.rigidbody;
                draggedObject.useGravity = false;
                draggedObject.linearDamping = 10f;

                objectDistance = hit.distance;
                isDragging = true;
            }
        }
    }

    private void DragObject()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = objectDistance;

        Vector3 worldPos = cam.ScreenToWorldPoint(mousePos);
        draggedObject.MovePosition(worldPos);
    }

    private void ReleaseObject()
    {
        if (draggedObject == null) return;

        draggedObject.useGravity = true;
        draggedObject.linearDamping = 0f;
        draggedObject = null;
        isDragging = false;
    }
}
