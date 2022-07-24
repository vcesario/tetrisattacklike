using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource musicSource;
    [Range(1f, 3f)]
    public float fastPitch = 1.1f;
    [Space]
    public List<AudioSource> soundSources;
    public List<AudioItem> audioItems;

    private float bounceCooldown;
    private int soundSourceIndex;

    private void Start()
    {
        setMute(PlayerPrefs.GetInt("audioEnabled", 1) == 0);
    }

    private void Update()
    {
        if (bounceCooldown > 0)
            bounceCooldown -= Time.deltaTime;
    }

    private void OnValidate()
    {
        for (int k = 0; k < audioItems.Count; k++)
        {
            audioItems[k].name = audioItems[k].id.ToString();
        }
    }

    public void playMusic(bool reset, bool fast)
    {
        if (reset)
            musicSource.Stop();

        musicSource.Play();
    }

    public void musicSpeed(float to)
    {
        musicSpeed(to, 1);
    }
    
    LTDescr musicSpeedTweener;
    public void musicSpeed(float to, float duration)
    {
        if (musicSpeedTweener != null)
            LeanTween.cancel(musicSpeedTweener.uniqueId);

        musicSpeedTweener = LeanTween.value(musicSource.pitch, to, duration).setOnUpdate(delegate (float value)
        {
            musicSource.pitch = value;
        });
    }

    public void stopMusic()
    {
        musicSource.Stop();
    }

    public void playSound(AudioID id)
    {
        AudioItem item = audioItems.Find(_it => _it.id == id);
        if (item == null)
        {
            Debug.Log("Audio " + id.ToString() + " não encontrado!");
            return;
        }
        if (item.clips == null || item.clips.Length == 0)
        {
            Debug.Log("Audio " + id.ToString() + " não tem clips!");
            return;
        }

        if (id == AudioID.Bounce && bounceCooldown > 0)
            return;
        bounceCooldown = 0.05f;

        int randomClipIndex = Random.Range(0, item.clips.Length);
        soundSources[soundSourceIndex].Stop();
        soundSources[soundSourceIndex].clip = item.clips[randomClipIndex];
        soundSources[soundSourceIndex].Play();
        soundSourceIndex = (soundSourceIndex + 1) % soundSources.Count;
    }

    public void toggleMute()
    {
        int audioEnabled = PlayerPrefs.GetInt("audioEnabled", 1); // 1 com som, 0 mutado

        setMute(audioEnabled == 1);

        playSound(AudioID.UI_Confirm);
    }
    private void setMute(bool value)
    {
        musicSource.mute = value;
        foreach (var source in soundSources)
            source.mute = value;

        if (value)
            PlayerPrefs.SetInt("audioEnabled", 0);
        else
            PlayerPrefs.SetInt("audioEnabled", 1);
        PlayerPrefs.Save();
    }
}

[System.Serializable]
public class AudioItem
{
    public string name;
    public AudioID id;
    public AudioClip[] clips;
}

public enum AudioID
{
    Empty = 0,
    Slash = 1,
    Bounce = 2,
    UI_Popup = 3,
    UI_Confirm = 4,
    UI_Cancel = 5,
    UI_ConfirmBig = 6,
    Bell = 7,
    Match = 8,
    Swap = 9,
    Move = 10,
}

