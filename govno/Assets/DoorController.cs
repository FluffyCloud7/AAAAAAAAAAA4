using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour
{
    public DoorOpenType openType;

    public Vector3 slideDirection = Vector3.right;
    public float slideDistance = 3f;

    public Vector3 rotationAxis = Vector3.up;
    public float rotationAngle = 90f;

    public float openSpeed = 2f;

    private Vector3 closedPosition;
    private Quaternion closedRotation;

    private Coroutine currentRoutine;

    private void Awake()
    {
        closedPosition = transform.localPosition;
        closedRotation = transform.localRotation;
    }

    public void Open()
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(OpenRoutine());
    }

    public void Close()
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(CloseRoutine());
    }

    IEnumerator OpenRoutine()
    {
        float t = 0;

        Vector3 targetPos = closedPosition + slideDirection.normalized * slideDistance;
        Quaternion targetRot = closedRotation * Quaternion.AngleAxis(rotationAngle, rotationAxis);

        while (t < 1)
        {
            t += Time.deltaTime * openSpeed;

            if (openType == DoorOpenType.SlideHorizontal || openType == DoorOpenType.SlideVertical)
                transform.localPosition = Vector3.Lerp(closedPosition, targetPos, t);

            if (openType == DoorOpenType.Rotate)
                transform.localRotation = Quaternion.Slerp(closedRotation, targetRot, t);

            yield return null;
        }
    }

    IEnumerator CloseRoutine()
    {
        float t = 0;

        Vector3 openPos = closedPosition + slideDirection.normalized * slideDistance;
        Quaternion openRot = closedRotation * Quaternion.AngleAxis(rotationAngle, rotationAxis);

        while (t < 1)
        {
            t += Time.deltaTime * openSpeed;

            if (openType == DoorOpenType.SlideHorizontal || openType == DoorOpenType.SlideVertical)
                transform.localPosition = Vector3.Lerp(openPos, closedPosition, t);

            if (openType == DoorOpenType.Rotate)
                transform.localRotation = Quaternion.Slerp(openRot, closedRotation, t);

            yield return null;
        }
    }
}