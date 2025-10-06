using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Centralized music manager that handles music across all scenes.
/// Manages music transitions, fading, and scene-specific music.
/// </summary>
public class MusicManager : MonoBehaviour
{
    [Header("Music Sources")]
    [Tooltip("Primary music audio source")]
    public AudioSource musicSource1;

    [Tooltip("Secondary music audio source for crossfading")]
    public AudioSource musicSource2;

    [Header("Main Menu Music")]
    [Tooltip("Intro music that plays once at the start of main menu")]
    public AudioClip mainMenuIntroMusic;

    [Tooltip("Loop music that plays continuously in main menu")]
    public AudioClip mainMenuLoopMusic;

    [Tooltip("Transition music that plays for 3 seconds when starting game")]
    public AudioClip mainMenuTransitionMusic;

    [Header("Game Music")]
    [Tooltip("Music that plays during day phase")]
    public AudioClip dayMusic;

    [Tooltip("Music that plays during night phase")]
    public AudioClip nightMusic;

    [Header("Music Settings")]
    [Tooltip("Master volume for all music")]
    [Range(0f, 1f)]
    public float masterMusicVolume = 0.7f;

    [Tooltip("Fade duration for music transitions")]
    public float fadeDuration = 2f;

    [Tooltip("Crossfade duration between music tracks")]
    public float crossfadeDuration = 1f;

    [Header("Debug")]
    [Tooltip("Enable debug logs")]
    public bool enableDebugLogs = false;

    // Singleton instance
    public static MusicManager Instance { get; private set; }

    // State tracking
    private bool isPlayingIntro = false;
    private bool isPlayingLoop = false;
    private bool isTransitioning = false;
    private Coroutine currentFadeCoroutine;
    private Coroutine currentCrossfadeCoroutine;

    // Scene tracking
    private string currentSceneName;
    private bool isMainMenu = false;
    private bool isMainScene = false;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeMusicSources();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Initialize based on current scene
        currentSceneName = SceneManager.GetActiveScene().name;
        CheckSceneType();
        StartSceneMusic();
    }

    void InitializeMusicSources()
    {
        // Create music sources if they don't exist
        if (musicSource1 == null)
        {
            musicSource1 = gameObject.AddComponent<AudioSource>();
            musicSource1.playOnAwake = false;
            musicSource1.loop = false;
        }

        if (musicSource2 == null)
        {
            musicSource2 = gameObject.AddComponent<AudioSource>();
            musicSource2.playOnAwake = false;
            musicSource2.loop = false;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentSceneName = scene.name;
        CheckSceneType();

        Debug.Log($"MusicManager: Scene loaded - {scene.name} (MainMenu: {isMainMenu}, MainScene: {isMainScene})");

        StartSceneMusic();
    }

    void CheckSceneType()
    {
        isMainMenu = currentSceneName == "MainMenu";
        isMainScene = currentSceneName == "MainScene";

        if (enableDebugLogs)
        {
            Debug.Log($"MusicManager: Scene type - MainMenu: {isMainMenu}, MainScene: {isMainScene}");
        }
    }

    void StartSceneMusic()
    {
        Debug.Log($"MusicManager: Starting scene music - isMainMenu: {isMainMenu}, isMainScene: {isMainScene}");

        if (isMainMenu)
        {
            StartMainMenuMusic();
        }
        else if (isMainScene)
        {
            StartGameMusic();
        }
        else
        {
            Debug.LogWarning($"MusicManager: Unknown scene type - {currentSceneName}. No music will play.");
        }
    }

    #region Main Menu Music

    void StartMainMenuMusic()
    {
        if (enableDebugLogs)
        {
            Debug.Log("MusicManager: Starting main menu music sequence");
        }

        // Stop any current music
        StopAllMusic();

        // Reset music states
        isPlayingIntro = false;
        isPlayingLoop = false;

        // Start intro music
        if (mainMenuIntroMusic != null)
        {
            PlayIntroMusic();
        }
        else
        {
            // If no intro music, go straight to loop
            PlayLoopMusic();
        }
    }

    void PlayIntroMusic()
    {
        if (mainMenuIntroMusic == null) return;

        isPlayingIntro = true;
        musicSource1.clip = mainMenuIntroMusic;
        musicSource1.loop = false;
        musicSource1.volume = masterMusicVolume;
        musicSource1.Play();

        if (enableDebugLogs)
        {
            Debug.Log($"MusicManager: Playing intro music - {mainMenuIntroMusic.name}");
        }

        // Start coroutine to transition to loop when intro ends
        StartCoroutine(WaitForIntroToEnd());
    }

    IEnumerator WaitForIntroToEnd()
    {
        // Wait for intro music to finish
        yield return new WaitUntil(() => !musicSource1.isPlaying);

        if (enableDebugLogs)
        {
            Debug.Log("MusicManager: Intro music finished, checking if should transition to loop");
        }

        isPlayingIntro = false;

        // Only play loop music if we're still in the main menu scene
        if (isMainMenu)
        {
            PlayLoopMusic();
        }
        else
        {
            if (enableDebugLogs)
            {
                Debug.Log("MusicManager: Not in main menu scene, skipping loop music");
            }
        }
    }

    void PlayLoopMusic()
    {
        if (mainMenuLoopMusic == null) return;

        // Safety check: only play loop music in main menu scene
        if (!isMainMenu)
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning("MusicManager: Attempted to play loop music outside main menu scene - blocked!");
            }
            return;
        }

        isPlayingLoop = true;
        musicSource1.clip = mainMenuLoopMusic;
        musicSource1.loop = true;
        musicSource1.volume = masterMusicVolume;
        musicSource1.Play();

        if (enableDebugLogs)
        {
            Debug.Log($"MusicManager: Playing loop music - {mainMenuLoopMusic.name}");
        }
    }

    public void PlayTransitionMusic()
    {
        if (mainMenuTransitionMusic == null) return;

        if (enableDebugLogs)
        {
            Debug.Log("MusicManager: Playing transition music");
        }

        // Stop current music and play transition
        StopAllMusic();

        musicSource1.clip = mainMenuTransitionMusic;
        musicSource1.loop = false;
        musicSource1.volume = masterMusicVolume;
        musicSource1.Play();

        // Start coroutine to continue playing for 3 seconds
        StartCoroutine(PlayTransitionForDuration());
    }

    IEnumerator PlayTransitionForDuration()
    {
        // Play transition music
        yield return new WaitForSeconds(3f);

        if (enableDebugLogs)
        {
            Debug.Log("MusicManager: Transition music finished, loading main scene");
        }

        // Load main scene
        SceneManager.LoadScene("MainScene");
    }

    #endregion

    #region Game Music

    void StartGameMusic()
    {
        if (enableDebugLogs)
        {
            Debug.Log("MusicManager: Starting game music");
        }

        // Stop any current music (including loop music from main menu)
        StopAllMusic();

        // Ensure loop music state is reset
        isPlayingLoop = false;

        // Start with day music by default
        PlayDayMusic();

        // If no day music is assigned, stop all music to prevent main menu music from continuing
        if (dayMusic == null)
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning("MusicManager: No day music assigned! Please assign day music clip in the Inspector.");
            }
            StopAllMusic();
        }
    }

    public void PlayDayMusic()
    {
        if (dayMusic == null)
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning("MusicManager: No day music assigned! Please assign day music clip in the Inspector.");
            }
            return;
        }

        if (enableDebugLogs)
        {
            Debug.Log("MusicManager: Playing day music");
        }

        // Crossfade to day music
        CrossfadeToMusic(dayMusic, true);
    }

    public void PlayNightMusic()
    {
        if (nightMusic == null)
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning("MusicManager: No night music assigned! Please assign night music clip in the Inspector.");
            }
            return;
        }

        if (enableDebugLogs)
        {
            Debug.Log("MusicManager: Playing night music");
        }

        // Crossfade to night music
        CrossfadeToMusic(nightMusic, true);
    }

    #endregion

    #region Music Control Methods

    void CrossfadeToMusic(AudioClip newClip, bool loop)
    {
        if (newClip == null) return;

        if (currentCrossfadeCoroutine != null)
        {
            StopCoroutine(currentCrossfadeCoroutine);
        }

        currentCrossfadeCoroutine = StartCoroutine(CrossfadeCoroutine(newClip, loop));
    }

    IEnumerator CrossfadeCoroutine(AudioClip newClip, bool loop)
    {
        isTransitioning = true;

        // Determine which source to use for new music
        AudioSource sourceToUse = musicSource1.isPlaying ? musicSource2 : musicSource1;
        AudioSource sourceToFade = musicSource1.isPlaying ? musicSource1 : musicSource2;

        // Start new music at volume 0
        sourceToUse.clip = newClip;
        sourceToUse.loop = loop;
        sourceToUse.volume = 0f;
        sourceToUse.Play();

        // Fade out old music and fade in new music
        float elapsedTime = 0f;
        float startVolumeOld = sourceToFade.volume;

        while (elapsedTime < crossfadeDuration)
        {
            float t = elapsedTime / crossfadeDuration;

            // Fade out old music
            sourceToFade.volume = Mathf.Lerp(startVolumeOld, 0f, t);

            // Fade in new music
            sourceToUse.volume = Mathf.Lerp(0f, masterMusicVolume, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure final volumes
        sourceToFade.volume = 0f;
        sourceToUse.volume = masterMusicVolume;

        // Stop old music
        sourceToFade.Stop();

        isTransitioning = false;
        currentCrossfadeCoroutine = null;

        if (enableDebugLogs)
        {
            Debug.Log($"MusicManager: Crossfade completed to {newClip.name}");
        }
    }

    void StopAllMusic()
    {
        if (musicSource1.isPlaying)
        {
            musicSource1.Stop();
        }

        if (musicSource2.isPlaying)
        {
            musicSource2.Stop();
        }

        // Stop any running coroutines
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
            currentFadeCoroutine = null;
        }

        if (currentCrossfadeCoroutine != null)
        {
            StopCoroutine(currentCrossfadeCoroutine);
            currentCrossfadeCoroutine = null;
        }

        isPlayingIntro = false;
        isPlayingLoop = false;
        isTransitioning = false;
    }

    public void SetMusicVolume(float volume)
    {
        masterMusicVolume = Mathf.Clamp01(volume);

        // Update current playing sources
        if (musicSource1.isPlaying)
        {
            musicSource1.volume = masterMusicVolume;
        }

        if (musicSource2.isPlaying)
        {
            musicSource2.volume = masterMusicVolume;
        }
    }

    #endregion

    #region Public API

    /// <summary>
    /// Called when player presses start button in main menu
    /// </summary>
    public void OnStartButtonPressed()
    {
        if (isMainMenu)
        {
            PlayTransitionMusic();
        }
    }

    /// <summary>
    /// Called when day phase starts in main scene
    /// </summary>
    public void OnDayStarted()
    {
        if (isMainScene)
        {
            PlayDayMusic();
        }
    }

    /// <summary>
    /// Called when night phase starts in main scene
    /// </summary>
    public void OnNightStarted()
    {
        if (isMainScene)
        {
            PlayNightMusic();
        }
    }

    /// <summary>
    /// Pause all music
    /// </summary>
    public void PauseMusic()
    {
        if (musicSource1.isPlaying)
        {
            musicSource1.Pause();
        }

        if (musicSource2.isPlaying)
        {
            musicSource2.Pause();
        }
    }

    /// <summary>
    /// Resume all music
    /// </summary>
    public void ResumeMusic()
    {
        if (musicSource1.clip != null && !musicSource1.isPlaying)
        {
            musicSource1.UnPause();
        }

        if (musicSource2.clip != null && !musicSource2.isPlaying)
        {
            musicSource2.UnPause();
        }
    }

    #endregion

    #region Debug Methods

    [ContextMenu("Test Main Menu Music")]
    public void TestMainMenuMusic()
    {
        Debug.Log("MusicManager: Testing main menu music sequence");
        StartMainMenuMusic();
    }

    [ContextMenu("Test Day Music")]
    public void TestDayMusic()
    {
        Debug.Log("MusicManager: Testing day music");
        PlayDayMusic();
    }

    [ContextMenu("Test Night Music")]
    public void TestNightMusic()
    {
        Debug.Log("MusicManager: Testing night music");
        PlayNightMusic();
    }

    [ContextMenu("Test Transition Music")]
    public void TestTransitionMusic()
    {
        Debug.Log("MusicManager: Testing transition music");
        PlayTransitionMusic();
    }

    [ContextMenu("Stop All Music")]
    public void StopAllMusicDebug()
    {
        Debug.Log("MusicManager: Stopping all music");
        StopAllMusic();
    }

    #endregion

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
