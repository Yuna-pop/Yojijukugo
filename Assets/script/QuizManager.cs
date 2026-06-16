using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// クイズ画面を管理するクラス
// 問題の表示・ボタンの配置・正誤判定・ハートの更新をすべてここで行う
public class QuizManager : MonoBehaviour
{
    // ── インスペクターで設定するUI部品 ──────────────────────────
    [Header("UI要素")]
    public Text displayText;       // 問題文（例：「イン◯◯ジチョウ」）を表示するテキスト
    public Button button1;         // 選択肢ボタン1
    public Button button2;         // 選択肢ボタン2
    public Image[] heartImages;    // ハート画像3つ。配列なのでループで一括処理できる

    [Header("デバッグ")]
    public bool showDebugInfo = true;

    // このシーンで表示中の問題データを一時保存しておく変数
    // privateなので外から直接触れない（このクラス内だけで使う）
    private QuizLoader.QuizData currentQuiz;

    // ── シーンが始まったときに自動で呼ばれる ─────────────────────
    void Start()
    {
        // BGMを再生する
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameBGM();
            if (showDebugInfo) Debug.Log("[QuizManager] ゲームBGM再生");
        }
        else
        {
            Debug.LogError("[QuizManager] AudioManagerが見つかりません！");
        }

        // QuizLoader（CSVからデータを読む係）をシーン内から探す
        var loader = FindObjectOfType<QuizLoader>();
        if (loader == null)
        {
            Debug.LogError("[QuizManager] QuizLoaderが見つかりません！");
            return;
        }

        // SelectedQuizzesが空のとき（＝ゲーム開始直後）だけ初期化する
        // シーンをまたいでも GameManager はデータを持ち続けるので
        // 2問目以降にStageシーンに戻ってきたときはここをスキップする
        if (GameManager.Instance.SelectedQuizzes.Count == 0)
        {
            GameManager.Instance.InitializeGame(loader.quizList);
        }

        // 現在のライフ状況に合わせてハートを更新する
        UpdateHearts();

        // 現在の問題番号に対応する問題を画面に表示する
        ShowQuestion();

        if (showDebugInfo)
        {
            Debug.Log($"[QuizManager] {GameManager.Instance.GetGameStatus()}");
        }
    }

    // ── 問題を画面に表示する ──────────────────────────────────
    void ShowQuestion()
    {
        int index = GameManager.Instance.CurrentQuestionIndex;

        // インデックスが有効な範囲内かチェック（範囲外だと配列エラーになる）
        if (index >= 0 && index < GameManager.Instance.SelectedQuizzes.Count)
        {
            // 現在の問題をリストから取り出す
            currentQuiz = GameManager.Instance.SelectedQuizzes[index];

            // 問題文を表示
            displayText.text = currentQuiz.display;

            // ── 正解・不正解をボタンにランダム配置 ──
            // Random.value は0.0〜1.0のランダムな小数を返す
            // 0.5より小さい確率は50%なので、ボタン1が正解になる確率も50%
            bool isButton1Correct = Random.value < 0.5f;

            // ボタンの中にある Text コンポーネントを取得する
            Text button1Text = button1.GetComponentInChildren<Text>();
            Text button2Text = button2.GetComponentInChildren<Text>();

            if (isButton1Correct)
            {
                button1Text.text = currentQuiz.correct; // ボタン1に正解を配置
                button2Text.text = currentQuiz.wrong;   // ボタン2に不正解を配置
            }
            else
            {
                button1Text.text = currentQuiz.wrong;   // ボタン1に不正解を配置
                button2Text.text = currentQuiz.correct; // ボタン2に正解を配置
            }

            // ボタンのクリック処理を設定する
            SetupButtons();
        }
        else
        {
            Debug.LogError($"[QuizManager] インデックスが範囲外: {index}");
        }
    }

    // ── ボタンのクリックイベントを登録する ───────────────────────
    void SetupButtons()
    {
        // 前回のシーンで登録したリスナーが残っていると二重実行されるので先に全削除
        button1.onClick.RemoveAllListeners();
        button2.onClick.RemoveAllListeners();

        // ラムダ式（=> で書く短い関数）でボタンごとの処理を登録
        // 「() =>」は「引数なしの無名関数」という意味
        button1.onClick.AddListener(() =>
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayButtonClickSE();
            OnAnswerSelected(button1); // 押されたボタンを引数として渡す
        });
        button2.onClick.AddListener(() =>
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayButtonClickSE();
            OnAnswerSelected(button2);
        });
    }

    // ── 選択肢ボタンが押されたときの処理 ─────────────────────────
    void OnAnswerSelected(Button selectedButton)
    {
        // 連打防止：両方のボタンをまとめて押せない状態にする
        button1.interactable = false;
        button2.interactable = false;

        // 押されたボタンのテキストを取得して正解と照合する
        string selectedAnswer = selectedButton.GetComponentInChildren<Text>().text;
        bool isCorrect = selectedAnswer == currentQuiz.correct;

        if (isCorrect)
        {
            // 正解だった場合：結果をPlayerPrefsに保存（ResultManagerが読みに来る）
            PlayerPrefs.SetString("LastResult", "correct");
            if (showDebugInfo) Debug.Log("[QuizManager] 正解！");
        }
        else
        {
            // 不正解だった場合：GameManagerにミスを記録してハートを減らす
            GameManager.Instance.RecordWrongAnswer();
            UpdateHearts(); // ハート表示をすぐ更新する
            PlayerPrefs.SetString("LastResult", "wrong");
            if (showDebugInfo)
                Debug.Log($"[QuizManager] 不正解… 残りライフ: {GameManager.MAX_WRONG_COUNT - GameManager.Instance.WrongCount}");
        }

        // 解説シーンで「どの問題を解説するか」をGameManagerに渡しておく
        GameManager.Instance.LastQuiz = currentQuiz;

        // ────────────────────────────────────────────────────
        // ✅ 問題番号をここで1回だけ進める
        // ExplanationManagerでは進めないので、必ずここで呼ぶこと
        // ────────────────────────────────────────────────────
        GameManager.Instance.MoveToNextQuestion();

        Debug.Log($"[QuizManager] ResultScene遷移直前 Count: {GameManager.Instance.SelectedQuizzes.Count}");

        // 正解・不正解どちらも ResultScene（正解！/残念…画面）を経由する
        // ゲームクリアの場合もResultSceneを見せてからExplanationSceneへ進む
        SceneManager.LoadScene("ResultScene");
    }

    // ── ハートの表示を更新する ────────────────────────────────
    void UpdateHearts()
    {
        // 残りライフを計算する（最大ライフ - 現在のミス回数）
        int remainingHearts = GameManager.MAX_WRONG_COUNT - GameManager.Instance.WrongCount;

        // heartImages配列をループして1つずつ色を変える
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (i < remainingHearts)
            {
                // 残機あり：白色（= 画像本来の色がそのまま表示される）
                heartImages[i].color = new Color(1f, 1f, 1f, 1f);
            }
            else
            {
                // 残機なし：グレーにして「失った」ことを表現する
                // new Color(赤, 緑, 青, 透明度) で色を指定（0〜1の範囲）
                heartImages[i].color = new Color(0.5f, 0.5f, 0.5f, 1f);
            }
        }

        if (showDebugInfo)
        {
            Debug.Log($"[QuizManager] ハート更新: {remainingHearts}/{GameManager.MAX_WRONG_COUNT}");
        }
    }
}