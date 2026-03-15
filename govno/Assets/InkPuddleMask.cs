using UnityEngine;

public class InkPuddleMask : MonoBehaviour
{
    public int textureResolution = 512;

    Texture2D runtimeTexture;
    Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();

        runtimeTexture = new Texture2D(textureResolution, textureResolution, TextureFormat.RGBA32, false);
        runtimeTexture.wrapMode = TextureWrapMode.Clamp;

        Color[] pixels = new Color[textureResolution * textureResolution];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = new Color(0, 0, 0, 1);

        runtimeTexture.SetPixels(pixels);
        runtimeTexture.Apply();

        rend.material.mainTexture = runtimeTexture;
    }

    public void Erase(Vector2 uv, float radius)
    {
        int centerX = Mathf.RoundToInt(uv.x * textureResolution);
        int centerY = Mathf.RoundToInt(uv.y * textureResolution);

        int pixelRadius = Mathf.RoundToInt(radius * textureResolution);

        for (int x = -pixelRadius; x <= pixelRadius; x++)
        {
            for (int y = -pixelRadius; y <= pixelRadius; y++)
            {
                int px = centerX + x;
                int py = centerY + y;

                if (px < 0 || py < 0 || px >= textureResolution || py >= textureResolution)
                    continue;

                float dist = Mathf.Sqrt(x * x + y * y);
                if (dist > pixelRadius) continue;

                float strength = 1f - dist / pixelRadius;

                Color c = runtimeTexture.GetPixel(px, py);
                c.a -= strength * 0.25f;
                c.a = Mathf.Clamp01(c.a);

                runtimeTexture.SetPixel(px, py, c);
            }
        }

        runtimeTexture.Apply();
    }

    public bool HasInk(Vector2 uv)
    {
        int x = Mathf.RoundToInt(uv.x * textureResolution);
        int y = Mathf.RoundToInt(uv.y * textureResolution);

        Color c = runtimeTexture.GetPixel(x, y);
        return c.a > 0.1f;
    }
}