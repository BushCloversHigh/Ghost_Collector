using UnityEngine;
using UnityEngine.AI;

public class GhostController : MonoBehaviour, IUpdate
{

    public enum State
    {
        NORMAL,
        FOUND,
        ESCAPE,
        DEATH
    }

    private State state = State.NORMAL;
    private NavMeshAgent agent;
    private Animator animator;
    [SerializeField] private AudioClip death;

    [SerializeField] private GameObject ghostAttack;
    [SerializeField] private Transform ballSpawn;

    [SerializeField] private bool moveAble = true;

    [SerializeField] private float normalSpeed = 1f;
    [SerializeField] private float escapeSpeed = 2f;
    public int stamina = 10;
    [SerializeField] private float sight = 3f;
    [SerializeField] private bool attack = false;
    [SerializeField] private bool warpAble = false;
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
        if(GameManager.progress == Progress.FINISH)
        {
            GetComponent<AudioSource> ().volume = 0;
            return;
        }

        if (state == State.DEATH)
        {
            return;
        }

        if (stamina <= 0)
        {
            state = State.DEATH;
            AudioSource audioSource = GetComponent<AudioSource> ();
            audioSource.Stop ();
            audioSource.PlayOneShot (death);
            audioSource.volume = 1f;
            player.GetComponent<PlayerController> ().AbsorbGhost (gameObject);
            GameManager.ScoreCount (point);
            UpdateManager.RemoveUpdateList (this);
            GetComponent<GhostController>().enabled = false;
            return;
        }

        if (!moveAble) return;


        if (warpAble)
        {
            RandomWarp ();
        }

        switch (state)
        {
        case State.NORMAL:
            agent.speed = normalSpeed;
            RandomPatrol ();
            FindPlayer ();
            break;
        case State.FOUND:
            agent.speed = 0f;
            break;
        case State.ESCAPE:
            agent.speed = escapeSpeed;
            Escaping ();
            break;
        }
        animator.SetFloat ("Speed", agent.velocity.magnitude);
    }

    public void WarpRandomPos ()
    {
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent> ();
        }
        while (!agent.Warp (RandomPos ())) { }
    }

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

    private float rpt = 0f, next_rpt = 0f;
    private void RandomPatrol ()
    {
        rpt += Time.deltaTime;
        if (rpt > next_rpt)
        {
            while (!agent.SetDestination (RandomPos ())) { }
            next_rpt = Random.Range (3f, 10f);
            rpt = 0;
        }
    }

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

    private void FoundPlayer ()
    {
        animator.SetTrigger (attack ? "Attack" : "Surprise");
        state = State.FOUND;

        transform.LookAt (player.transform.position);
    }

    private void Escape ()
    {
        state = State.ESCAPE;
    }

    public void ForceEscape ()
    {
        animator.Play ("Escape");
        Escape ();
    }

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
