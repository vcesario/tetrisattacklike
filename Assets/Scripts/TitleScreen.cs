using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScreen : MonoBehaviour
{
    public GameObject screenObject;
    public Animator thisAnimator;
    public Controls controls;
    public GameManager gameManager;
    public Popup popup;

    public bool isAnimating
    { get; private set; }

    private void Start()
    {
        screenObject.SetActive(true);
        controls.setInputMode(Controls.InputModes.Title);
    }

    public void playGame()
    {
        thisAnimator.Play("InGame");
        gameManager.resetGrid();
        controls.setInputMode(Controls.InputModes.Game);
    }

    public void exitGame()
    {
        popup.Open("Exit",
            delegate { Debug.Log("Fechou o jogo!"); Application.Quit(); controls.setInputMode(Controls.InputModes.Title); },
            delegate { Debug.Log("Não fechou o jogo..."); controls.setInputMode(Controls.InputModes.Title); });
    }

    public void clearScores()
    {
        popup.Open("Clear scores",
            delegate { Debug.Log("Resetou scores!"); controls.setInputMode(Controls.InputModes.Title); },
            delegate { Debug.Log("Não resetou scores..."); controls.setInputMode(Controls.InputModes.Title); });
    }
}
