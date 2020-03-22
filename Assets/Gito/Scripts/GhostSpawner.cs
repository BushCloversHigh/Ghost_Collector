using UnityEngine;

// ゴーストの種類
public enum GhostColor
{
    WHITE,
    BLUE,
    GREEN,
    RED,
    PINK,
    YELLOW
}

// ゴースト生成クラス
public class GhostSpawner : MonoBehaviour, IUpdate
{
    // 生成するゴーストのプレハブ
    [SerializeField] private GameObject[] spawnGhosts = new GameObject[6];
    // ゴーストの数
    [SerializeField] private int ghostNum = 20;

    // Updateリストに追加
    private void Awake ()
    {
        UpdateManager.AddUpdateList (this);
    }

    // 最初の生成
    public void FirstSpawn ()
    {
        // ランダムで生成
        for (int i = 0 ; i < ghostNum - 1 ; i++)
        {
            RandomSpawn ();
        }
        // 最後の一体は黄色
        Spawn (GhostColor.YELLOW);
    }

    private float t = 0;
    public void UpdateMe ()
    {
        // タイトル画面の時は何もできない
        if (GameManager.progress == Progress.TITLE) return;

        // 時間経過でゴースト生成
        t += Time.deltaTime;
        if (t > 3.5f)
        {
            t = 0;
            RandomSpawn ();
        }
    }

    // ランダム生成
    private void RandomSpawn ()
    {
        // 0〜99でランダム
        int rand = Random.Range (0, 100);
        GameObject spawn;
        // 5％の確率で黄色が出る
        if (rand > 95)
        {
            spawn = spawnGhosts[(int)GhostColor.YELLOW];
        }
        else
        {
            spawn = spawnGhosts[Random.Range (0, 5)];
        }
        // 生成
        GameObject goGhost = Instantiate (spawn);
        // ランダムな位置にワープ
        goGhost.GetComponent<GhostController> ().WarpRandomPos ();
    }

    // 何色を生成するか指定できる
    private void Spawn (GhostColor ghostColor)
    {
        GameObject goGhost = Instantiate (spawnGhosts[(int)ghostColor]);
        goGhost.GetComponent<GhostController> ().WarpRandomPos ();
    }
}
