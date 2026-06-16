using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// ゲームクリア画面を管理するクラス
public class GameClearManager : MonoBehaviour
{
    [Header("UI要素")]
    public Button restartButton;      // 同じステージをもう一度やるボタン
    public Button titleButton;        // タイトル画面に戻るボタン
    public Button reviewButton;       // 今回の問題一覧を見るボタン

    [Header("シーン名設定")]
    public string titleSceneName = "Title";       // タイトルシーンの名前
    public string stageSceneName = "Stage";       // ステージシーンの名前
    public string reviewSceneName = "QuizReview"; // 問題一覧シーンの名前

    [Header("デバッグ")]
    public bool showDebugInfo = true;

    // ── シーンが始まったときに自動で呼ばれる ─────────────────────
    void Start()
    {
        if (showDebugInfo) Debug.Log("[GameClearManager] Start実行");

        // 各ボタンに押されたときの関数を登録する
        restartButton.onClick.AddListener(OnRestartButtonClicked);
        titleButton.onClick.AddListener(OnTitleButtonClicked);
        reviewButton.onClick.AddListener(OnReviewButtonClicked);

        // クリア用BGMを再生（AudioManagerがあれば）
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClearBGM();
        }
    }

    // ── 「リスタート」ボタンが押されたときの処理 ─────────────────
    void OnRestartButtonClicked()
    {
        if (showDebugInfo) Debug.Log("[GameClearManager] リスタート");

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClickSE();

        // ゲームデータをリセットしてからステージへ
        // リセットしないと前回の問題リストやスコアが残ったままになる
        GameManager.Instance.ResetGame();
        SceneManager.LoadScene(stageSceneName);
    }

    // ── 「タイトルに戻る」ボタンが押されたときの処理 ──────────────
    void OnTitleButtonClicked()
    {
        if (showDebugInfo) Debug.Log("[GameClearManager] タイトルへ");

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClickSE();

        // ゲームデータをリセットしてタイトルへ
        GameManager.Instance.ResetGame();
        SceneManager.LoadScene(titleSceneName);
    }

    // ── 「今回の問題」ボタンが押されたときの処理 ─────────────────
    void OnReviewButtonClicked()
    {
        // 遷移直前のデータを確認する
        Debug.Log($"[GameClearManager] 遷移直前のSelectedQuizzes.Count: {GameManager.Instance.SelectedQuizzes.Count}");

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClickSE();

        SceneManager.LoadScene(reviewSceneName);
    }
}