using System.Collections.Generic;
using UnityEngine;
using NCMB;
using UnityEngine.UI;

public class Ranking : UIManager
{

    [SerializeField] private GameObject rankerPrefab;

    private void Start ()
    {
        NCMBSettings.ApplicationKey = MyStrings.NCMB_APPKEY;
        NCMBSettings.ClientKey = MyStrings.NCMB_CLIENTKEY;
    }

    public void Init ()
    {
        GameObject rankingUI = GameObject.Find ("UIs/Ranking/Panel");
        rankingUI.transform.Find ("BestScore").GetComponent<Text> ().text = "ベストスコア : " + DataManager.GetBestScore ();
        Transform rankerParent = rankingUI.transform.Find ("LeaderBoard/Viewport/Content");
        for (int i = 0 ; i < rankerParent.childCount ; i++)
        {
            Destroy (rankerParent.GetChild (i).gameObject);
        }
    }

    public void GetRanking ()
    {
        Init ();
        // データストアの「HighScore」クラスから検索
        NCMBQuery<NCMBObject> query = new NCMBQuery<NCMBObject> ("Ranking");
        query.OrderByDescending ("Score");
        query.Limit = 30;
        query.FindAsync ((List<NCMBObject> objList, NCMBException e) =>
        {
            if (e != null)
            {
                //検索失敗時の処理
                ShowToast ("エラーが発生しました。");
            }
            else
            {
                //検索成功時の処理
                Transform rankerParent = GameObject.Find ("UIs/Ranking/Panel/LeaderBoard/Viewport/Content").transform;
                int r = 0;
                foreach (NCMBObject obj in objList)
                {
                    r++;
                    int s = System.Convert.ToInt32 (obj["Score"]);
                    string n = System.Convert.ToString (obj["Name"]);
                    GameObject ranker = Instantiate (rankerPrefab, rankerParent);
                    ranker.transform.GetChild (0).GetComponent<Text> ().text = r.ToString ();
                    ranker.transform.GetChild (1).GetComponent<Text> ().text = n;
                    ranker.transform.GetChild (2).GetComponent<Text> ().text = s.ToString ();
                }
            }
        });
    }

    public void UploadScore ()
    {
        if (DataManager.GetBestScore () == 0)
        {
            ShowToast ("スコアがありません。");
            return;
        }
        string upName = GameObject.Find ("UIs/Ranking/Panel/InputField").GetComponent<InputField> ().text;
        if (string.IsNullOrEmpty (upName))
        {
            ShowToast ("名前を入力してください。");
            return;
        }
        // データストアの「HighScore」クラスから、Nameをキーにして検索
        NCMBQuery<NCMBObject> query = new NCMBQuery<NCMBObject> ("Ranking");
        query.WhereEqualTo ("Name", upName);
        query.FindAsync ((List<NCMBObject> objList, NCMBException e) =>
        {
            //検索成功したら
            if (e == null)
            {
                //未登録
                if (objList.Count == 0)
                {
                    NCMBObject obj = new NCMBObject ("Ranking");
                    obj["Name"] = upName;
                    obj["Score"] = DataManager.GetBestScore ();
                    obj.SaveAsync ((NCMBException ee) =>
                    {
                        if (ee == null)
                        {
                            GetRanking ();
                        }
                        else
                        {
                            ShowToast ("エラーが発生しました。");
                        }
                    });
                }
                else
                {
                    float cloudScore = (float)System.Convert.ToDouble (objList[0]["Score"]);
                    if (DataManager.GetBestScore () > cloudScore)
                    {
                        objList[0]["Score"] = DataManager.GetBestScore ();
                        objList[0].SaveAsync ((NCMBException ee) =>
                        {
                            if (ee == null)
                            {
                                GetRanking ();
                            }
                            else
                            {
                                ShowToast ("エラーが発生しました。");
                            }
                        });
                    }
                    else
                    {
                        GetRanking ();
                    }
                }
            }
            else
            {
                ShowToast ("エラーが発生しました。");
            }
        });
    }
}
