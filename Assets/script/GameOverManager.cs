using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// ゲームオーバー画面を管理するクラス
// GameClearManagerとほぼ同じ構成
public class GameOverManager : MonoBehaviour
{
    [Header("UI要素")]
    public Button restartButton;      // 同じステージをもう一度やるボタン
    public Button titleButton;        // タイトル画面に戻るボタン
    public Button reviewButton;       // 今回の問題一覧を見るボタン

    [Header("シーン名設定")]
    public string titleSceneName = "Title";
    public string stageSceneName = "Stage";
    public string reviewSceneName = "QuizReview";

    [Header("デバッグ")]
    public bool showDebugInfo = true;

    // ── シーンが始まったときに自動で呼ばれる ─────────────────────
    void Start()
    {
        if (showDebugInfo) Debug.Log("[GameOverManager] Start実行");

        restartButton.onClick.AddListener(OnRestartButtonClicked);
        titleButton.onClick.AddListener(OnTitleButtonClicked);
        reviewButton.onClick.AddListener(OnReviewButtonClicked);

        // ゲームオーバー用BGMを再生
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameOverBGM();
        }
    }

    // ── 「リスタート」ボタンが押されたときの処理 ─────────────────
    void OnRestartButtonClicked()
    {
        if (showDebugInfo) Debug.Log("[GameOverManager] リスタート");

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClickSE();

        // リセットしてステージへ（問題もライフも0から）
        GameManager.Instance.ResetGame();
        SceneManager.LoadScene(stageSceneName);
    }

    // ── 「タイトルに戻る」ボタンが押されたときの処理 ──────────────
    void OnTitleButtonClicked()
    {
        if (showDebugInfo) Debug.Log("[GameOverManager] タイトルへ");

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClickSE();

        GameManager.Instance.ResetGame();
        SceneManager.LoadScene(titleSceneName);
    }

    // ── 「今回の問題」ボタンが押されたときの処理 ─────────────────
    void OnReviewButtonClicked()
    {
        if (showDebugInfo) Debug.Log("[GameOverManager] 問題一覧へ");

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClickSE();

        // ゲームオーバーの場合も途中までの問題をそのまま表示したいので
        // ResetGameは呼ばずに一覧シーンへ移動する
        SceneManager.LoadScene(reviewSceneName);
    }
}