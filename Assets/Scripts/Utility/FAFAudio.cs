using UnityEngine;
using System.Collections;

public class FAFAudio : MonoBehaviour 
{
    public static FAFAudio Instance 
    { 
        get 
        { 
            if(instance == null)
            {
                GameObject gobj = new GameObject("FAFAudio");
                instance = gobj.AddComponent<FAFAudio>();
            }
            return instance; 
        } 
    } 
    static FAFAudio instance = null;

    AudioSource musicSource = null;
    AudioSource nextMusicSource = null;
    float fadeIn = 1;
    float fadeOut = 1;
    bool crossfade = false;

    public void PlayOnce2D(AudioClip _clip, Vector3 _pos, float _volume)
    {
        if (_clip)
        {
            GameObject gobj = new GameObject(_clip.name);
            AudioSource source = gobj.AddComponent<AudioSource>();
            AutoDestruct autoDestruct = gobj.AddComponent<AutoDestruct>();

            gobj.transform.position = _pos;

            source.clip = _clip;
            source.volume = _volume;
            source.Play();

            autoDestruct.delay = _clip.length;
        }
    }

    public void TryPlayMusic(AudioClip _clip, float _fadeOut = 1, float _fadeIn = 1, bool _crossFade = false)
    {
        if ((musicSource && musicSource.clip == _clip) || //check current
            (nextMusicSource && nextMusicSource.clip == _clip))//check next (transition)
        {
            return;
        }
        PlayMusic(_clip, _fadeOut, _fadeIn, _crossFade);
    }

    public void PlayMusic(AudioClip _clip, float _fadeOut = 1, float _fadeIn = 1, bool _crossFade = false)
    {
        if (nextMusicSource)
            Destroy(nextMusicSource);

        GameObject gobj = new GameObject("NextMusic");
        DontDestroyOnLoad(gobj);
        nextMusicSource = gobj.AddComponent<AudioSource>();
        nextMusicSource.clip = _clip;
        nextMusicSource.loop = true;
        nextMusicSource.Play();
        nextMusicSource.volume = 0;

        fadeIn = _fadeIn;
        fadeOut = _fadeOut;
        crossfade = _crossFade;
    }

	void Awake () 
    {
        //there can only be one!
        if (instance)
            Destroy(this.gameObject);
	}
	
	// Update is called once per frame
	void Update () 
    {
	    if (nextMusicSource != null)
        {
            const float targetVolume = 0.5f;
            if(musicSource && musicSource.volume > 0 && fadeOut > 0)
            {
                musicSource.volume -= Time.deltaTime / fadeOut;
            }
            else if(nextMusicSource.volume < targetVolume && fadeIn > 0)
            {
                nextMusicSource.volume += Time.deltaTime / fadeIn;
            }
            else
            {
                if(musicSource)
                    Destroy(musicSource.gameObject);

                musicSource = nextMusicSource;
                musicSource.name = "CurrentMusic";
                musicSource.volume = targetVolume;
                nextMusicSource = null;
            }
        }
	}
}
