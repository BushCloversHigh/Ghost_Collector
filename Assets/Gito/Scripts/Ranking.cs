using System.Collections.Generic;
using UnityEngine;
using NCMB;
using UnityEngine.UI;

// ランキングクラス
public class Ranking : UIManager
{
    // ランカー
    [SerializeField] private GameObject rankerPrefab;

    private void Start ()
    {
        // キーの設定
        NCMBSettings.ApplicationKey = MyStrings.NCMB_APPKEY;
        NCMBSettings.ClientKey = MyStrings.NCMB_CLIENTKEY;
    }

    // 初期化
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

    // ランキングをサーバーから取得
    public void GetRanking ()
    {
        Init ();
        NCMBQuery<NCMBObject> query = new NCMBQuery<NCMBObject> ("Ranking");
        query.OrderByDescending ("Score");
        query.Limit = 30;
        query.FindAsync ((List<NCMBObject> objList, NCMBException e) =>
        {
            if (e != null)
            {
                ShowToast ("エラーが発生しました。");
            }
            else
            {
                // ランカーを適用
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

    // アップロードスコア
    public void UploadScore ()
    {
        // スコアがないとアップロードできない
        if (DataManager.GetBestScore () == 0)
        {
            ShowToast ("スコアがありません。");
            return;
        }
        // 名前が空白だとアップロードできない
        string upName = GameObject.Find ("UIs/Ranking/Panel/InputField").GetComponent<InputField> ().text;
        if (string.IsNullOrEmpty (upName))
        {
            ShowToast ("名前を入力してください。");
            return;
        }
        // アップロード
        NCMBQuery<NCMBObject> query = new NCMBQuery<NCMBObject> ("Ranking");
        query.WhereEqualTo ("Name", upName);
        query.FindAsync ((List<NCMBObject> objList, NCMBException e) =>
        {
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
                    // サーバーの値より良ければ更新
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
