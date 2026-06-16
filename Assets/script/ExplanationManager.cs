using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// 解説シーンを管理するクラス
// 問題に答えた後、その四字熟語の読み・意味を表示する
public class ExplanationManager : MonoBehaviour
{
    [Header("UI要素")]
    public Text displayText;      // 四字熟語本体を表示するテキスト
    public Text readText;         // 読み仮名を表示するテキスト
    public Text explanationText;  // 意味・解説を表示するテキスト
    public Button nextButton;     // 「次の問題へ」ボタン
    public Image[] heartImages;   // ①ハート画像3つ。Inspectorでドラッグして設定する

    [Header("デバッグ")]
    public bool showDebugInfo = true;

    // ── シーンが始まったときに自動で呼ばれる ─────────────────────
    void Start()
    {
        if (showDebugInfo) Debug.Log("[ExplanationManager] Start実行");

        // GameManagerに保存してある「直前に解いた問題」を取り出す
        var quiz = GameManager.Instance.LastQuiz;

        // 万が一データがなければエラーを表示して処理を止める
        if (quiz == null)
        {
            Debug.LogError("[ExplanationManager] 解説する問題が見つかりません！");
            displayText.text = "エラー：問題情報を取得できませんでした";
            return; // returnで以降の処理をスキップ
        }

        // ①現在の残機に合わせてハートの色を更新する
        UpdateHearts();

        // 問題データを画面に表示する
        DisplayExplanation(quiz);

        // ボタンが押されたときに呼ぶ関数を登録する
        nextButton.onClick.AddListener(OnNextButtonClicked);

        // 解説シーン用のBGMを再生
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayExplanationBGM();
        }
    }

    // ── ①ハートの色を残機に合わせて更新する ─────────────────────
    void UpdateHearts()
    {
        // heartImagesがInspectorで設定されていない場合はスキップ（エラー防止）
        if (heartImages == null || heartImages.Length == 0) return;

        // 残りライフを計算する
        int remainingHearts = GameManager.MAX_WRONG_COUNT - GameManager.Instance.WrongCount;

        for (int i = 0; i < heartImages.Length; i++)
        {
            if (i < remainingHearts)
            {
                // 残機あり：白色（画像本来の色がそのまま表示される）
                heartImages[i].color = new Color(1f, 1f, 1f, 1f);
            }
            else
            {
                // 残機なし：グレーにして「失った」ことを表現する
                heartImages[i].color = new Color(0.5f, 0.5f, 0.5f, 1f);
            }
        }
    }

    // ── 解説を画面に表示する ──────────────────────────────────
    // QuizDataを受け取って、各テキストにセットするだけのシンプルな関数
    void DisplayExplanation(QuizLoader.QuizData quiz)
    {
        displayText.text = quiz.full_phrase;         // 例：「隠忍自重」
        readText.text = $"読み: {quiz.read}";        // 例：「読み: インニンジチョウ」
        explanationText.text = quiz.explanation;     // 意味の文章

        if (showDebugInfo)
        {
            Debug.Log($"[ExplanationManager] 解説表示: {quiz.full_phrase}");
        }
    }

    // ── 「次へ」ボタンが押されたときの処理 ───────────────────────
    void OnNextButtonClicked()
    {
        // 連打防止：ボタンを押せない状態にする
        nextButton.interactable = false;

        // ボタンクリック音を鳴らす
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClickSE();
        }

        // ────────────────────────────────────────────────────
        // ⚠️ ここでは MoveToNextQuestion() を呼ばない！
        // 問題番号を進める処理はQuizManagerで既に済んでいるため、
        // ここで呼ぶと1問ごとに2回進んでしまい10問以上出題されるバグになる
        // ────────────────────────────────────────────────────

        // ゲームオーバー判定（3回間違えたか？）
        if (GameManager.Instance.IsGameOver())
        {
            if (showDebugInfo) Debug.Log("[ExplanationManager] ゲームオーバー（3回間違い）");
            SceneManager.LoadScene("GameOverScene");
        }
        // クリア判定（10問全部終わったか？）
        else if (GameManager.Instance.IsGameClear())
        {
            Debug.Log($"[ExplanationManager] GameClearScene遷移直前 Count: {GameManager.Instance.SelectedQuizzes.Count}");
            if (showDebugInfo) Debug.Log("[ExplanationManager] ゲームクリア（10問完了）");
            SceneManager.LoadScene("GameClearScene");
        }
        // まだ続く → 次の問題シーンへ
        else
        {
            if (showDebugInfo) Debug.Log($"[ExplanationManager] 次の問題へ: {GameManager.Instance.GetGameStatus()}");
            SceneManager.LoadScene("Stage");
        }
    }
}