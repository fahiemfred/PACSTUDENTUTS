using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public AudioSource musicSource;
    public AudioClip introMusic;
    public AudioClip ghostNormalMusic;

    void Start()
    {
        StartCoroutine(PlayMusic());
    }

    IEnumerator PlayMusic()
    {
        musicSource.clip = introMusic;
        musicSource.loop = false;
        musicSource.Play();

        float waitTime = Mathf.Min(introMusic.length, 3f);
        yield return new WaitForSeconds(waitTime);

        musicSource.clip = ghostNormalMusic;
        musicSource.loop = true;
        musicSource.Play();
    }
}

