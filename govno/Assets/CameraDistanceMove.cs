using UnityEngine;
using Cinemachine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraDistanceMove : MonoBehaviour
{
    public Volume mouseVolume;
    public float effectSmooth = 4f;

    private float volumeWeight = 0f;

    public float farDistance = 7f;
    public float mouseTilt = 25f;
    public float smooth = 6f;

    private CinemachineVirtualCamera vcam;
    private Cinemachine3rdPersonFollow thirdPerson;
    private CinemachineFramingTransposer framing;

    private float defaultDistance;
    private float defaultTilt;

    private float currentDistance;
    private float currentTilt;

    void Awake()
    {
        vcam = GetComponent<CinemachineVirtualCamera>();

        thirdPerson = vcam.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        framing = vcam.GetCinemachineComponent<CinemachineFramingTransposer>();

        defaultDistance = GetDistance();
        defaultTilt = vcam.transform.localEulerAngles.x;

        currentDistance = defaultDistance;
        currentTilt = defaultTilt;

        // ВАЖНО: Volume всегда активен, просто ставим вес 0
        if (mouseVolume != null)
            mouseVolume.weight = 0f;
    }

    void LateUpdate()
    {
        if (CursorManager.Instance == null) return;

        bool mouseMode =
            CursorManager.Instance.CurrentMode == InputMode.MouseGameplay;

        float targetDistance = mouseMode ? farDistance : defaultDistance;
        float targetTilt = mouseMode ? mouseTilt : defaultTilt;

        currentDistance = Mathf.Lerp(
            currentDistance,
            targetDistance,
            Time.deltaTime * smooth
        );

        currentTilt = Mathf.LerpAngle(
            currentTilt,
            targetTilt,
            Time.deltaTime * smooth
        );

        SetDistance(currentDistance);

        // Меняем наклон аккуратно
        Quaternion targetRotation = Quaternion.Euler(currentTilt, 0f, 0f);
        vcam.transform.localRotation = targetRotation;

        // Управляем ТОЛЬКО весом Volume
        float targetWeight = mouseMode ? 1f : 0f;

        volumeWeight = Mathf.Lerp(
            volumeWeight,
            targetWeight,
            Time.deltaTime * effectSmooth
        );

        if (mouseVolume != null)
            mouseVolume.weight = volumeWeight;
    }

    float GetDistance()
    {
        if (thirdPerson != null) return thirdPerson.CameraDistance;
        if (framing != null) return framing.m_CameraDistance;
        return 0f;
    }

    void SetDistance(float value)
    {
        if (thirdPerson != null) thirdPerson.CameraDistance = value;
        if (framing != null) framing.m_CameraDistance = value;
    }
}