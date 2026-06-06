using UnityEngine;
using Cinemachine;
using UnityEngine.Rendering;

public class CameraDistanceMove : MonoBehaviour
{
    public Volume mouseVolume;
    public float effectSmooth = 4f;

    private float volumeWeight = 0f;

    public float farDistance = 7f;
    public float mouseTilt = 25f;
    public float smooth = 6f;

    [Header("Edge Panning Settings")]
    [Tooltip("Размер зоны у края экрана (0.1 = 10% от края экрана)")]
    public float edgeBoundary = 0.1f;
    [Tooltip("Максимальное расстояние сдвига от исходной точки")]
    public float maxPanDistance = 5f;
    [Tooltip("Скорость смещения камеры")]
    public float panSpeed = 8f;

    private CinemachineVirtualCamera vcam;
    private Cinemachine3rdPersonFollow thirdPerson;
    private CinemachineFramingTransposer framing;

    private float defaultDistance;
    private float defaultTilt;

    private float currentDistance;
    private float currentTilt;

    // Базовые оффсеты
    private Vector3 defaultShoulderOffset;
    private Vector3 defaultTrackedObjectOffset;

    // Накопленный сдвиг (в системе координат экрана: X - право/лево, Z - верх/низ)
    private Vector3 currentPanOffset;
    private Vector3 targetPanOffset;

    void Awake()
    {
        vcam = GetComponent<CinemachineVirtualCamera>();

        thirdPerson = vcam.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        framing = vcam.GetCinemachineComponent<CinemachineFramingTransposer>();

        defaultDistance = GetDistance();
        defaultTilt = vcam.transform.localEulerAngles.x;

        if (thirdPerson != null) defaultShoulderOffset = thirdPerson.ShoulderOffset;
        if (framing != null) defaultTrackedObjectOffset = framing.m_TrackedObjectOffset;

        currentDistance = defaultDistance;
        currentTilt = defaultTilt;

        if (mouseVolume != null)
            mouseVolume.weight = 0f;
    }

    void LateUpdate()
    {
        if (CursorManager.Instance == null) return;

        bool mouseMode = CursorManager.Instance.CurrentMode == InputMode.MouseGameplay;

        // 1. Дистанция и наклон
        float targetDistance = mouseMode ? farDistance : defaultDistance;
        float targetTilt = mouseMode ? mouseTilt : defaultTilt;

        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * smooth);
        currentTilt = Mathf.LerpAngle(currentTilt, targetTilt, Time.deltaTime * smooth);

        SetDistance(currentDistance);

        Quaternion targetRotation = Quaternion.Euler(currentTilt, 0f, 0f);
        vcam.transform.localRotation = targetRotation;

        // 2. Логика сдвига к краям экрана
        if (mouseMode)
        {
            CalculateEdgePanning();
        }
        else
        {
            targetPanOffset = Vector3.zero;
        }

        currentPanOffset = Vector3.Lerp(currentPanOffset, targetPanOffset, Time.deltaTime * panSpeed);
        ApplyOffset(currentPanOffset);

        // 3. Управление весом Volume
        float targetWeight = mouseMode ? 1f : 0f;
        volumeWeight = Mathf.Lerp(volumeWeight, targetWeight, Time.deltaTime * effectSmooth);

        if (mouseVolume != null)
            mouseVolume.weight = volumeWeight;
    }

    private void CalculateEdgePanning()
    {
        Vector3 mousePos = Input.mousePosition;

        float normalizedX = mousePos.x / Screen.width;
        float normalizedY = mousePos.y / Screen.height;

        Vector3 moveDirection = Vector3.zero;

        // Точно так же, как в первом (рабочем) коде:
        if (normalizedX >= 1f - edgeBoundary) moveDirection.x = 1f;
        else if (normalizedX <= edgeBoundary) moveDirection.x = -1f;

        if (normalizedY >= 1f - edgeBoundary) moveDirection.z = 1f;
        else if (normalizedY <= edgeBoundary) moveDirection.z = -1f;

        if (moveDirection.sqrMagnitude > 0)
        {
            moveDirection.Normalize();
            targetPanOffset += moveDirection * panSpeed * Time.deltaTime;
            targetPanOffset = Vector3.ClampMagnitude(targetPanOffset, maxPanDistance);
        }
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

    void ApplyOffset(Vector3 offset)
    {
        if (vcam.Follow == null) return;

        // СЕКРЕТ КОРРЕКТНОЙ РАБОТЫ:
        // Чтобы оффсет не зависел от поворота персонажа, мы берем наш чистый экранный сдвиг (offset)
        // и вращаем его обратно ПОД ПОВОРОТ ПЕРСОНАЖА. Таким образом, когда Cinemachine внутри себя
        // умножит оффсет на разворот персонажа, они взаимно уничтожатся, и камера сдвинется строго по экрану.
        Quaternion targetRotationCompensate = Quaternion.Inverse(vcam.Follow.rotation);
        Vector3 compensatedOffset = targetRotationCompensate * offset;

        // Убираем влияние наклона по высоте, оставляем только чистый плоский сдвиг
        compensatedOffset.y = 0f;

        if (thirdPerson != null)
        {
            thirdPerson.ShoulderOffset = defaultShoulderOffset + compensatedOffset;
        }
        else if (framing != null)
        {
            framing.m_TrackedObjectOffset = defaultTrackedObjectOffset + compensatedOffset;
        }
    }
}