using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class QuizManager : MonoBehaviour
{
    [Header("UI要素")]
    public Text displayText;
    public Button button1;
    public Button button2;
    public Image[] heartImages; // 3つのハート画像を配置

    [Header("デバッグ")]
    public bool showDebugInfo = true;

    private QuizLoader.QuizData currentQuiz;

    void Start()
    {
        // 🎵 重要: ゲームBGMを再生
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameBGM();
            if (showDebugInfo) Debug.Log("[QuizManager] ゲームBGM再生");
        }
        else
        {
            Debug.LogError("[QuizManager] AudioManagerが見つかりません！");
        }

        // QuizLoaderが存在するか確認
        var loader = FindObjectOfType<QuizLoader>();
        if (loader == null)
        {
            Debug.LogError("[QuizManager] QuizLoaderが見つかりません！");
            return;
        }

        // 初回のゲーム開始時のみクイズを選択
        if (GameManager.Instance.SelectedQuizzes.Count == 0)
        {
            GameManager.Instance.InitializeGame(loader.quizList);
        }

        // シーン開始時にハートを更新（ライフが保持される）
        UpdateHearts();

        // 現在の問題を表示
        ShowQuestion();

        if (showDebugInfo)
        {
            Debug.Log($"[QuizManager] {GameManager.Instance.GetGameStatus()}");
        }
    }

    /// <summary>
    /// 問題を画面に表示
    /// </summary>
    void ShowQuestion()
    {
        if (GameManager.Instance.SelectedQuizzes.Count > GameManager.Instance.CurrentQuestionIndex && GameManager.Instance.CurrentQuestionIndex >= 0)
        {
            // 現在の問題を取得
            currentQuiz = GameManager.Instance.SelectedQuizzes[GameManager.Instance.CurrentQuestionIndex];

            // 問題文を表示（例: ドハツ◯◯）
            displayText.text = currentQuiz.display;

            // ボタンをランダムに配置
            bool isButton1Correct = Random.value < 0.5f;

            Text button1Text = button1.GetComponentInChildren<Text>();
            Text button2Text = button2.GetComponentInChildren<Text>();

            if (isButton1Correct)
            {
                button1Text.text = currentQuiz.correct;
                button2Text.text = currentQuiz.wrong;
            }
            else
            {
                button1Text.text = currentQuiz.wrong;
                button2Text.text = currentQuiz.correct;
            }

            // ボタンイベントを設定
            SetupButtons();
        }
        else
        {
            Debug.LogError("インデックスが範囲外です");
        }
    }

    /// <summary>
    /// ボタンのクリックイベントを設定
    /// </summary>
    void SetupButtons()
    {
        button1.onClick.RemoveAllListeners();
        button2.onClick.RemoveAllListeners();

        // ボタンクリック時にSEを再生
        button1.onClick.AddListener(() =>
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayButtonClickSE();
            OnAnswerSelected(button1);
        });
        button2.onClick.AddListener(() =>
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayButtonClickSE();
            OnAnswerSelected(button2);
        });
    }

    /// <summary>
    /// 答えが選択された時の処理
    /// </summary>
    void OnAnswerSelected(Button selectedButton)
    {
        // 連打防止
        button1.interactable = false;
        button2.interactable = false;

        string selectedAnswer = selectedButton.GetComponentInChildren<Text>().text;
        bool isCorrect = selectedAnswer == currentQuiz.correct;

        if (isCorrect)
        {
            PlayerPrefs.SetString("LastResult", "correct");
            if (showDebugInfo) Debug.Log("[QuizManager] 正解！");
        }
        else
        {
            // GameManagerで不正解を記録（これでライフが減る）
            GameManager.Instance.RecordWrongAnswer();

            // ハート表示を即座に更新
            UpdateHearts();

            PlayerPrefs.SetString("LastResult", "wrong");
            if (showDebugInfo)
                Debug.Log($"[QuizManager] 不正解… 残りライフ: {GameManager.MAX_WRONG_COUNT - GameManager.Instance.WrongCount}");
        }

        // 解説シーンで使用するため、現在の問題を保存
        GameManager.Instance.LastQuiz = currentQuiz;

        // 問題番号を進める
        GameManager.Instance.MoveToNextQuestion();

        // クリア判定
        if (GameManager.Instance.IsGameClear())
        {
            SceneManager.LoadScene("GameClearScene");
        }
        else
        {
            // 結果シーンへ遷移
            SceneManager.LoadScene("ResultScene");
        }
    }

    /// <summary>
    /// ハート表示を更新（GameManagerの状態を反映）
    /// </summary>
    void UpdateHearts()
    {
        int remainingHearts = GameManager.MAX_WRONG_COUNT - GameManager.Instance.WrongCount;

        for (int i = 0; i < heartImages.Length; i++)
        {
            if (i < remainingHearts)
            {
                // 残っているハート（白色）
                heartImages[i].color = Color.white;
            }
            else
            {
                // 失ったハート（グレーアウト）
                heartImages[i].color = new Color(0.5f, 0.5f, 0.5f, 1f);
            }
        }

        if (showDebugInfo)
        {
            Debug.Log($"[QuizManager] ハート更新: {remainingHearts}/{GameManager.MAX_WRONG_COUNT}");
        }
    }
}