using UnityEngine;
using System.Collections;

/// <summary>
/// BGMとSE（効果音）を管理するシングルトン
/// Inspectorで音声ファイルを設定できます
/// </summary>
public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;
    public static AudioManager Instance
    {
        get
        {
            // 🔧 修正1: シーン内の既存AudioManagerを優先的に探す
            if (_instance == null)
            {
                _instance = FindObjectOfType<AudioManager>();

                // シーン内にもない場合は新規作成
                if (_instance == null)
                {
                    GameObject go = new GameObject("AudioManager");
                    _instance = go.AddComponent<AudioManager>();
                }

                DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        }
    }

    // 🎵 Inspector設定: BGM用AudioClip
    [Header("BGM設定")]
    [Tooltip("タイトル画面のBGM")]
    public AudioClip titleBGM;

    [Tooltip("ゲーム中（問題画面・結果画面）のBGM")]
    public AudioClip gameBGM;

    [Tooltip("解説画面のBGM（nullの場合はgameBGMを継続）")]
    public AudioClip explanationBGM;

    [Tooltip("クリア画面のBGM")]
    public AudioClip clearBGM;

    [Tooltip("ゲームオーバー画面のBGM")]
    public AudioClip gameOverBGM;

    // 🔊 Inspector設定: SE用AudioClip
    [Header("効果音設定")]
    [Tooltip("正解時のSE")]
    public AudioClip correctSE;

    [Tooltip("不正解時のSE")]
    public AudioClip wrongSE;

    [Tooltip("ボタンクリック時のSE（タイピング音など）")]
    public AudioClip buttonClickSE;

    [Tooltip("ページめくり音（次へボタンなど）")]
    public AudioClip pageFlipSE;

    // 🎚️ Inspector設定: 音量調整
    [Header("音量設定")]
    [Range(0f, 1f)]
    [Tooltip("BGMの音量")]
    public float bgmVolume = 0.5f;

    [Range(0f, 1f)]
    [Tooltip("SEの音量")]
    public float seVolume = 0.8f;

    [Header("フェード設定")]
    [Tooltip("BGM切り替え時のフェード時間（秒）")]
    public float fadeDuration = 1f;

    // 🔧 修正2: デバッグモード追加
    [Header("デバッグ")]
    [Tooltip("デバッグログを表示するか")]
    public bool showDebugLog = true;

    // AudioSource（プログラム側で管理）
    private AudioSource bgmSource;
    private AudioSource seSource;
    private AudioClip currentBGMClip; // 🔧 追加: 現在再生中のAudioClip
    private bool isInitialized = false;

    void Awake()
    {
        // 🔧 修正3: 既に別のインスタンスが存在する場合は破棄
        if (_instance != null && _instance != this)
        {
            if (showDebugLog) Debug.Log("[AudioManager] 重複インスタンスを破棄");
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        // AudioSourceを初期化
        SetupAudioSources();

        if (showDebugLog) Debug.Log("[AudioManager] 初期化完了");
    }

    /// <summary>
    /// AudioSourceコンポーネントを初期化
    /// </summary>
    void SetupAudioSources()
    {
        // 既存のAudioSourceを削除（重複防止）
        AudioSource[] existingSources = GetComponents<AudioSource>();
        foreach (var source in existingSources)
        {
            Destroy(source);
        }

        // BGM用AudioSourceを作成
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.volume = bgmVolume;

        // SE用AudioSourceを作成
        seSource = gameObject.AddComponent<AudioSource>();
        seSource.loop = false;
        seSource.playOnAwake = false;
        seSource.volume = seVolume;

        isInitialized = true;

        if (showDebugLog)
        {
            Debug.Log($"[AudioManager] AudioSource作成完了 - BGM音量:{bgmVolume}, SE音量:{seVolume}");
        }
    }

    /// <summary>
    /// 🔧 重要な修正: BGMを再生（同じBGMなら継続再生）
    /// </summary>
    public void PlayBGM(AudioClip clip, string bgmName = "")
    {
        if (!isInitialized)
        {
            Debug.LogError("[AudioManager] 初期化されていません！");
            return;
        }

        if (clip == null)
        {
            Debug.LogWarning($"[AudioManager] BGMがnullです: {bgmName}");
            return;
        }

        // 🎵 重要: 同じBGMが既に再生中なら何もしない（継続再生）
        if (currentBGMClip == clip && bgmSource.isPlaying)
        {
            if (showDebugLog) Debug.Log($"[AudioManager] 既に再生中のため継続: {bgmName} ({clip.name})");
            return;
        }

        if (showDebugLog) Debug.Log($"[AudioManager] BGM再生開始: {bgmName} ({clip.name})");

        currentBGMClip = clip;
        StartCoroutine(FadeOutAndPlayBGM(clip));
    }

    /// <summary>
    /// BGMをフェードアウトしてから新しいBGMを再生
    /// </summary>
    IEnumerator FadeOutAndPlayBGM(AudioClip newClip)
    {
        // 現在のBGMをフェードアウト
        if (bgmSource.isPlaying)
        {
            float startVolume = bgmSource.volume;
            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                bgmSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
                yield return null;
            }
            bgmSource.Stop();
        }

        // 新しいBGMを設定して再生
        bgmSource.clip = newClip;
        bgmSource.volume = 0;
        bgmSource.Play();

        if (showDebugLog) Debug.Log($"[AudioManager] BGM再生中: {newClip.name}");

        // フェードイン
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(0, bgmVolume, t / fadeDuration);
            yield return null;
        }
        bgmSource.volume = bgmVolume;
    }

    /// <summary>
    /// BGMを停止
    /// </summary>
    public void StopBGM()
    {
        if (!isInitialized) return;
        StartCoroutine(FadeOutBGM());
    }

    IEnumerator FadeOutBGM()
    {
        float startVolume = bgmSource.volume;
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
            yield return null;
        }
        bgmSource.Stop();
        bgmSource.volume = bgmVolume;
        currentBGMClip = null;

        if (showDebugLog) Debug.Log("[AudioManager] BGM停止");
    }

    /// <summary>
    /// SEを再生
    /// </summary>
    public void PlaySE(AudioClip clip)
    {
        if (!isInitialized)
        {
            Debug.LogError("[AudioManager] 初期化されていません！");
            return;
        }

        if (clip == null)
        {
            Debug.LogWarning("[AudioManager] SEがnullです");
            return;
        }

        seSource.PlayOneShot(clip, seVolume);

        if (showDebugLog) Debug.Log($"[AudioManager] SE再生: {clip.name}");
    }

    // 🎵 便利メソッド: シーン別BGM再生
    public void PlayTitleBGM()
    {
        if (titleBGM == null)
        {
            Debug.LogError("[AudioManager] titleBGMがInspectorで設定されていません！");
            return;
        }
        PlayBGM(titleBGM, "Title");
    }

    /// <summary>
    /// 🔧 変更: ゲーム中BGM（Stage・Result・Explanationで共通）
    /// </summary>
    public void PlayGameBGM()
    {
        if (gameBGM == null)
        {
            Debug.LogError("[AudioManager] gameBGMがInspectorで設定されていません！");
            return;
        }
        PlayBGM(gameBGM, "Game");
    }

    /// <summary>
    /// 🆕 追加: 解説BGM（nullの場合はgameBGMを継続）
    /// </summary>
    public void PlayExplanationBGM()
    {
        if (explanationBGM != null)
        {
            PlayBGM(explanationBGM, "Explanation");
        }
        else
        {
            // explanationBGMが設定されていない場合はgameBGMを継続
            PlayGameBGM();
        }
    }

    public void PlayClearBGM()
    {
        if (clearBGM == null)
        {
            Debug.LogError("[AudioManager] clearBGMがInspectorで設定されていません！");
            return;
        }
        PlayBGM(clearBGM, "Clear");
    }

    public void PlayGameOverBGM()
    {
        if (gameOverBGM == null)
        {
            Debug.LogError("[AudioManager] gameOverBGMがInspectorで設定されていません！");
            return;
        }
        PlayBGM(gameOverBGM, "GameOver");
    }

    // 🔊 便利メソッド: SE再生
    public void PlayCorrectSE() => PlaySE(correctSE);
    public void PlayWrongSE() => PlaySE(wrongSE);
    public void PlayButtonClickSE() => PlaySE(buttonClickSE);
    public void PlayPageFlipSE() => PlaySE(pageFlipSE);

    /// <summary>
    /// 音量を動的に変更
    /// </summary>
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        if (bgmSource != null)
        {
            bgmSource.volume = bgmVolume;
        }
    }

    public void SetSEVolume(float volume)
    {
        seVolume = Mathf.Clamp01(volume);
        if (seSource != null)
        {
            seSource.volume = seVolume;
        }
    }

    // 🔧 修正4: デバッグ用メソッド - 設定状況を確認
    [ContextMenu("設定状況を確認")]
    public void CheckSettings()
    {
        Debug.Log("=== AudioManager 設定状況 ===");
        Debug.Log($"Title BGM: {(titleBGM != null ? titleBGM.name : "未設定")}");
        Debug.Log($"Game BGM: {(gameBGM != null ? gameBGM.name : "未設定")}");
        Debug.Log($"Explanation BGM: {(explanationBGM != null ? explanationBGM.name : "未設定（gameBGM継続）")}");
        Debug.Log($"Clear BGM: {(clearBGM != null ? clearBGM.name : "未設定")}");
        Debug.Log($"GameOver BGM: {(gameOverBGM != null ? gameOverBGM.name : "未設定")}");
        Debug.Log($"現在再生中: {(currentBGMClip != null ? currentBGMClip.name : "なし")}");
        Debug.Log($"BGM音量: {bgmVolume}");
        Debug.Log($"SE音量: {seVolume}");
        Debug.Log($"初期化状態: {isInitialized}");
        Debug.Log($"BGMSource存在: {bgmSource != null}");
        Debug.Log($"SESource存在: {seSource != null}");
    }
}