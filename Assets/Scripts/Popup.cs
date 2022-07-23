using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Popup : MonoBehaviour
{
    public GameObject popupObject;
    public TMP_Text titleTextMesh;
    [Space]
    public Controls controls;

    private Action yesCallback;
    private Action noCallback;

    public void Open(string title, Action onYes, Action onNo)
    {
        popupObject.SetActive(true);
        controls.setInputMode(Controls.InputModes.Popup);
        titleTextMesh.text = "<style=c5>" + title + "</style>";

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
