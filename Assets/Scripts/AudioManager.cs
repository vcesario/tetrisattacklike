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

    private void Update()
    {
        if (bounceCooldown > 0)
            bounceCooldown -= Time.deltaTime;
    }

    public void playMusic(bool reset, bool fast)
    {
        if (reset)
            musicSource.Stop();

        musicSource.Play();
    }

    public void musicSpeed(float to)
    {
        LeanTween.value(musicSource.pitch, to, 1f).setOnUpdate(delegate (float value)
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
}

[System.Serializable]
public class AudioItem
{
    public AudioID id;
    public AudioClip[] clips;
}

public enum AudioID
{
    Empty = 0,
    Slash = 1,
    Bounce = 2,
}

