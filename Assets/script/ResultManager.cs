using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ResultManager : MonoBehaviour
{
    [Header("UI要素")]
    public Text resultText;
    public Button nextButton;

    [Header("表示設定")]
    public string correctMessage = "正解！";
    public string wrongMessage = "残念…";

    [Header("デバッグ")]
    public bool showDebugInfo = true;

    void Start()
    {
        // 🎵 重要: ゲームBGMを継続再生（StageからResultへシーン遷移しても途切れない）
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameBGM();
            if (showDebugInfo) Debug.Log("[ResultManager] ゲームBGM継続");
        }
        else
        {
            Debug.LogError("[ResultManager] AudioManagerが見つかりません！");
        }

        // 結果を取得
        string result = PlayerPrefs.GetString("LastResult", "correct");
        bool isCorrect = (result == "correct");

        // 結果を表示
        DisplayResult(isCorrect);

        // 🎵 正解/不正解のSEを再生
        if (AudioManager.Instance != null)
        {
            if (isCorrect)
            {
                AudioManager.Instance.PlayCorrectSE();
                if (showDebugInfo) Debug.Log("[ResultManager] 正解SE再生");
            }
            else
            {
                AudioManager.Instance.PlayWrongSE();
                if (showDebugInfo) Debug.Log("[ResultManager] 不正解SE再生");
            }
        }

        // 次へボタンの設定
        nextButton.onClick.AddListener(OnNextButtonClicked);
    }

    void DisplayResult(bool isCorrect)
    {
        resultText.text = isCorrect ? correctMessage : wrongMessage;
    }

    void OnNextButtonClicked()
    {
        // 連打防止
        nextButton.interactable = false;

        // 🎵 ページめくりSEを再生
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPageFlipSE();
        }

        // 解説シーンへ遷移
        SceneManager.LoadScene("ExplanationScene");
    }
}