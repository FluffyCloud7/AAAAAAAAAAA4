using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(Collider))]
public class RigidPageCurl : MonoBehaviour
{
    public enum PageOrientation
    {
        Horizontal,
        Vertical
    }

    [Header("Page Setup")]
    public PageOrientation orientation = PageOrientation.Horizontal;
    public LayerMask paperLayer;

    [Header("Curl Settings")]
    public float maxAngle = 180f;
    public float sensitivity = 200f;
    public float returnSpeed = 5f;

    private Mesh mesh;
    private Vector3[] originalVerts;
    private Vector3[] deformedVerts;

    private Camera cam;
    private bool dragging;
    private Vector3 grabPointLocal;
    private float currentAngle;

    void Start()
    {
        cam = Camera.main;

        mesh = GetComponent<MeshFilter>().mesh;
        originalVerts = mesh.vertices;
        deformedVerts = new Vector3[originalVerts.Length];

        mesh.MarkDynamic();
    }

    void Update()
    {
        HandleInput();
        DeformMesh();
        CheckCursorHover();
    }

    void HandleInput()
    {
        if (cam == null)
            cam = Camera.main;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, paperLayer))
            {
                if (hit.transform == transform)
                {
                    dragging = true;
                    grabPointLocal = transform.InverseTransformPoint(hit.point);
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
            dragging = false;

        if (dragging)
        {
            float move = Input.GetAxis("Mouse Y");
            currentAngle += move * sensitivity * Time.deltaTime;
            currentAngle = Mathf.Clamp(currentAngle, 0, maxAngle);
        }
        else
        {
            currentAngle = Mathf.Lerp(currentAngle, 0f, Time.deltaTime * returnSpeed);
        }
    }

    void CheckCursorHover()
    {
        // Если страницу тащат, принудительно заставляем курсор светиться фиолетовым
        if (dragging && CursorVisualController.Instance != null)
        {
            CursorVisualController.Instance.SetInteractableState(true);
        }
    }

    void DeformMesh()
    {
        float foldLine;

        if (orientation == PageOrientation.Horizontal)
            foldLine = grabPointLocal.x;
        else
            foldLine = grabPointLocal.y;

        for (int i = 0; i < originalVerts.Length; i++)
        {
            Vector3 v = originalVerts[i];

            bool shouldFold;

            if (orientation == PageOrientation.Horizontal)
                shouldFold = v.x > foldLine;
            else
                shouldFold = v.y > foldLine;

            if (shouldFold)
            {
                Vector3 pivot;

                if (orientation == PageOrientation.Horizontal)
                    pivot = new Vector3(foldLine, 0, 0);
                else
                    pivot = new Vector3(0, foldLine, 0);

                Vector3 offset = v - pivot;

                Quaternion rot;

                if (orientation == PageOrientation.Horizontal)
                    rot = Quaternion.AngleAxis(currentAngle, Vector3.forward);
                else
                    rot = Quaternion.AngleAxis(currentAngle, Vector3.right);

                offset = rot * offset;
                v = pivot + offset;
            }

            deformedVerts[i] = v;
        }

        mesh.vertices = deformedVerts;
        mesh.RecalculateNormals();
    }
}