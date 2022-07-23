using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Popup : MonoBehaviour
{
    public GameObject popupObject;
    public TMP_Text titleTextMesh;
    public TMP_Text subtitleTextMesh;
    [Space]
    public Controls controls;

    private Action yesCallback;
    private Action noCallback;

    private void Start()
    {
        popupObject.SetActive(false);
    }

    public void Open(string title, Action onYes, Action onNo)
    {
        Open(title, string.Empty, onYes, onNo);
    }
    public void Open(string title, string subtitle, Action onYes, Action onNo)
    {
        popupObject.SetActive(true);
        controls.setInputMode(Controls.InputModes.Popup);
        titleTextMesh.text = "<style=c5>" + title + "</style>";
        subtitleTextMesh.text = subtitle;

        yesCallback = onYes;
        noCallback = onNo;
    }

    public void yes()
    {
        yesCallback?.Invoke();
        close();
    }

    public void no()
    {
        noCallback?.Invoke();
        close();
    }

    private void close()
    {
        yesCallback = null;
        noCallback = null;
        popupObject.SetActive(false);
    }
}
