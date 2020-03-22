using UnityEngine;

// データを管理しているクラス
public class DataManager : MonoBehaviour
{
    private const string KEY_SCORE = "cwnceinwoinincdimw";

    // ベストスコアを更新
    public static void SaveBestScore (int score)
    {
        PlayerPrefs.SetInt (KEY_SCORE, score);
        PlayerPrefs.Save ();
    }

    // ベストスコアを取得
    public static int GetBestScore ()
    {
        return PlayerPrefs.GetInt (KEY_SCORE, 0);
    }
}
