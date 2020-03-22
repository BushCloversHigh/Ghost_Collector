using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

// プレイヤーのコントローラー　インターフェース
public class PlayerController : MonoBehaviour, IUpdate
{
    // 状態
    public enum State
    {
        NORMAL,
        VACUUM,
        VACUUMING,
        VACUUMED,
        DAMANE
    }

    // 最初はノーマル状態
    private State state = State.NORMAL;

    // 移動速度
    [SerializeField] private float speed;
    // マウス感度
    public float mouse_sense;

    // カメラ
    [SerializeField] private Transform cam;

    // キャラコン
    private CharacterController controller;

    // カメラ移動の角度を入れておく変数
    private Vector3 rot;

    // 音関係 
    private AudioSource playerAudio; 
    [SerializeField] private AudioSource vacuumAudio;
    [SerializeField] private AudioClip footStep;
    [SerializeField] private AudioClip vacuum1, vacuum2;

    // アニメーター
    private Animator animator;

    // 掃除機
    [SerializeField] private Transform vacuum;
    // 吸い込み判定
    [SerializeField] private Transform vacuumRange;
    // エフェクト　吸い込みミス　吸い込んでるー
    [SerializeField] private ParticleSystem effect1, effect2;

    private List<GhostController> vacuumGhosts;

    // アップデートのリストに追加
    private void Awake ()
    {
        UpdateManager.AddUpdateList (this);
    }

    private void Start ()
    {
        // コンポーネントを取得
        controller = GetComponent<CharacterController> ();
        playerAudio = GetComponent<AudioSource> ();
        animator = GetComponent<Animator> ();
        // 角度を代入
        rot = transform.eulerAngles;
    }

    // 呼び出してもらうUpdate関数
    public void UpdateMe ()
    {
        // ゲームの進行がフィニッシュの時、プレイヤーと掃除機の音を消す
        if (GameManager.progress == Progress.FINISH)
        {
            playerAudio.volume = 0;
            vacuumAudio.volume = 0;
        }

        // ゲームの進行がプレイ中じゃない時、何もしない
        if (GameManager.progress != Progress.PLAYING)
        {
            return;
        }

        switch (state)
        {
        case State.NORMAL: // プレイヤーの状態がノーマルの時、移動と吸い込みができる
            Move ();
            Vacuum ();
            break;
        case State.VACUUMING: // 吸い込み中の時は、ゴーストを吸い込み
            VacuumingGhost ();
            break;
        }

        // 視点移動はいつでもできる
        MouseLook ();
        // アニメーションのSpeedを適用
        animator.SetFloat ("Speed", (controller.velocity.magnitude / speed) * (state == State.NORMAL ? 1f : 0f));

        //Debug.Log (state);
    }

    // 移動
    private void Move ()
    {
        // wasdの丹生ろく
        float h = Input.GetAxis ("Horizontal");
        float v = Input.GetAxis ("Vertical");
        // スピードを計算
        float s = speed * (1 + Input.GetAxis ("Dash") * 0.8f);
        // 進行方向を単位ベクトル化
        Vector3 dir = Vector3.Normalize (transform.forward * v + transform.right * h);
        // キャラコンで移動
        controller.SimpleMove (dir * s);
    }

    // 足音を鳴らすイベント
    private void FootStepSound ()
    {
        playerAudio.PlayOneShot (footStep);
    }

    // 視点移動
    private void MouseLook ()
    {
        // タイムスケールがない時、何もできない
        if (Time.timeScale < 0.1f)
        {
            return;
        }
        // マウスの移動量と感度
        float m = mouse_sense * 1f;
        float mX = Input.GetAxis ("Mouse X") * m * Time.deltaTime;
        float mY = Input.GetAxis ("Mouse Y") * m * Time.deltaTime;
        // x軸は角度制限付き、y軸はそのまま回転
        rot = new Vector3 (Mathf.Clamp (rot.x - mY, -80, 80), rot.y + mX, 0f);
        // y軸はこれに
        transform.eulerAngles = new Vector3 (0, rot.y, 0);
        // x軸はカメラに
        cam.localEulerAngles = new Vector3 (rot.x, 0, 0);
    }

    // 吸い込む
    private void Vacuum ()
    {
        // 攻撃ボタンを押した時
        if (Input.GetButtonDown ("Fire"))
        {
            // 状態を変える
            state = State.VACUUM;
            // 吸い込み範囲内にゴーストがいるか
            Collider[] vacuumHit = Physics.OverlapBox (vacuumRange.transform.position, vacuumRange.transform.localScale * 0.5f);
            // ゴーストを探す
            bool there = false;
            for (int i = 0 ; i < vacuumHit.Length ; i++)
            {
                if (vacuumHit[i].CompareTag ("Ghost"))
                {
                    there = true;
                    break;
                }
            }
            // いない
            if (!there)
            {
                // 外れ用のエフェクト
                vacuumAudio.clip = vacuum1;
                vacuumAudio.Play ();
                vacuumAudio.volume = 0.5f;
                vacuumAudio.DOFade (0f, 1f);
                effect1.Play ();
                StartCoroutine (VacuumReticle ());
                ChangeState (State.NORMAL, 1f);
                vacuum.DOPunchPosition (Vector3.back * 0.2f, 0.3f, 1);
            }
            else // いる！
            {
                // 吸い込む用のエフェクト
                vacuumAudio.clip = vacuum2;
                vacuumAudio.Play ();
                vacuumAudio.volume = 0f;
                vacuumAudio.DOFade (0.5f, 0.5f);
                ChangeState (State.VACUUMING, 0);
                StartCoroutine (VacuumingReticle ());
                effect2.Play ();
            }
        }
    }

    // ゴーストを実際に吸い込む 連打
    private float stop = 0;
    private void VacuumingGhost ()
    {
        // 吸い込み範囲内にゴーストがいるか
        Collider[] vacuumHit = Physics.OverlapBox (vacuumRange.transform.position, vacuumRange.transform.localScale * 0.5f);
        // いるだけ追加
        List<GhostController> ghosts = new List<GhostController> ();
        for (int i = 0 ; i < vacuumHit.Length ; i++)
        {
            if (vacuumHit[i].CompareTag ("Ghost"))
            {
                ghosts.Add (vacuumHit[i].GetComponent<GhostController> ());
                ghosts[ghosts.Count - 1].ForceEscape ();
            }
        }
        // いない時は掃除機ストップ
        if (ghosts.Count == 0)
        {
            StopVacuum ();
        }
        else
        {
            // クリック連打しないと、掃除機ストップ
            stop += Time.deltaTime;
            if (stop > 1.5f)
            {
                StopVacuum ();
                stop = 0;
                return;
            }
            // 連打している間、吸い込む
            if (Input.GetButtonDown ("Fire"))
            {
                stop = 0f;
                vacuum.DOPunchPosition (Vector3.back * 0.2f, 0.1f, 1);
                effect1.Play ();
                // ゴーストが吸い込まれて、体力が減っていく
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

    // 吸い込み時のレティクルのアニメーション
    private IEnumerator VacuumingReticle ()
    {
        RectTransform reticle = GameObject.Find ("UIs/Game/Reticle").GetComponent<RectTransform> ();
        reticle.DOSizeDelta (Vector2.one * 250f, 0.4f);
        yield return new WaitForSeconds (0.4f);
        float t = 0.2f;
        bool b = true;
        // 吸い込んでいる間、大小を繰り返す
        while(state == State.VACUUMING)
        {
            t += Time.deltaTime;

            if (t > (b ? 0.2f : 0.4f))
            {
                t = 0;
                b = !b;
                reticle.DOSizeDelta (Vector2.one * (b ? 250f : 200f), b ? 0.2f : 0.4f);
            }
            yield return null;
        }
        reticle.DOKill ();
        reticle.DOSizeDelta (Vector2.one * 10f, 0.5f);
        yield break;
    }

    // 吸い込もうとしたときのレティクルのアニメーション
    private IEnumerator VacuumReticle ()
    {
        // 一瞬大きくなる
        RectTransform reticle = GameObject.Find ("UIs/Game/Reticle").GetComponent<RectTransform> ();
        reticle.DOSizeDelta (Vector2.one * 30f, 0.3f);
        yield return new WaitForSeconds (0.3f);
        reticle.DOSizeDelta (Vector2.one * 10f, 0.7f);
        yield break;
    }

    // 掃除機を止める
    private void StopVacuum ()
    {
        ChangeState (State.VACUUMED, 0);
        ChangeState (State.NORMAL, 1f);
        effect2.Stop ();
        vacuumAudio.DOFade (0f, 0.5f);
    }

    // お化けの吸い込みに成功
    public void AbsorbGhost (GameObject ghost)
    {
        ghost.transform.DOMove (vacuum.GetChild (0).position, 2f).SetEase (Ease.OutExpo);
        ghost.transform.DOScale (Vector3.zero, 2f).SetEase (Ease.OutExpo);
        Destroy (ghost, 3f);
    }

    // 少し経ったら状態を変える
    private void ChangeState (State to, float delay)
    {
        StartCoroutine (ChangeStateCor (to, delay));
    }

    private IEnumerator ChangeStateCor (State to, float delay)
    {
        yield return new WaitForSeconds (delay);
        state = to;
    }

    // 攻撃を受けた　一瞬止まる
    public void Damage ()
    {
        state = State.DAMANE;
        ChangeState (State.NORMAL, 2f);
    }
}
