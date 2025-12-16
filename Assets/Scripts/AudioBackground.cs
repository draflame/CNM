using UnityEngine;

/// <summary>
/// Script quản lý nhạc nền cho scene
/// Đặt vào GameObject trong mỗi scene để phát nhạc nền riêng
/// </summary>
public class AudioBackground : MonoBehaviour
{
    [Header("Audio Settings")]
    [Tooltip("AudioSource component để phát nhạc")]
    [SerializeField] private AudioSource audioSource;

    [Tooltip("AudioClip nhạc nền cho scene này")]
    [SerializeField] private AudioClip backgroundMusic;

    [Tooltip("Âm lượng nhạc nền (0-1)")]
    [SerializeField] private float volume = 0.5f;

    [Tooltip("Tự động phát nhạc khi scene load")]
    [SerializeField] private bool playOnStart = true;

    [Tooltip("Lặp lại nhạc nền")]
    [SerializeField] private bool loop = true;

    private void Start()
    {
        // Nếu không có AudioSource, tự động lấy hoặc tạo mới
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                Debug.Log("🎵 AudioBackground: Tự động tạo AudioSource component");
            }
        }

        // Setup AudioSource
        audioSource.clip = backgroundMusic;
        audioSource.volume = volume;
        audioSource.loop = loop;
        audioSource.playOnAwake = false;

        // Phát nhạc nếu được bật
        if (playOnStart && backgroundMusic != null)
        {
            PlayMusic();
        }
        else if (backgroundMusic == null)
        {
            Debug.LogWarning("⚠️ AudioBackground: Chưa có AudioClip! Kéo nhạc nền vào Inspector.");
        }
    }

    /// <summary>
    /// Phát nhạc nền
    /// </summary>
    public void PlayMusic()
    {
        if (audioSource != null && backgroundMusic != null && !audioSource.isPlaying)
        {
            audioSource.Play();
            Debug.Log($"🎵 Playing background music: {backgroundMusic.name}");
        }
    }

    /// <summary>
    /// Dừng nhạc nền
    /// </summary>
    public void StopMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            Debug.Log("⏹️ Stopped background music");
        }
    }

    /// <summary>
    /// Tạm dừng nhạc nền
    /// </summary>
    public void PauseMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Pause();
            Debug.Log("⏸️ Paused background music");
        }
    }

    /// <summary>
    /// Tiếp tục phát nhạc (sau khi pause)
    /// </summary>
    public void ResumeMusic()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.UnPause();
            Debug.Log("▶️ Resumed background music");
        }
    }

    /// <summary>
    /// Đổi âm lượng nhạc nền
    /// </summary>
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }

    /// <summary>
    /// Đổi nhạc nền sang clip khác
    /// </summary>
    public void ChangeMusic(AudioClip newClip)
    {
        if (newClip == null) return;

        StopMusic();
        backgroundMusic = newClip;
        audioSource.clip = newClip;
        PlayMusic();
    }
}
