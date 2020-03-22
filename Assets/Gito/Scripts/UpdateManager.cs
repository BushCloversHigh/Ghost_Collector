using System.Collections.Generic;
using UnityEngine;

// インターフェース
public interface IUpdate
{
    void UpdateMe ();
}

// インターフェース
public interface IFixedUpdate
{
    void FixedUpdateMe ();
}

// Update関数をここから呼び出す (お化けがいっぱい出るから)
public class UpdateManager : MonoBehaviour
{
    // Updateをするリスト
    private static List<IUpdate> updates = new List<IUpdate> ();
    private static List<IFixedUpdate> fixedUpdates = new List<IFixedUpdate> ();

    // リストに追加
    public static void AddUpdateList (IUpdate update)
    {
        updates.Add (update);
    }

    public static void AddFixedUpdateList (IFixedUpdate fixedUpdate)
    {
        fixedUpdates.Add (fixedUpdate);
    }

    // リストから削除
    public static void RemoveUpdateList (IUpdate update)
    {
        updates.Remove (update);
    }

    public static void RemoveFixedUpdateList (IFixedUpdate fixedUpdate)
    {
        fixedUpdates.Remove (fixedUpdate);
    }

    // シーンが破棄された時、リストをクリア
    private void OnDestroy ()
    {
        updates.Clear ();
        fixedUpdates.Clear ();
    }

    // リストにあるアップデートをここから呼び出す
    private void Update ()
    {
        for (int i = 0 ; i < updates.Count ; i++)
        {
            updates[i].UpdateMe ();
        }
    }

    private void FixedUpdate ()
    {
        for(int i = 0 ;i < fixedUpdates.Count ; i++)
        {
            fixedUpdates[i].FixedUpdateMe ();
        }
    }

}
