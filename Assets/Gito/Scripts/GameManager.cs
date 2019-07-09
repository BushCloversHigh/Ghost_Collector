using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public enum Progress
{
    TITLE,
    READY,
    PLAYING,
    PAUSE,
    FINISH
}

public class GameManager : UIManager, IUpdate
{
    public static Progress progress = Progress.READY;

    private static int score;

    [SerializeField] private Text timerText, scoreText;

    private int h = 23, m = 45;
    private float t = 0;

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

    private void UpdateText ()
    {
        timerText.text = string.Format ("{0:d} : {1:d2}", h, m);
        scoreText.text = "Score : " + score;
    }

    private void GameStart ()
    {
        progress = Progress.PLAYING;
        PlaySoundEffect (bell);
        RectTransform timerTransform = timerText.GetComponent<RectTransform> ();
        timerTransform.DOMove (new Vector3 (Screen.width - 150f, Screen.height - 50f, 0), 0.5f);
        GetComponent<GhostSpawner> ().FirstSpawn ();
    }

    private void FinishGame ()
    {
        progress = Progress.FINISH;
        CursorEnable (true);
        StartCoroutine (FinishCor ());
    }

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

    public static void ScoreCount (int up)
    {
        score += up;
    }

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

    public void PauseClose ()
    {
        progress = pprogress;
        Time.timeScale = 1;
        CursorEnable (false);
        GameObject pause = GameObject.Find ("UIs").transform.Find ("Pause").gameObject;
        Slider slider = pause.transform.Find ("Sensi/Slider").GetComponent<Slider> ();
        sensi = slider.value;
        GameObject.FindWithTag ("Player").GetComponent<PlayerController> ().mouse_sense = sensi;
        pause.SetActive (false);
    }

    public void GoTitle ()
    {
        LoadScene ("Title");
    }

}
