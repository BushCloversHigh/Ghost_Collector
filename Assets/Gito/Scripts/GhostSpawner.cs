using UnityEngine;

public enum GhostColor
{
    WHITE,
    BLUE,
    GREEN,
    RED,
    PINK,
    YELLOW
}

public class GhostSpawner : MonoBehaviour, IUpdate
{
    [SerializeField] private GameObject[] spawnGhosts = new GameObject[6];

    [SerializeField] private int ghostNum = 20;

    private void Awake ()
    {
        UpdateManager.AddUpdateList (this);
    }

    public void FirstSpawn ()
    {
        for (int i = 0 ; i < ghostNum - 1 ; i++)
        {
            RandomSpawn ();
        }
        Spawn (GhostColor.YELLOW);
    }

    private float t = 0;
    public void UpdateMe ()
    {
        if (GameManager.progress == Progress.TITLE) return;
        t += Time.deltaTime;
        if (t > 3.5f)
        {
            t = 0;
            RandomSpawn ();
        }
    }

    private void RandomSpawn ()
    {
        int rand = Random.Range (0, 100);
        GameObject spawn;
        if (rand > 95)
        {
            spawn = spawnGhosts[(int)GhostColor.YELLOW];
        }
        else
        {
            spawn = spawnGhosts[Random.Range (0, 5)];
        }
        GameObject goGhost = Instantiate (spawn);
        goGhost.GetComponent<GhostController> ().WarpRandomPos ();
    }

    private void Spawn (GhostColor ghostColor)
    {
        GameObject goGhost = Instantiate (spawnGhosts[(int)ghostColor]);
        goGhost.GetComponent<GhostController> ().WarpRandomPos ();
    }
}
