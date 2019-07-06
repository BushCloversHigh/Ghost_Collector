using System.Collections.Generic;
using UnityEngine;

public interface IUpdate
{
    void UpdateMe ();
}

public interface IFixedUpdate
{
    void FixedUpdateMe ();
}

public class UpdateManager : MonoBehaviour
{
    private static List<IUpdate> updates = new List<IUpdate> ();
    private static List<IFixedUpdate> fixedUpdates = new List<IFixedUpdate> ();

    public static void AddUpdateList (IUpdate update)
    {
        updates.Add (update);
    }

    public static void AddFixedUpdateList (IFixedUpdate fixedUpdate)
    {
        fixedUpdates.Add (fixedUpdate);
    }

    public static void RemoveUpdateList (IUpdate update)
    {
        updates.Remove (update);
    }

    public static void RemoveFixedUpdateList (IFixedUpdate fixedUpdate)
    {
        fixedUpdates.Remove (fixedUpdate);
    }

    private void OnDestroy ()
    {
        updates.Clear ();
        fixedUpdates.Clear ();
    }

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
