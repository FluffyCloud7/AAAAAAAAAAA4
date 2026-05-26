using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CinemachineFreeLook))]
public class FixFreeLookInput : MonoBehaviour
{
    void Start()
    {
        var freeLook = GetComponent<CinemachineFreeLook>();
        // Жёстко задаем имена осей при старте
        freeLook.m_XAxis.m_InputAxisName = "Mouse X";
        freeLook.m_YAxis.m_InputAxisName = "Mouse Y";

        // Убедимся, что чувствительность адекватная
        freeLook.m_XAxis.m_MaxSpeed = 150f;
        freeLook.m_YAxis.m_MaxSpeed = 0.5f;
    }
}