using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public static AudioManager instance;

    [Header("Audio Sources")]
    [Tooltip("Komponen AudioSource untuk Musik")]
    public AudioSource musicSource;

    [Tooltip("Komponen AudioSource untuk sound effects")]
    public AudioSource sfxSource;
    [Tooltip("Komponen AudioSource khusus untuk Dialog")]
    public AudioSource dialogSource;

    [Header("Startup")]
    [SerializeField] private bool playMusicOnStart = true;
    [SerializeField] private string startMusicName;
    [SerializeField] private bool loopMusic = true;

    //Struktur data kustom untuk menyimpan nama file audio

    [Serializable]
    public struct Sound
    {
        public string name;
        public AudioClip clip;
    }

    [Header("Audio Clips Library")]
    public Sound[] musicSounds;
    public Sound[] sfxSounds;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        EnsureAudioSources();
    }
    // Start is called before the first frame update
    void Start()
    {
        if (playMusicOnStart && !string.IsNullOrWhiteSpace(startMusicName))
        {
            PlayMusic(startMusicName);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PlayMusic(string soundName)
    {
        if (musicSource == null)
        {
            Debug.LogError("Music source belum di-assign.", this);
            return;
        }

        Sound s = Array.Find(musicSounds, x => x.name == soundName);

        if (s.clip == null)
        {
            Debug.LogWarning("Musik: " + soundName + " Tidak ditemukan");
            return;
        }
        musicSource.loop = loopMusic;
        musicSource.clip = s.clip;
        musicSource.Play();
    }

    public void PlaySFX(string soundName)
    {
        if (sfxSource == null)
        {
            Debug.LogError("SFX source belum di-assign.", this);
            return;
        }

        Sound s = Array.Find(sfxSounds, x => x.name == soundName);

        if (s.clip == null)
        {
            Debug.LogWarning("SFX: " + soundName + " Tidak ditemukan");
            return;
        }

        sfxSource.PlayOneShot(s.clip);
    }

    // Fungsi untuk memutar dialog (akan memotong dialog sebelumnya jika ada yang sedang bunyi)
    public void PlayDialog(string soundName)
    {
        if (dialogSource == null)
        {
            Debug.LogError("Dialog source belum di-assign.", this);
            return;
        }

        Sound s = Array.Find(sfxSounds, x => x.name == soundName);

        if (s.clip == null)
        {
            Debug.LogWarning("Dialog: " + soundName + " tidak ditemukan!");
            return;
        }

        // Kita BUKAN menggunakan PlayOneShot, melainkan mengganti clip utama lalu Play()
        dialogSource.clip = s.clip;
        dialogSource.Play();
    }

    // Fungsi untuk menghentikan dialog secara spesifik
    public void StopDialog()
    {
        if (dialogSource.isPlaying)
        {
            dialogSource.Stop();
        }
    }

    public void StopMusic()
    {
        if (musicSource == null)
        {
            Debug.LogError("Music source belum di-assign.", this);
            return;
        }

        musicSource.Stop();
    }

    public void ToggleMusic()
    {
        if (musicSource == null)
        {
            Debug.LogError("Music source belum di-assign.", this);
            return;
        }

        musicSource.mute = !musicSource.mute;
    }

    public void ToggleSFX()
    {
        if (sfxSource == null)
        {
            Debug.LogError("SFX source belum di-assign.", this);
            return;
        }

        sfxSource.mute = !sfxSource.mute;
    }

    private void EnsureAudioSources()
    {
        if (musicSource != null && sfxSource != null && dialogSource != null)
        {
            return;
        }

        AudioSource[] sources = GetComponents<AudioSource>();
        int index = 0;

        if (musicSource == null && sources.Length > index) musicSource = sources[index++];
        if (sfxSource == null && sources.Length > index) sfxSource = sources[index++];
        if (dialogSource == null && sources.Length > index) dialogSource = sources[index++];

        if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
        if (dialogSource == null) dialogSource = gameObject.AddComponent<AudioSource>();
    }
}
