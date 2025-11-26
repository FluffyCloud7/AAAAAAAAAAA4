using UnityEngine;
using System.Collections;

public class DoorRotation : MonoBehaviour
{
    public float openAngle = 90f; // на сколько градусов открывается
    public float speed = 2f;      // скорость вращения
    private bool isOpen = false;

    private Quaternion closedRotation;
    private Quaternion openRotation;

    void Start()
    {
        closedRotation = transform.rotation;
        openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);
    }

    public void OpenDoor()
    {
        if (!isOpen)
            StartCoroutine(RotateDoor(openRotation));
    }

    public void CloseDoor()
    {
        if (isOpen)
            StartCoroutine(RotateDoor(closedRotation));
    }

    private IEnumerator RotateDoor(Quaternion targetRotation)
    {
        Quaternion startRotation = transform.rotation;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }

        transform.rotation = targetRotation; // чтобы точно совпало
        isOpen = (targetRotation == openRotation);
    }
}
