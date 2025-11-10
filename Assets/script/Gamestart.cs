using UnityEngine;
using UnityEngine.SceneManagement;

public class Gamestart : MonoBehaviour
{
    private bool firstPush = false;

    [Header("設定")]
    public string stageSceneName = "Stage";

    [Header("デバッグ")]
    public bool testBGMOnStart = true; // テスト用  

    private bool isTransitioning = false;

    [System.Obsolete]
    void Start()
    {
        Debug.Log("[GameStart] Start() 実行");

        // 🔧 テスト: AudioManagerが正しく初期化されているか確認  
        if (AudioManager.Instance != null)
        {
            Debug.Log("[GameStart] AudioManagerインスタンス取得成功");

            if (testBGMOnStart)
            {
                Debug.Log("[GameStart] タイトルBGM再生を試行");
                AudioManager.Instance.PlayTitleBGM();
            }
        }
        else
        {
            Debug.LogError("[GameStart] AudioManagerインスタンスがnull！");
        }
    }

    [System.Obsolete]
    public void PressStart()
    {
        Debug.Log("[GameStart] PressStart() 呼び出し");

        // 遷移中は無視  
        if (isTransitioning)
        {
            Debug.Log("[GameStart] 既に遷移中");
            return;
        }

        isTransitioning = true;

        // ボタンクリックSEを再生  
        AudioManager.Instance.PlayButtonClickSE();

        // ゲームをリセット（新規ゲーム開始）  
        GameManager.Instance.ResetGame();

        // ステージシーンへ遷移  
        Debug.Log($"[GameStart] シーン遷移: {stageSceneName}");
        SceneManager.LoadScene(stageSceneName);
    }

    // 🔧 テスト用: インスペクターから手動でBGM再生テスト  
    [ContextMenu("BGM再生テスト")]
    [System.Obsolete]
    void TestPlayBGM()
    {
        if (AudioManager.Instance != null)
        {
            Debug.Log("BGM再生テスト実行");
            AudioManager.Instance.PlayTitleBGM();
        }
        else
        {
            Debug.LogError("AudioManagerが見つかりません");
        }
    }
}
