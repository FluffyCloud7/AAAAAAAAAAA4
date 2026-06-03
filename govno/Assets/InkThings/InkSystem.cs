using UnityEngine;

public class InkSystem : MonoBehaviour
{
    [Header("Разрешение текстуры маски")]
    public int resolution = 512;
    public RenderTexture inkMask;

    [Header("Финальный Слот для Шейдера")]
    public Shader regenShader;

    [Header("Настройки автоматического восстановления")]
    public bool autoRegenerate = true;

    [Tooltip("Время задержки в секундах после последнего стирания перед тем, как чернила начнут течь назад")]
    public float regenDelay = 2.0f;

    [Tooltip("Скорость БЫСТРОГО стягивания чернил после того, как время ожидания вышло")]
    public float fastRegenSpeed = 0.4f;

    Renderer rend;
    private Material regenMat;
    private Material eraseMat;
    private RenderTextureFormat maskFormat = RenderTextureFormat.R8;

    private float delayTimer = 0f;

    void Start()
    {
        rend = GetComponent<Renderer>();

        if (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.R8))
        {
            maskFormat = RenderTextureFormat.ARGB32;
        }

        inkMask = new RenderTexture(resolution, resolution, 0, maskFormat, RenderTextureReadWrite.Linear);
        inkMask.Create();

        Graphics.Blit(Texture2D.whiteTexture, inkMask);
        rend.material.SetTexture("_Mask", inkMask);

        if (regenShader != null)
        {
            regenMat = new Material(regenShader);
        }

        Shader eShader = Shader.Find("Hidden/InkErase");
        if (eShader != null)
        {
            eraseMat = new Material(eShader);
        }
    }

    void Update()
    {
        if (!autoRegenerate || inkMask == null || regenMat == null) return;

        if (delayTimer > 0f)
        {
            delayTimer -= Time.deltaTime;
        }
        else
        {
            RegenerateInk();
        }
    }

    private void RegenerateInk()
    {
        regenMat.SetFloat("_RegenSpeed", fastRegenSpeed * Time.deltaTime);

        RenderTexture temp = RenderTexture.GetTemporary(inkMask.width, inkMask.height, 0, maskFormat, RenderTextureReadWrite.Linear);
        Graphics.Blit(inkMask, temp);
        Graphics.Blit(temp, inkMask, regenMat);
        RenderTexture.ReleaseTemporary(temp);
    }

    public void Erase(Vector2 uv, float radius)
    {
        delayTimer = regenDelay;

        if (eraseMat == null) return;

        eraseMat.SetVector("_ErasePos", new Vector4(uv.x, uv.y, radius, 0));

        RenderTexture temp = RenderTexture.GetTemporary(inkMask.width, inkMask.height, 0, maskFormat, RenderTextureReadWrite.Linear);
        Graphics.Blit(inkMask, temp);
        Graphics.Blit(temp, inkMask, eraseMat);
        RenderTexture.ReleaseTemporary(temp);
    }

    public bool HasInk(Vector2 uv)
    {
        RenderTexture.active = inkMask;
        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false, true);

        tex.ReadPixels(new Rect(uv.x * inkMask.width, uv.y * inkMask.height, 1, 1), 0, 0);
        tex.Apply();

        Color c = tex.GetPixel(0, 0);
        RenderTexture.active = null;
        Destroy(tex);

        return c.r > 0.1f;
    }
}