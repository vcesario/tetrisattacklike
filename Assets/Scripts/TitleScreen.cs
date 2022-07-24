using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TitleScreen : MonoBehaviour
{
    public GameObject screenObject;
    public Animator thisAnimator;
    public TMP_Text titleControlsTextMesh;
    public TMP_Text scoreTextMesh;
    public TMP_Text lastScoreTextMesh;
    public TMP_Text bestScoreTextMesh;

    [Space]
    public Controls controls;
    public GameManager gameManager;
    public Popup popup;

    public bool isAnimating
    { get; private set; }

    private bool firstOpen;

    private void Start()
    {
        screenObject.SetActive(true);
        gameManager.eventScoreChanged += onScoreChanged;
        firstOpen = true;

        open();
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

    public void clearScoresPrompt()
    {
        popup.Open("Clear scores",
            delegate { clearScores(); controls.setInputMode(Controls.InputModes.Title); },
            delegate { controls.setInputMode(Controls.InputModes.Title); });
    }
    private void clearScores()
    {
        PlayerPrefs.DeleteAll();
        lastScoreTextMesh.transform.rotation = Quaternion.Euler(90, 0, 0);
        bestScoreTextMesh.transform.rotation = Quaternion.Euler(90, 0, 0);
    }

    public void open()
    {
        StartCoroutine(_playOpenSequence());
    }
    private IEnumerator _playOpenSequence()
    {
        thisAnimator.Play("Title");
        controls.setInputMode(Controls.InputModes.Title);

        isAnimating = true;
        titleControlsTextMesh.gameObject.SetActive(false);

        if (firstOpen)
        {
            // caso seja a primeira vez que estou abrindo a tela no jogo:
            //  se tenho pontuacao salva, apenas mostro imediatamente
            //  se nao tenho pontuacao salva, escondo o texto
            if (PlayerPrefs.HasKey("lastScore"))
            {
                lastScoreTextMesh.transform.rotation = Quaternion.Euler(0, 0, 0);
                int lastScore = PlayerPrefs.GetInt("lastScore");
                lastScoreTextMesh.text = "<style=c4>LAST SCORE</style> " + lastScore.ToString("D4");

                if (PlayerPrefs.HasKey("bestScore"))
                {
                    bestScoreTextMesh.transform.rotation = Quaternion.Euler(0, 0, 0);
                    int bestScore = PlayerPrefs.GetInt("bestScore");
                    bestScoreTextMesh.text = "<style=c5>BEST SCORE</style> " + bestScore.ToString("D4");
                }
                else
                {
                    bestScoreTextMesh.transform.rotation = Quaternion.Euler(90, 0, 0);
                }
            }
            else
            {
                lastScoreTextMesh.transform.rotation = Quaternion.Euler(90, 0, 0);
                bestScoreTextMesh.transform.rotation = Quaternion.Euler(90, 0, 0);
            }
        }
        else
        {
            // caso seja a segunda vez em diante, significa que joguei
            if (PlayerPrefs.HasKey("lastScore"))
            {
                lastScoreTextMesh.transform.rotation = Quaternion.Euler(0, 0, 0);
                bestScoreTextMesh.transform.rotation = Quaternion.Euler(90, 0, 0);

                yield return new WaitForSeconds(.5f);

                int lastScore = PlayerPrefs.GetInt("lastScore", 0);

                bool easeComplete = false;
                LeanTween.rotate(lastScoreTextMesh.gameObject, new Vector3(90, 0, 0), 1f).setEaseInCirc().setOnComplete(delegate () { easeComplete = true; });
                while (!easeComplete)
                    yield return 0;
                lastScoreTextMesh.text = "<style=c4>LAST SCORE</style> " + lastScore.ToString("D4");

                easeComplete = false;
                LeanTween.rotate(lastScoreTextMesh.gameObject, new Vector3(0, 0, 0), .5f).setEaseOutCirc().setOnComplete(delegate () { easeComplete = true; });

                bestScoreTextMesh.transform.rotation = Quaternion.Euler(0, 0, 0);
                int lastBestScore = PlayerPrefs.GetInt("bestScore", 0);
                if (lastScore > lastBestScore)
                {
                    PlayerPrefs.SetInt("bestScore", lastScore);
                    // play sound
                    bestScoreTextMesh.text = "<style=c5>BEST SCORE</style> " + lastScore.ToString("D4");
                }
                else
                {
                    bestScoreTextMesh.text = "<style=c5>BEST SCORE</style> " + lastBestScore.ToString("D4");
                }

                yield return new WaitForSeconds(1.5f);
            }
            else
            {
                Debug.Log("To do: voltei para tela de titulo sem ter score salvo.");
            }

        }

        titleControlsTextMesh.gameObject.SetActive(true);
        isAnimating = false;
        firstOpen = false;
    }

    private void onScoreChanged()
    {
        scoreTextMesh.text = gameManager.score.ToString("D4");
    }
}
