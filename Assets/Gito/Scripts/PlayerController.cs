using UnityEngine;

public class PlayerController : MonoBehaviour, IUpdate
{

    [SerializeField] private float speed;
    public float mouse_sense;

    [SerializeField] private Transform cam;

    private CharacterController controller;

    private Vector3 rot;

    private AudioSource audioSource;
    [SerializeField] private AudioClip footStep;

    private Animator animator;

    private void Awake ()
    {
        UpdateManager.AddUpdateList (this);
    }

    private void Start ()
    {
        controller = GetComponent<CharacterController> ();
        audioSource = GetComponent<AudioSource> ();
        animator = GetComponent<Animator> ();

        rot = transform.eulerAngles;
    }

    public void UpdateMe ()
    {
        Move ();
        MouseLook ();
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
        animator.SetFloat ("Speed", controller.velocity.magnitude / speed);
    }

    private void FootStepSound ()
    {
        audioSource.PlayOneShot (footStep);
    }

    private void MouseLook ()
    {
        if (Time.timeScale < 0.1f)
        {
            return;
        }
        float m = mouse_sense * 0.01f;
        float mX = Input.GetAxis ("Mouse X") * m
         + Input.GetAxis ("LeftRight") * m * 800f * Time.deltaTime;
        float mY = Input.GetAxis ("Mouse Y") * m
         + Input.GetAxis ("UpDown") * m * 800f * Time.deltaTime;
        rot = new Vector3 (Mathf.Clamp (rot.x - mY, -80, 80), rot.y + mX, 0f);
        transform.eulerAngles = new Vector3 (0, rot.y, 0);
        cam.localEulerAngles = new Vector3 (rot.x, 0, 0);
    }
}
