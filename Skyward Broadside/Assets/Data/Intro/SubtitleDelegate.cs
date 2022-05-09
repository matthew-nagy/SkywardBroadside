using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SubtitleDelegate : MonoBehaviour
{
    public static SubtitleDelegate obj;
    public Text text1;
    public Text text2;

    private void Start()
    {
        obj = this;
    }

    public void SetText(string newText)
    {
        text1.text = newText;
        text2.text = newText;
    }

    public void EndCutscene()
    {
        Destroy(gameObject);
    }
}
