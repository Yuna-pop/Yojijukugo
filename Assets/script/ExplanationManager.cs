using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ExplanationManager : MonoBehaviour
{
    [Header("UI要素")]
    public Text displayText;
    public Text readText;
    public Text explanationText;
    public Button nextButton;

    [Header("デバッグ")]
    public bool showDebugInfo = true;

    void Start()
    {
        // 🎵 重要: 解説シーンでもBGMを継続（ResultBGMのまま）
        // もし専用BGMが欲しい場合は、AudioManagerに追加して再生
        if (showDebugInfo) Debug.Log("[ExplanationManager] Start実行");

        // GameManagerから現在の問題情報を取得
        var quiz = GameManager.Instance.LastQuiz;

        if (quiz == null)
        {
            Debug.LogError("[ExplanationManager] 解説する問題が見つかりません！");
            displayText.text = "エラー：問題情報を取得できませんでした";
            return;
        }

        // 解説を表示
        DisplayExplanation(quiz);

        // 次へボタンの設定
        nextButton.onClick.AddListener(OnNextButtonClicked);
    }

    /// <summary>
    /// 解説を表示
    /// </summary>
    void DisplayExplanation(QuizLoader.QuizData quiz)
    {
        displayText.text = quiz.full_phrase;
        readText.text = $"読み: {quiz.read}";
        explanationText.text = quiz.explanation;

        if (showDebugInfo)
        {
            Debug.Log($"[ExplanationManager] 解説表示: {quiz.full_phrase}");
        }
    }

    /// <summary>
    /// 次へボタンが押された時の処理
    /// </summary>
    void OnNextButtonClicked()
    {
        // 連打防止
        nextButton.interactable = false;

        // 🎵 ボタンクリックSEを再生
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClickSE();
        }

        // ゲーム終了条件をチェック
        if (GameManager.Instance.IsGameOver())
        {
            // 3回間違えた → ゲームオーバー
            if (showDebugInfo) Debug.Log("[ExplanationManager] ゲームオーバー（3回間違い）");
            SceneManager.LoadScene("GameOverScene");
        }
        else if (GameManager.Instance.IsGameClear())
        {
            // 10問終了 → クリア
            if (showDebugInfo) Debug.Log("[ExplanationManager] ゲームクリア（10問完了）");
            SceneManager.LoadScene("GameClearScene");
        }
        else
        {
            // 次の問題へ
            GameManager.Instance.MoveToNextQuestion();
            if (showDebugInfo)
                Debug.Log($"[ExplanationManager] 次の問題へ: {GameManager.Instance.GetGameStatus()}");
            SceneManager.LoadScene("Stage");
        }
    }
}