using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controls : MonoBehaviour
{
    public TitleScreen titleScreen;
    public GameManager gameManager;
    public AudioManager audioManager;
    public Popup popup;

    public enum InputModes { None = 0, Title = 1, Game = 2, Popup = 3, Credits = 4 }
    [SerializeField]
    private InputModes currentInputMode;

    private void Update()
    {
        switch (currentInputMode)
        {
            case InputModes.Title:
                input_Title();
                break;
            case InputModes.Game:
                input_Game();
                break;
            case InputModes.Popup:
                input_Popup();
                break;
            case InputModes.Credits:
                input_Credits();
                break;
            default:
                break;
        }
    }

    public void setInputMode(InputModes newMode)
    {
        currentInputMode = newMode;
        Debug.Log("INPUT MODE ATIVADO: " + newMode.ToString());
    }

    private void input_Title()
    {
        if (titleScreen.isAnimating)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            titleScreen.playGame();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            titleScreen.exitGame();
        }
        else if (Input.GetKeyDown(KeyCode.Z))
        {
            titleScreen.openCredits();
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            titleScreen.clearDataPrompt();
        }
        else if (Input.GetKeyDown(KeyCode.M))
        {
            audioManager.toggleMute();
        }
    }

    private void input_Game()
    {
        if (!gameManager.isUpdating)
            return;

        if (Input.GetKeyDown(KeyCode.W))
        {
            gameManager.moveSelectorUp();
            gameManager.updateSelectorGraphics();
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            gameManager.moveSelectorLeft();
            gameManager.updateSelectorGraphics();
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            gameManager.moveSelectorDown();
            gameManager.updateSelectorGraphics();
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            gameManager.moveSelectorRight();
            gameManager.updateSelectorGraphics();
        }
        //else if (Input.GetKeyDown(KeyCode.Return))
        //{
        //    gameManager.changeSelectorOrientation();
        //    gameManager.updateSelectorGraphics();
        //}
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            gameManager.swapBalls();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            gameManager.finishGame();
        }
        else if (Input.GetKeyDown(KeyCode.M))
        {
            audioManager.toggleMute();
        }
#if UNITY_EDITOR
        else if (Input.GetKeyDown(KeyCode.R))
        {
            gameManager.resetGrid();
        }
#endif
    }

    private void input_Popup()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            popup.yes();
        }
        else if (Input.GetKeyDown(KeyCode.N))
        {
            popup.no();
        }
    }

    private void input_Credits()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            titleScreen.closeCredits();
        }
    }
}
