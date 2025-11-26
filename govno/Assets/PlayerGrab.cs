using UnityEngine;

public class PlayerGrabIso : MonoBehaviour
{
    public float holdDistance = 2f;      // �� ����� ���������� ������ ������
    public float moveSpeed = 10f;        // �������� ����������� ������������� �������
    public Transform holdParent;         // ����� ����� �������, ��� ������ �������

    private GameObject heldObject = null;
    private Rigidbody heldRb = null;

    public float grabRange = 2f;         // ������ ������ �������� ��� ��������

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldObject == null)
                TryGrab();
            else
                Drop();
        }

        if (heldObject != null)
            MoveHeldObject();
    }

    void TryGrab()
    {
        // ���� ��������� ������ � ����� "Grabbable" � ������� grabRange
        Collider[] hits = Physics.OverlapSphere(transform.position, grabRange);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Grabbable"))
            {
                heldObject = hit.gameObject;
                heldRb = heldObject.GetComponent<Rigidbody>();

                if (heldRb != null)
                {
                    heldRb.useGravity = false;
                    heldRb.linearVelocity = Vector3.zero;
                    heldRb.angularVelocity = Vector3.zero;
                }

                heldObject.transform.SetParent(holdParent);
                return; // ���� ������ ���� ������
            }
        }
    }

    void MoveHeldObject()
    {
        if (heldObject == null) return;

        Vector3 targetPos = holdParent.position + holdParent.forward * holdDistance;
        if (heldRb != null)
        {
            Vector3 moveDir = targetPos - heldObject.transform.position;
            heldRb.linearVelocity = moveDir * moveSpeed;
        }
        else
        {
            heldObject.transform.position = targetPos;
        }
    }

    void Drop()
    {
        if (heldObject != null && heldRb != null)
        {
            heldRb.useGravity = true;
            heldRb.linearVelocity = Vector3.zero;
            heldRb.angularVelocity = Vector3.zero;
        }

        if (heldObject != null)
            heldObject.transform.SetParent(null);

        heldObject = null;
        heldRb = null;
    }
}
