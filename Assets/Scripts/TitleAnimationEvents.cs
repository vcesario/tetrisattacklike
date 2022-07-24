using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleAnimationEvents : MonoBehaviour
{
    public AudioManager audioManager;

    public void playMusic()
    {
        audioManager.playMusic(true, false);
    }

    public void playSound_Slash()
    {
        audioManager.playSound(AudioID.Slash);
    }
    public void playSound_Bounce()
    {
        audioManager.playSound(AudioID.Bounce);
    }
}
