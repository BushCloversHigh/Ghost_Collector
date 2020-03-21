using UnityEngine;
using DG.Tweening;
using System.Collections;
// タイトル画面を管理
public class TitleManager : UIManager
{
    private Transform cam;

    [SerializeField] private AudioClip piano1, piano2;

    private void Awake ()
    {
        GameManager.progress = Progress.TITLE;
        cam = Camera.main.transform;
        Time.timeScale = 1;
    }

    private void Start ()
    {
        GetComponent<GhostSpawner> ().FirstSpawn ();
        StartCoroutine (CameraMove ());
    }

    // カメラをアニメーションさせる
    private IEnumerator CameraMove ()
    {
        int i = 0;
        Transform points = GameObject.Find ("CameraPoints").transform;
        while (true)
        {
            cam.transform.position = points.GetChild (i).position;
            i++;
            cam.DORotate (points.GetChild (i).eulerAngles, 10f);
            cam.DOMove (points.GetChild (i).position, 10f).SetEase (Ease.Linear);
            i++;
            if (i == 8)
            {
                i = 0;
            }
            yield return new WaitForSeconds (10f);
        }
    }

    // スタートボタンを押した
    public void StartPushed ()
    {
        PlaySoundEffect (piano2);
        BGMVolumeFade (0f, 1f);
        BlackFadeOut (1f);
        LoadScene ("Main", 1f);
    }

    // ランキングボタンを押した
    public void RankingPushed ()
    {
        PlaySoundEffect (piano1);
        GameObject.Find ("UIs").transform.Find ("Ranking").gameObject.SetActive (true);
        GetComponent<Ranking> ().Init ();
        GetComponent<Ranking> ().GetRanking ();

    }

    // ランキングを閉じた
    public void RankingBack ()
    {
        PlaySoundEffect (piano1);
        GameObject.Find ("UIs").transform.Find ("Ranking").gameObject.SetActive (false);
    }

}
