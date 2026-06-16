using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// 問題一覧画面を管理するクラス
// 今回出題された問題を四字熟語＋意味の形でスクロール表示する
public class QuizReviewManager : MonoBehaviour
{
    [Header("UI要素")]
    public Transform contentParent;   // ScrollViewの中のContentオブジェクトをセット
    public GameObject quizItemPrefab; // 1問分の表示用Prefabをセット（後述）
    public Button backButton;         // リザルト画面に戻るボタン

    [Header("シーン名設定")]
    // GameClear/GameOverどちらから来たかで戻り先を変える
    public string gameClearSceneName = "GameClearScene";
    public string gameOverSceneName = "GameOverScene";

    [Header("デバッグ")]
    public bool showDebugInfo = true;

    // ── シーンが始まったときに自動で呼ばれる ─────────────────────
    void Start()
    {
        // GameManagerの状態を詳しく確認する
        Debug.Log($"[QuizReviewManager] GameManager Instance: {GameManager.Instance != null}");
        Debug.Log($"[QuizReviewManager] SelectedQuizzes: {GameManager.Instance.SelectedQuizzes != null}");
        Debug.Log($"[QuizReviewManager] SelectedQuizzes.Count: {GameManager.Instance.SelectedQuizzes?.Count ?? -1}");

        if (showDebugInfo) Debug.Log("[QuizReviewManager] Start実行");

        PopulateQuizList();
        backButton.onClick.AddListener(OnBackButtonClicked);
    }

    // ── 問題リストをScrollViewに並べる ───────────────────────────
    void PopulateQuizList()
    {
        var quizzes = GameManager.Instance.SelectedQuizzes;

        if (quizzes == null || quizzes.Count == 0)
        {
            Debug.LogError("[QuizReviewManager] 表示する問題データがありません！");
            return;
        }

        if (showDebugInfo) Debug.Log($"[QuizReviewManager] {quizzes.Count}問を表示");

        float itemHeight = 200f;  // QuizItem1個分の高さ
        float spacing = 10f;      // アイテム同士の間隔
        float currentY = 0f;      // 今どこまで配置したか（上から下に積んでいく）

        int index = 0;

        // Contentの横幅を取得しておく
        RectTransform contentRect = contentParent.GetComponent<RectTransform>();
        float contentWidth = contentRect.rect.width;

        // Contentの横幅がまだ0の場合は親（Viewport）から直接取得する
        if (contentWidth <= 0)
        {
            contentWidth = 1100f;// 仮の固定値（後でInspectorから調整できるようにする）
        }
        foreach (var quiz in quizzes)
        {
            GameObject item = Instantiate(quizItemPrefab, contentParent);

            // ── 座標を直接指定する（Layout Groupに頼らない） ──
            RectTransform itemRect = item.GetComponent<RectTransform>();
            itemRect.anchorMin = new Vector2(0.5f, 1); // 左上を基準にする
            itemRect.anchorMax = new Vector2(0.5f, 1); // 横幅は親に合わせる
            itemRect.pivot = new Vector2(0.5f, 1);
            itemRect.anchoredPosition = new Vector2(0, -currentY); // 上から順にYをずらす
            itemRect.sizeDelta = new Vector2(contentWidth - 20f, itemHeight); // 横は親基準、高さは固定

            Text[] texts = item.GetComponentsInChildren<Text>();
            if (texts.Length >= 2)
            {
                texts[0].text = quiz.full_phrase;
                texts[1].text = quiz.explanation;
            }

            currentY += itemHeight + spacing; // 次のアイテムのY位置を更新
            index++;
        }

        // Contentの高さを全アイテム分に設定
        float totalHeight = currentY - spacing;
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, totalHeight);

        Debug.Log($"[QuizReviewManager] Content Height設定: {totalHeight}");
        Debug.Log($"[QuizReviewManager] 子オブジェクト数: {contentParent.childCount}");
    }

    // ── 「戻る」ボタンが押されたときの処理 ───────────────────────
    void OnBackButtonClicked()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClickSE();

        // ゲームオーバーかクリアかで戻り先を分岐する
        // IsGameOver()がtrueならゲームオーバー画面、falseならクリア画面へ
        if (GameManager.Instance.IsGameOver())
        {
            if (showDebugInfo) Debug.Log("[QuizReviewManager] GameOver画面へ戻る");
            SceneManager.LoadScene(gameOverSceneName);
        }
        else
        {
            if (showDebugInfo) Debug.Log("[QuizReviewManager] GameClear画面へ戻る");
            SceneManager.LoadScene(gameClearSceneName);
        }
    }
}