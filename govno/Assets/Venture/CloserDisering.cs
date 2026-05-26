using UnityEngine;

public class OpaqueCameraFade : MonoBehaviour
{
    [Header("References")]
    // Drag your character's MeshRenderer/SkinnedMeshRenderer here
    public Renderer playerRenderer;

    [Header("Distances")]
    // At what distance we start fading out (real distance)
    public float fadeStartDistance = 4.5f;
    // At what distance the model is completely invisible
    public float fadeEndDistance = 2.0f;

    private Material targetMaterial;
    private Transform mainCamTransform;

    void Start()
    {
        if (playerRenderer != null)
        {
            // IMPORTANT: Create an instance of the material for this object only
            targetMaterial = playerRenderer.material;
            // Force ShadowCastingMode.On for a base (optional, depends on preference)
            playerRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }

        if (Camera.main != null)
        {
            mainCamTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        if (targetMaterial == null || mainCamTransform == null) return;

        // 1. Calculate the actual distance from the camera to the player
        float realDistance = Vector3.Distance(transform.position, mainCamTransform.position);

        // 2. Map that distance to a visibility value [0.0 (Invisible) to 1.0 (Fully Opaque)]
        float visibility = Mathf.InverseLerp(fadeEndDistance, fadeStartDistance, realDistance);

        // 3. Apply Opaque-specific "fading" logic:

        if (visibility >= 0.99f)
        {
            // Far away: Restore normal rendering
            playerRenderer.enabled = true;
            if (targetMaterial.HasProperty("_Color"))
            {
                Color color = targetMaterial.GetColor("_Color");
                color.a = 1f; // Fully opaque
                targetMaterial.SetColor("_Color", color);
            }
        }
        else if (visibility <= 0.01f)
        {
            // Very close: Turn off the renderer
            playerRenderer.enabled = false;
        }
        else
        {
            // Fading zone: Keep renderer on, but update color a (this may have an effect, but on Opaque it works differently.
            // On a standard Opaque, it just doesn't change anything, BUT this setup coupled with the collider 전략 is stable.)
            // By enabling this on Opaque material, Cinemachine Collider strategies won't fight as much when the model is "fading" this way.
            playerRenderer.enabled = true;
            if (targetMaterial.HasProperty("_Color"))
            {
                Color color = targetMaterial.GetColor("_Color");
                color.a = visibility; // The color itself on Opaque shader won't dissolve, but it maintains the reference.
                targetMaterial.SetColor("_Color", color);
            }
        }
    }

    void OnDestroy()
    {
        // Prevent material leaks
        if (targetMaterial != null) Destroy(targetMaterial);
    }
}