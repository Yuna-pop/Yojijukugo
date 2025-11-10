using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ゲーム全体の状態を管理するシングルトン
/// シーン間でデータを永続化します
/// </summary>
public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    [System.Obsolete]
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // シーン内の既存GameManagerを探す
                _instance = FindObjectOfType<GameManager>();

                // 見つからなければ新規作成
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameManager");
                    _instance = go.AddComponent<GameManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    // ゲーム状態（シーン間で保持される）
    public List<QuizLoader.QuizData> SelectedQuizzes { get; private set; } = new();
    public int CurrentQuestionIndex { get; set; } = 0;

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

    public QuizLoader.QuizData LastQuiz { get; set; }

    // 定数
    public const int TOTAL_QUESTIONS = 10;
    public const int MAX_WRONG_COUNT = 3;

    // デバッグ設定
    [Header("デバッグ")]
    [Tooltip("デバッグログを表示するか")]
    public bool showDebugLog = true;

    void Awake()
    {
        // 既にインスタンスが存在する場合は破棄
        if (_instance != null && _instance != this)
        {
            if (showDebugLog) Debug.Log("[GameManager] 重複インスタンスを破棄");
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        if (showDebugLog) Debug.Log("[GameManager] 初期化完了");
    }

    /// <summary>
    /// ゲームを初期化（新規ゲーム開始時）
    /// </summary>
    public void InitializeGame(List<QuizLoader.QuizData> allQuizzes)
    {
        if (allQuizzes == null || allQuizzes.Count < TOTAL_QUESTIONS)
        {
            Debug.LogError($"[GameManager] クイズデータが不足: 必要{TOTAL_QUESTIONS}問、実際{allQuizzes?.Count ?? 0}問");
            return;
        }

        // ランダムに10問選択（重複なし）
        SelectedQuizzes.Clear();
        List<QuizLoader.QuizData> pool = new(allQuizzes);

        for (int i = 0; i < TOTAL_QUESTIONS && pool.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, pool.Count);
            SelectedQuizzes.Add(pool[randomIndex]);
            pool.RemoveAt(randomIndex);
        }

        // ゲーム状態をリセット
        CurrentQuestionIndex = 0;
        WrongCount = 0;
        LastQuiz = null;

        if (showDebugLog)
            Debug.Log($"[GameManager] ゲーム初期化完了: {SelectedQuizzes.Count}問選択、ライフ: {MAX_WRONG_COUNT}");
    }

    /// <summary>
    /// 次の問題に進む
    /// </summary>
    public void MoveToNextQuestion()
    {
        CurrentQuestionIndex++;
        if (showDebugLog)
            Debug.Log($"[GameManager] 次の問題へ: {CurrentQuestionIndex + 1}/{TOTAL_QUESTIONS}");
    }

    /// <summary>
    /// 不正解を記録
    /// </summary>
    public void RecordWrongAnswer()
    {
        WrongCount++;
        if (showDebugLog)
            Debug.Log($"[GameManager] 不正解記録: 残りライフ {MAX_WRONG_COUNT - WrongCount}");
    }

    /// <summary>
    /// ゲームオーバー判定（3回間違えたか？）
    /// </summary>
    public bool IsGameOver()
    {
        bool result = WrongCount >= MAX_WRONG_COUNT;
        if (showDebugLog)
            Debug.Log($"[GameManager] ゲームオーバー判定: {result} (間違い{WrongCount}回)");
        return result;
    }

    /// <summary>
    /// クリア判定（10問終了したか？）
    /// </summary>
    public bool IsGameClear()
    {
        bool result = CurrentQuestionIndex >= TOTAL_QUESTIONS;
        if (showDebugLog)
            Debug.Log($"[GameManager] クリア判定: {result} (問題{CurrentQuestionIndex}/{TOTAL_QUESTIONS})");
        return result;
    }

    /// <summary>
    /// ゲームをリセット
    /// </summary>
    public void ResetGame()
    {
        SelectedQuizzes.Clear();
        CurrentQuestionIndex = 0;
        WrongCount = 0;
        LastQuiz = null;
        if (showDebugLog)
            Debug.Log("[GameManager] ゲームリセット完了");
    }

    /// <summary>
    /// 現在の状態を文字列で取得
    /// </summary>
    public string GetGameStatus()
    {
        return $"問題: {CurrentQuestionIndex + 1}/{TOTAL_QUESTIONS}, " +
               $"残りライフ: {MAX_WRONG_COUNT - WrongCount}/{MAX_WRONG_COUNT}";
    }
}