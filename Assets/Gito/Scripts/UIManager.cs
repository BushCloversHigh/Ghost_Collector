using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public void CursorEnable (bool enable)
    {
        if (enable)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void BlackFadeIn (float dulation)
    {
        GameObject black = GameObject.Find ("UIs").transform.Find ("Black").gameObject;
        gameObject.SetActive (true);
        Image img = black.transform.GetChild(0).GetComponent<Image> ();
        Color color = img.color;
        color.a = 1f;
        img.color = color;
        img.DOFade (0f, dulation);
        DelayActive (black, false, dulation);
    }

    public void BlackFadeOut (float dulation)
    {
        GameObject black = GameObject.Find ("UIs").transform.Find ("Black").gameObject;
        black.SetActive (true);
        Image img = black.transform.GetChild (0).GetComponent<Image> ();
        Color color = img.color;
        color.a = 0f;
        img.color = color;
        img.DOFade (1f, dulation);
    }

    public void Fade (Text text, float alpha, float dulation)
    {
        text.DOFade (alpha, dulation);
    }

    public void Fade (Image img, float alpha, float dulation)
    {
        img.DOFade (alpha, dulation);
    }

    public void DelayActive (GameObject obj, bool active, float delay)
    {
        StartCoroutine (DelayActiveCor (obj, active, delay));
    }

    private IEnumerator DelayActiveCor (GameObject obj, bool active, float delay)
    {
        yield return new WaitForSeconds (delay);
        obj.SetActive (active);
    }

    public void PlaySoundEffect (AudioClip se)
    {
        transform.GetChild (1).GetComponent<AudioSource> ().PlayOneShot (se);
    }

    public void BGMStop ()
    {
        transform.GetChild (0).GetComponent<AudioSource> ().Stop();
    }

    public void BGMVolumeFade(float to, float dulation)
    {
        transform.GetChild (0).GetComponent<AudioSource> ().DOFade(to, dulation);
    }

    public void LoadScene (string sceneName, float delay = 0)
    {
        StartCoroutine (LoadSceneCor (sceneName, delay));
    }

    private IEnumerator LoadSceneCor (string sceneName, float delay)
    {
        yield return new WaitForSecondsRealtime (delay);
        SceneManager.LoadScene (sceneName);
    }

    private float fadeSpeed = 0.3f;
    private bool isShowing = false;
    private IEnumerator cor;
    public void ShowToast (string message)
    {
        if (isShowing)
        {
            StopCoroutine (cor);
        }
        isShowing = true;
        GameObject toast = GameObject.Find ("UIs/System").transform.GetChild (0).gameObject;
        toast.SetActive (true);
        Image back = toast.GetComponent<Image> ();
        Text mess = back.transform.GetChild(0).GetComponent<Text> ();
        mess.text = message;
        back.DOFade (0f, 0f);
        mess.DOFade (0f, 0f);
        back.DOFade (0.7f, fadeSpeed);
        mess.DOFade (1f, fadeSpeed);
        cor = Close ();
        StartCoroutine (cor);
    }

    private IEnumerator Close ()
    {
        yield return new WaitForSeconds (3f);
        GameObject toast = GameObject.Find ("UIs/System").transform.GetChild (0).gameObject;
        Image back = toast.GetComponent<Image> ();
        back.DOFade (0f, fadeSpeed);
        Text mess = back.transform.GetChild (0).GetComponent<Text> ();
        mess.DOFade (0f, fadeSpeed);
        yield return new WaitForSeconds (fadeSpeed);
        toast.gameObject.SetActive (false);
        isShowing = false;
    }
}
