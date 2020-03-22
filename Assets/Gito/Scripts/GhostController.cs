using UnityEngine;
using UnityEngine.AI;

// ゴーストのコントローラー
public class GhostController : MonoBehaviour, IUpdate
{
    // 状態
    public enum State
    {
        NORMAL,
        FOUND,
        ESCAPE,
        DEATH
    }

    private State state = State.NORMAL;
    // エージェント
    private NavMeshAgent agent;
    // アニメーター
    private Animator animator;
    // 吸い込まれ時の効果音
    [SerializeField] private AudioClip death;
    // 攻撃のオブジェクト
    [SerializeField] private GameObject ghostAttack;
    [SerializeField] private Transform ballSpawn;
    // 動けるか
    [SerializeField] private bool moveAble = true;
    // 通常時の速度
    [SerializeField] private float normalSpeed = 1f;
    // 逃げるときの速度
    [SerializeField] private float escapeSpeed = 2f;
    // スタミナ
    public int stamina = 10;
    // 視野の距離
    [SerializeField] private float sight = 3f;
    // 攻撃できるか
    [SerializeField] private bool attack = false;
    // ワープできるか
    [SerializeField] private bool warpAble = false;
    // このゴーストを吸い込んだときの獲得ポイントは
    [SerializeField] private int point = 10;
    public float vacuum = 0;

    private Transform player;

    private void Awake ()
    {
        UpdateManager.AddUpdateList (this);

        agent = GetComponent<NavMeshAgent> ();
        agent.speed = normalSpeed;

        animator = GetComponent<Animator> ();

        player = GameObject.FindWithTag ("Player").transform;
    }

    public void UpdateMe ()
    {
        // ゲームが終了してたら、音量0
        if(GameManager.progress == Progress.FINISH)
        {
            GetComponent<AudioSource> ().volume = 0;
            return;
        }

        // 死んでたら何もできない
        if (state == State.DEATH)
        {
            return;
        }

        // スタミナがもうない　死ぬ
        if (stamina <= 0)
        {
            // 吸い込まれる〜
            state = State.DEATH;
            AudioSource audioSource = GetComponent<AudioSource> ();
            audioSource.Stop ();
            audioSource.PlayOneShot (death);
            audioSource.volume = 1f;
            player.GetComponent<PlayerController> ().AbsorbGhost (gameObject);
            tag = "Untagged";
            GameManager.ScoreCount (point);
            UpdateManager.RemoveUpdateList (this);
            GetComponent<GhostController>().enabled = false;
            return;
        }

        // 動けないなら、ここから何もできない
        if (!moveAble) return;

        // ワープできる個体なら、ワープする
        if (warpAble)
        {
            RandomWarp ();
        }

        switch (state)
        {
        case State.NORMAL: // 通常時　普通のスピード　巡回　プレイヤーを見つける
            agent.speed = normalSpeed;
            RandomPatrol ();
            FindPlayer ();
            break;
        case State.FOUND: // プレイヤーを見つけている
            agent.speed = 0f;
            break;
        case State.ESCAPE: // 逃げる
            agent.speed = escapeSpeed;
            Escaping ();
            break;
        }
        animator.SetFloat ("Speed", agent.velocity.magnitude);
    }

    // ランダムなポジションにワープ
    public void WarpRandomPos ()
    {
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent> ();
        }
        // ワープできるところがだったらワープ
        while (!agent.Warp (RandomPos ())) { }
    }

    // ワープするやつは3秒おきにワープ
    private float warpTime = 0f;
    private void RandomWarp ()
    {
        warpTime += Time.deltaTime;
        if(warpTime > 3f)
        {
            warpTime = 0;
            int r = Random.Range (0, 3);
            if(r == 0)
            {
                WarpRandomPos ();
            }
        }
    }

    // 巡回
    private float rpt = 0f, next_rpt = 0f;
    private void RandomPatrol ()
    {
        // ランダムなところをパトロール
        rpt += Time.deltaTime;
        if (rpt > next_rpt)
        {
            while (!agent.SetDestination (RandomPos ())) { }
            next_rpt = Random.Range (3f, 10f);
            rpt = 0;
        }
    }

    // プレイヤーを見つける
    private float angle = 0;
    private bool angleDir = true;
    private void FindPlayer ()
    {
        if (angle > 1f)
        {
            angleDir = true;
        }
        else if (angle < -1f)
        {
            angleDir = false;
        }
        angle += angleDir ? -0.05f : +0.05f;
        Ray ray = new Ray (transform.position + Vector3.up * 0.5f, transform.forward + transform.right * angle);
        RaycastHit hit;
        if (Physics.Raycast (ray, out hit, sight))
        {
            if (hit.collider.CompareTag ("Player"))
            {
                FoundPlayer ();
            }
        }
    }

    // プレイヤーを見つけた
    private void FoundPlayer ()
    {
        // 攻撃かびっくりする
        animator.SetTrigger (attack ? "Attack" : "Surprise");
        state = State.FOUND;

        transform.LookAt (player.transform.position);
    }

    // 逃げる
    private void Escape ()
    {
        state = State.ESCAPE;
    }

    public void ForceEscape ()
    {
        animator.Play ("Escape");
        Escape ();
    }

    // 逃げている最中
    private float cantSeeDuration = 0;
    private void Escaping ()
    {
        vacuum -= Time.deltaTime * 2f;
        if (vacuum <= 0.2f)
        {
            vacuum = 0.25f;
        }
        float speed = escapeSpeed - vacuum;
        if (speed <= 0f)
        {
            speed = 0.1f;
        }
        agent.velocity = -Vector3.Normalize (player.transform.position - transform.position) * speed;
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 dir = player.transform.position + Vector3.up * 0.5f - origin;
        Ray ray = new Ray (origin, dir);
        RaycastHit hit;
        if (Physics.Raycast (ray, out hit))
        {
            if (!hit.collider.CompareTag ("Player"))
            {
                cantSeeDuration += Time.deltaTime;
            }
            else
            {
                cantSeeDuration = 0;
            }
        }
        if (cantSeeDuration > 3f)
        {
            state = State.NORMAL;
            animator.SetTrigger ("Escaped");
            cantSeeDuration = 0;
            vacuum = 0;
        }
    }

    private Vector3 RandomPos ()
    {
        Vector3 r;
        r.x = Random.Range (-25f, 12f);
        r.y = 0f;
        r.z = Random.Range (-8f, 8f);
        return r;
    }

    private void AttackBall ()
    {
        GameObject attackBall = Instantiate (ghostAttack);
        attackBall.transform.position = ballSpawn.position;
        attackBall.transform.forward = transform.forward;
    }

}
