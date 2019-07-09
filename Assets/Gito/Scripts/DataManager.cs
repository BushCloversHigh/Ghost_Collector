using UnityEngine;

public class DataManager : MonoBehaviour
{
    private const string KEY_SCORE = "cwnceinwoinincdimw";

    public static void SaveBestScore (int score)
    {
        PlayerPrefs.SetInt (KEY_SCORE, score);
        PlayerPrefs.Save ();
    }

    public static int GetBestScore ()
    {
        return PlayerPrefs.GetInt (KEY_SCORE, 0);
    }
}
