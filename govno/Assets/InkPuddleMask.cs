using UnityEngine;

public class InkPuddleMask : MonoBehaviour
{
    Texture2D runtimeTexture;
    Color[] pixels;

    Renderer rend;

    public Texture2D baseTexture;

    int texWidth;
    int texHeight;

    void Start()
    {
        Debug.Log(baseTexture);
        rend = GetComponent<Renderer>();

        texWidth = baseTexture.width;
        texHeight = baseTexture.height;

        runtimeTexture = new Texture2D(texWidth, texHeight, TextureFormat.RGBA32, false);
        runtimeTexture.wrapMode = TextureWrapMode.Repeat;

        pixels = baseTexture.GetPixels();

        runtimeTexture.SetPixels(pixels);
        runtimeTexture.Apply();

        rend.material.SetTexture("_MaskTexture", runtimeTexture);
    }

    public void Erase(Vector2 uv, float radius)
    {
        uv.x = Mathf.Clamp01(uv.x);
        uv.y = Mathf.Clamp01(uv.y);

        int centerX = Mathf.RoundToInt(uv.x * texWidth);
        int centerY = Mathf.RoundToInt(uv.y * texHeight);

        int pixelRadius = Mathf.RoundToInt(radius * Mathf.Min(texWidth, texHeight));

        for (int x = -pixelRadius; x <= pixelRadius; x++)
        {
            for (int y = -pixelRadius; y <= pixelRadius; y++)
            {
                int px = centerX + x;
                int py = centerY + y;

                if (px < 0 || py < 0 || px >= texWidth || py >= texHeight)
                    continue;

                float dist = Mathf.Sqrt(x * x + y * y);
                if (dist > pixelRadius) continue;

                float strength = 1f - dist / pixelRadius;

                int index = py * texWidth + px;

                Color c = pixels[index];
                c.a -= strength * 0.25f;
                c.a = Mathf.Clamp01(c.a);

                pixels[index] = c;
            }
        }

        runtimeTexture.SetPixels(pixels);
        runtimeTexture.Apply();
    }

    public bool HasInk(Vector2 uv)
    {
        uv.x = Mathf.Clamp01(uv.x);
        uv.y = Mathf.Clamp01(uv.y);

        int x = Mathf.RoundToInt(uv.x * texWidth);
        int y = Mathf.RoundToInt(uv.y * texHeight);

        int index = y * texWidth + x;

        return pixels[index].a > 0.1f;
    }
}