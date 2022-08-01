using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Text : MonoBehaviour
{
    public string id;

    Localiation transl;
    TMP_Text text;

    private void Start()
    {
        transl = GameObject.Find("Translation").GetComponent<Localiation>();
        text = GetComponentInChildren<TMP_Text>();
    }

    private void OnEnable()
    {
        UpdateText();
    }

    void UpdateText()
    {
        if (text != null && transl != null)
            text.text = transl.GetText(id);
    }
}
