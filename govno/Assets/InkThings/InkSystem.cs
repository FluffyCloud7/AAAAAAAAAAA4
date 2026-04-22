using UnityEngine;

public class InkSystem : MonoBehaviour
{
    public int resolution = 512;

    public RenderTexture inkMask;

    Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();

        inkMask = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.R8);
        inkMask.Create();

        Graphics.Blit(Texture2D.whiteTexture, inkMask);

        rend.material.SetTexture("_Mask", inkMask);
    }

    public void Erase(Vector2 uv, float radius)
    {
        Material eraseMat = new Material(Shader.Find("Hidden/InkErase"));

        eraseMat.SetVector("_ErasePos", new Vector4(uv.x, uv.y, radius, 0));

        RenderTexture temp = RenderTexture.GetTemporary(inkMask.width, inkMask.height);

        Graphics.Blit(inkMask, temp);
        Graphics.Blit(temp, inkMask, eraseMat);

        RenderTexture.ReleaseTemporary(temp);
    }

    public bool HasInk(Vector2 uv)
    {
        RenderTexture.active = inkMask;

        Texture2D tex = new Texture2D(1, 1, TextureFormat.R8, false);

        tex.ReadPixels(new Rect(
            uv.x * inkMask.width,
            uv.y * inkMask.height,
            1,
            1), 0, 0);

        tex.Apply();

        Color c = tex.GetPixel(0, 0);

        RenderTexture.active = null;

        Destroy(tex);

        return c.r > 0.1f;
    }
}