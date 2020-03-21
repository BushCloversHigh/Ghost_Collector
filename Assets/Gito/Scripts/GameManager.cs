using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

// 進行度
public enum Progress
{
    TITLE,
    READY,
    PLAYING,
    PAUSE,
    FINISH
}

// ゲームの進行管理
public class GameManager : UIManager, IUpdate
{
    // 進行度
    public static Progress progress = Progress.READY;
    // スコア
    private static int score;
    // テキスト
    [SerializeField] private Text timerText, scoreText;
    // 時間用の変数
    private int h = 23, m = 45;
    private float t = 0;
    // ベルとアラーム
    [SerializeField] private AudioClip bell, alerm;

    private static float sensi = 50f;

    private void Awake ()
    {
        UpdateManager.AddUpdateList (this);
        score = 0;
        progress = Progress.READY;
        Time.timeScale = 1;
    }

    private void Start ()
    {
        CursorEnable (false);
        BlackFadeIn (1f);
        GameObject.FindWithTag ("Player").GetComponent<PlayerController> ().mouse_sense = sensi;
    }

    public void UpdateMe ()
    {
        if (progress == Progress.FINISH) return;

        TimeCount ();
        UpdateText ();
        Pause ();
    }

    // 時間経過
    private void TimeCount ()
    {
        t += Time.deltaTime;
        if (t >= 0.2f)
        {
            t = 0;
            m++;
            if (m == 60)
            {
                m = 0;
                h++;
            }
            if (progress == Progress.READY)
            {
                if (h == 24)
                {
                    h = 0;
                    GameStart ();
                }
            }
            else if (progress == Progress.PLAYING)
            {
                if (h == 6)
                {
                    FinishGame ();
                }
            }
        }
    }

    // テキストを更新する
    private void UpdateText ()
    {
        timerText.text = string.Format ("{0:d} : {1:d2}", h, m);
        scoreText.text = "Score : " + score;
    }

    // ゲームスタート
    private void GameStart ()
    {
        progress = Progress.PLAYING;
        PlaySoundEffect (bell);
        RectTransform timerTransform = timerText.GetComponent<RectTransform> ();
        timerTransform.DOMove (new Vector3 (Screen.width - 150f, Screen.height - 50f, 0), 0.5f);
        GetComponent<GhostSpawner> ().FirstSpawn ();
    }

    // ゲーム終了
    private void FinishGame ()
    {
        progress = Progress.FINISH;
        CursorEnable (true);
        StartCoroutine (FinishCor ());
    }

    // ゲーム終了時のフロー
    private IEnumerator FinishCor ()
    {
        BlackFadeOut (1f);
        BGMStop ();
        PlaySoundEffect (alerm);
        GameObject result = GameObject.Find ("UIs").transform.Find ("Result").gameObject;
        result.SetActive (true);
        Text resultText = result.transform.Find ("Text").GetComponent<Text> ();
        Fade (resultText, 1f, 1f);
        yield return new WaitForSeconds (3f);
        resultText.text = "Score : " + score;
        if(score > DataManager.GetBestScore ())
        {
            DataManager.SaveBestScore (score);
        }
        yield return new WaitForSeconds (2f);
        result.transform.Find ("GoTitle").gameObject.SetActive (true);
    }

    // スコアをアップ
    public static void ScoreCount (int up)
    {
        score += up;
    }

    // 一時停止
    Progress pprogress;
    private void Pause ()
    {
        if (Input.GetButtonDown ("Pause"))
        {
            if (progress != Progress.PAUSE)
            {
                PauseOpen ();
            }
            else
            {
                PauseClose ();
            }
        }
    }

    // 一時停止する
    private void PauseOpen ()
    {
        pprogress = progress;
        progress = Progress.PAUSE;
        Time.timeScale = 0;
        CursorEnable (true);
        GameObject pause = GameObject.Find ("UIs").transform.Find ("Pause").gameObject;
        Slider slider = pause.transform.Find ("Sensi/Slider").GetComponent<Slider> ();
        slider.value = sensi;
        pause.SetActive (true);
    }

    // 一時停止を閉じる
    public void PauseClose ()
    {
        progress = pprogress;
        Time.timeScale = 1;
        CursorEnable (false);
        GameObject pause = GameObject.Find ("UIs").transform.Find ("Pause").gameObject;
        // 視点移動の感度を設定
        Slider slider = pause.transform.Find ("Sensi/Slider").GetComponent<Slider> ();
        sensi = slider.value;
        GameObject.FindWithTag ("Player").GetComponent<PlayerController> ().mouse_sense = sensi;
        pause.SetActive (false);
    }

    // タイトルへ
    public void GoTitle ()
    {
        LoadScene ("Title");
    }

}
