using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour, IUpdate
{

    public enum State
    {
        NORMAL,
        VACUUM,
        VACUUMING,
        VACUUMED,
        DAMANE
    }

    private State state = State.NORMAL;

    [SerializeField] private float speed;
    public float mouse_sense;

    [SerializeField] private Transform cam;

    private CharacterController controller;

    private Vector3 rot;

    private AudioSource playerAudio; 
    [SerializeField] private AudioSource vacuumAudio;
    [SerializeField] private AudioClip footStep;
    [SerializeField] private AudioClip vacuum1, vacuum2;

    private Animator animator;

    [SerializeField] private Transform vacuum;
    [SerializeField] private Transform vacuumRange;
    [SerializeField] private ParticleSystem effect1, effect2;

    private List<GhostController> vacuumGhosts;

    private void Awake ()
    {
        UpdateManager.AddUpdateList (this);
    }

    private void Start ()
    {
        controller = GetComponent<CharacterController> ();
        playerAudio = GetComponent<AudioSource> ();
        animator = GetComponent<Animator> ();

        rot = transform.eulerAngles;
    }

    public void UpdateMe ()
    {
        if (GameManager.progress == Progress.FINISH)
        {
            playerAudio.volume = 0;
            vacuumAudio.volume = 0;
        }

        if (GameManager.progress != Progress.PLAYING)
        {
            return;
        }

        switch (state)
        {
        case State.NORMAL:
            Move ();
            Vacuum ();
            break;
        case State.VACUUMING:
            VacuumingGhost ();
            break;
        }

        MouseLook ();

        animator.SetFloat ("Speed", (controller.velocity.magnitude / speed) * (state == State.NORMAL ? 1f : 0f));
    }

    float foot = 0;
    bool b = false;
    private void Move ()
    {
        float h = Input.GetAxis ("Horizontal");
        float v = Input.GetAxis ("Vertical");
        float s = speed * (1 + Input.GetAxis ("Dash") * 0.8f);
        Vector3 dir = Vector3.Normalize (transform.forward * v + transform.right * h);
        controller.SimpleMove (dir * s);
    }

    private void FootStepSound ()
    {
        playerAudio.PlayOneShot (footStep);
    }

    private void MouseLook ()
    {
        if (Time.timeScale < 0.1f)
        {
            return;
        }
        float m = mouse_sense * 1f;
        float mX = Input.GetAxis ("Mouse X") * m * Time.deltaTime;
        float mY = Input.GetAxis ("Mouse Y") * m * Time.deltaTime;
        rot = new Vector3 (Mathf.Clamp (rot.x - mY, -80, 80), rot.y + mX, 0f);
        transform.eulerAngles = new Vector3 (0, rot.y, 0);
        cam.localEulerAngles = new Vector3 (rot.x, 0, 0);
    }

    private void Vacuum ()
    {
        if (Input.GetButtonDown ("Fire"))
        {
            state = State.VACUUM;
            Collider[] vacuumHit = Physics.OverlapBox (vacuumRange.transform.position, vacuumRange.transform.localScale * 0.5f);
            bool there = false;
            for (int i = 0 ; i < vacuumHit.Length ; i++)
            {
                if (vacuumHit[i].CompareTag ("Ghost"))
                {
                    there = true;
                    break;
                }
            }
            if (!there)
            {
                vacuumAudio.clip = vacuum1;
                vacuumAudio.Play ();
                vacuumAudio.volume = 0.5f;
                vacuumAudio.DOFade (0f, 1f);
                effect1.Play ();
                ChangeState (State.NORMAL, 1f);
                vacuum.DOPunchPosition (Vector3.back * 0.2f, 0.3f, 1);
            }
            else
            {
                vacuumAudio.clip = vacuum2;
                vacuumAudio.Play ();
                vacuumAudio.volume = 0f;
                vacuumAudio.DOFade (0.5f, 0.5f);
                ChangeState (State.VACUUMING, 0);
                effect2.Play ();
            }
        }
    }

    private float stop = 0;
    private void VacuumingGhost ()
    {
        Collider[] vacuumHit = Physics.OverlapBox (vacuumRange.transform.position, vacuumRange.transform.localScale * 0.5f);
        List<GhostController> ghosts = new List<GhostController> ();
        for (int i = 0 ; i < vacuumHit.Length ; i++)
        {
            if (vacuumHit[i].CompareTag ("Ghost"))
            {
                ghosts.Add (vacuumHit[i].GetComponent<GhostController> ());
                ghosts[ghosts.Count - 1].ForceEscape ();
            }
        }
        if (ghosts.Count == 0)
        {
            StopVacuum ();
        }
        else
        {
            stop += Time.deltaTime;
            if (stop > 1.5f)
            {
                StopVacuum ();
                stop = 0;
                return;
            }
            if (Input.GetButtonDown ("Fire"))
            {
                stop = 0f;
                vacuum.DOPunchPosition (Vector3.back * 0.2f, 0.1f, 1);
                effect1.Play ();
                for (int i = 0 ; i < ghosts.Count ; i++)
                {
                    if (ghosts[i] != null)
                    {
                        ghosts[i].vacuum += 0.5f;
                        ghosts[i].stamina--;
                    }
                }
            }
        }
    }

    private void StopVacuum ()
    {
        ChangeState (State.VACUUMED, 0);
        ChangeState (State.NORMAL, 1f);
        effect2.Stop ();
        vacuumAudio.DOFade (0f, 0.5f);
    }

    public void AbsorbGhost (GameObject ghost)
    {
        ghost.transform.DOMove (vacuum.GetChild (0).position, 2f).SetEase (Ease.OutExpo);
        ghost.transform.DOScale (Vector3.zero, 2f).SetEase (Ease.OutExpo);
        Destroy (ghost, 3f);
    }

    private void ChangeState (State to, float delay)
    {
        StartCoroutine (ChangeStateCor (to, delay));
    }

    private IEnumerator ChangeStateCor (State to, float delay)
    {
        yield return new WaitForSeconds (delay);
        state = to;
    }

    public void Damage ()
    {
        state = State.DAMANE;
        ChangeState (State.NORMAL, 2f);
    }
}
