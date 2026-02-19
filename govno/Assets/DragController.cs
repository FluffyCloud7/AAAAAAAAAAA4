using UnityEngine;


public class DragController : MonoBehaviour
{
    [SerializeField] private LayerMask draggableLayer;

    [SerializeField] private float scrollSpeed = 2f;
    [SerializeField] private float minDistance = 0.5f;
    [SerializeField] private float maxDistance = 20f;


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
        if (CursorManager.Instance == null) return;

        if (CursorManager.Instance.CurrentMode != InputMode.MouseGameplay)
            return;

        HandleDragging();
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
        float scroll = Input.mouseScrollDelta.y;
        if (scroll != 0f)
        {
            objectDistance += scroll * scrollSpeed;
            objectDistance = Mathf.Clamp(objectDistance, minDistance, maxDistance);
        }


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
