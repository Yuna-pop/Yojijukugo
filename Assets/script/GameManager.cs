using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ゲーム全体の状態を管理するシングルトン
/// 「シングルトン」とは、ゲーム中にインスタンスが1つだけ存在することを保証するパターン
/// DontDestroyOnLoad によってシーンをまたいでもデータが消えない
/// </summary>
public class GameManager : MonoBehaviour
{
    // staticにすることでクラス名から直接アクセスできる（GameManager._instance）
    // privateで外からは触れないようにする
    private static GameManager _instance;

    // 外部から GameManager.Instance と書くだけでどこからでもアクセスできるプロパティ
    // get {} の中でインスタンスがなければ自動で作る仕組み（遅延初期化）
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // 動的生成をやめてエラーログだけ出す
                _instance = FindAnyObjectByType<GameManager>();
                if (_instance == null)
                {
                    Debug.LogError("[GameManager] GameManagerがシーンに存在しません！Titlesシーンに配置してください。");
                }
            }
            return _instance;
        }
    }

    // ── ゲーム中に保持するデータ（シーンをまたいで保持される）──────
    // 今回のゲームで出題する10問のリスト
    // private set なので外からは読めるが書き換えはできない
    public List<QuizLoader.QuizData> SelectedQuizzes { get; private set; } = new();

    // 今何問目か（0始まり：0=1問目、9=10問目）
    public int CurrentQuestionIndex { get; set; } = 0;

    // ミス回数（プロパティにしてセットのたびにログを出せるようにしている）
    private int _wrongCount = 0;
    public int WrongCount
    {
        get => _wrongCount;
        set
        {
            _wrongCount = value;
            if (showDebugLog)
                Debug.Log($"[GameManager] WrongCount更新: {_wrongCount}/{MAX_WRONG_COUNT}");
        }
    }

    // 直前に解いた問題データ（解説シーンで使う）
    public QuizLoader.QuizData LastQuiz { get; set; }

    // ── 定数（変わらない値はconstで定義するとわかりやすい）──────────
    public const int TOTAL_QUESTIONS = 10;  // 1ゲームの出題数
    public const int MAX_WRONG_COUNT = 3;   // ミス上限（これ以上でゲームオーバー）

    [Header("デバッグ")]
    [Tooltip("デバッグログを表示するか")]
    public bool showDebugLog = true;

    // ── MonoBehaviourのAwakeはStartより先に呼ばれる ──────────────
    void Awake()
    {
        // すでに別のGameManagerがいたら自分を破棄して終わる（重複防止）
        if (_instance != null && _instance != this)
        {
            if (showDebugLog) Debug.Log("[GameManager] 重複インスタンスを破棄");
            Destroy(gameObject);
            return;
        }

        // 自分がインスタンスとして登録される
        _instance = this;

        // このオブジェクトはシーンを移動しても破棄されない
        DontDestroyOnLoad(gameObject);

        if (showDebugLog) Debug.Log("[GameManager] 初期化完了");
    }

    // ── ゲーム開始時にランダムで10問を選ぶ ───────────────────────
    public void InitializeGame(List<QuizLoader.QuizData> allQuizzes)
    {
        // データが10問未満なら動かせないのでエラーを出して止める
        if (allQuizzes == null || allQuizzes.Count < TOTAL_QUESTIONS)
        {
            Debug.LogError($"[GameManager] クイズデータが不足: 必要{TOTAL_QUESTIONS}問、実際{allQuizzes?.Count ?? 0}問");
            return;
        }

        SelectedQuizzes.Clear(); // 前回のデータを消す

        // 元のリストを壊さないようにコピーを作り、そこから1つずつ抜き出す
        List<QuizLoader.QuizData> pool = new(allQuizzes);

        for (int i = 0; i < TOTAL_QUESTIONS && pool.Count > 0; i++)
        {
            // 残っているプールからランダムに1問選ぶ
            int randomIndex = Random.Range(0, pool.Count);
            SelectedQuizzes.Add(pool[randomIndex]);
            // 選んだ問題はプールから削除（同じ問題が2回出ないようにする）
            pool.RemoveAt(randomIndex);
        }

        // 各カウンターを0にリセット
        CurrentQuestionIndex = 0;
        WrongCount = 0;
        LastQuiz = null;

        if (showDebugLog)
            Debug.Log($"[GameManager] ゲーム初期化完了: {SelectedQuizzes.Count}問選択");
    }

    // ── 次の問題へ進む（問題番号を1増やすだけ）─────────────────────
    public void MoveToNextQuestion()
    {
        CurrentQuestionIndex++;
        if (showDebugLog)
            Debug.Log($"[GameManager] 次の問題へ: {CurrentQuestionIndex}/{TOTAL_QUESTIONS}");
    }

    // ── ミスを記録する（WrongCountを1増やすだけ）──────────────────
    public void RecordWrongAnswer()
    {
        WrongCount++;
        if (showDebugLog)
            Debug.Log($"[GameManager] 不正解記録: 残りライフ {MAX_WRONG_COUNT - WrongCount}");
    }

    // ── ゲームオーバー判定 ────────────────────────────────────
    // ミス回数が上限に達したらtrueを返す
    public bool IsGameOver()
    {
        bool result = WrongCount >= MAX_WRONG_COUNT;
        if (showDebugLog)
            Debug.Log($"[GameManager] ゲームオーバー判定: {result} (間違い{WrongCount}回)");
        return result;
    }

    // ── クリア判定 ───────────────────────────────────────────
    // 問題番号が10に達したらtrue（0始まりなので10 = 10問終了）
    public bool IsGameClear()
    {
        bool result = CurrentQuestionIndex >= TOTAL_QUESTIONS;
        if (showDebugLog)
            Debug.Log($"[GameManager] クリア判定: {result} ({CurrentQuestionIndex}/{TOTAL_QUESTIONS})");
        return result;
    }

    // ── ゲームを完全にリセットする ──────────────────────────────
    // タイトルに戻って新しくゲームを始めるときに呼ぶ
    public void ResetGame()
    {
        SelectedQuizzes.Clear();
        CurrentQuestionIndex = 0;
        WrongCount = 0;
        LastQuiz = null;

        // どこから呼ばれたか全部表示する
        Debug.Log($"[GameManager] ゲームリセット完了 呼び出し元: {System.Environment.StackTrace}");
    }

    // ── デバッグ用：現在の状態を文字列で返す ───────────────────────
    public string GetGameStatus()
    {
        return $"問題: {CurrentQuestionIndex + 1}/{TOTAL_QUESTIONS}, " +
               $"残りライフ: {MAX_WRONG_COUNT - WrongCount}/{MAX_WRONG_COUNT}";
    }
}