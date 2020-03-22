using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ゴーストの攻撃
public class GhostBall : MonoBehaviour, IUpdate
{

    private void Start ()
    {
        UpdateManager.AddUpdateList (this);
    }

    private float t = 0;
    // 前に進む　5秒経ったら消える
    public void UpdateMe ()
    {
        transform.position += transform.forward * 1.5f * Time.deltaTime;
        t += Time.deltaTime;
        if (t > 5f)
        {
            UpdateManager.RemoveUpdateList (this);
            Destroy (gameObject);
        }
    }

    // プレイヤーにぶつかったら、ダメージを与える
    public void OnCollisionEnter (Collision collision)
    {
        if (collision.gameObject.CompareTag ("Player"))
        {
            collision.gameObject.GetComponent<PlayerController> ().Damage ();
            UpdateManager.RemoveUpdateList (this);
            Destroy (gameObject);
        }

    }
}
