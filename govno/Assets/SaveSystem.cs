using UnityEngine;

public static class SaveSystem
{
    public static void SaveCheckpoint(int id, Vector3 pos)
    {
        PlayerPrefs.SetInt("checkpoint_id", id);
        PlayerPrefs.SetFloat("pos_x", pos.x);
        PlayerPrefs.SetFloat("pos_y", pos.y);
        PlayerPrefs.SetFloat("pos_z", pos.z);
        PlayerPrefs.Save();
    }

    public static bool HasCheckpoint()
    {
        return PlayerPrefs.HasKey("checkpoint_id");
    }

    public static Vector3 LoadPosition()
    {
        float x = PlayerPrefs.GetFloat("pos_x");
        float y = PlayerPrefs.GetFloat("pos_y");
        float z = PlayerPrefs.GetFloat("pos_z");
        return new Vector3(x, y, z);
    }
}
