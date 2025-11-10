using UnityEngine;
using System.Collections.Generic;

public class QuizLoader : MonoBehaviour
{
    [System.Serializable]
    public class QuizData
    {
        public string display;        // 表示用問題文（例: ドハツ◯◯）
        public string correct;         // 正解の漢字
        public string wrong;           // 不正解の漢字
        public string read;            // 読み方
        public string explanation;     // 解説
        public string full_phrase;     // 完全な四字熟語

        public override string ToString()
        {
            return $"{full_phrase} ({read})";
        }
    }

    [Header("クイズデータ")]
    public List<QuizData> quizList = new List<QuizData>();

    [Header("CSV設定")]
    public string csvFileName = "Yojijukugodatacsv";

    void Awake()
    {
        LoadCSV();
    }

    /// <summary>
    /// CSVファイルからクイズデータを読み込む
    /// </summary>
    void LoadCSV()
    {
        // Resourcesフォルダからロード
        TextAsset csv = Resources.Load<TextAsset>(csvFileName);

        if (csv == null)
        {
            Debug.LogError($"CSVファイルが見つかりません: Resources/{csvFileName}");
            return;
        }

        // CSVをパース
        string[] lines = csv.text.Split('\n');

        if (lines.Length <= 1)
        {
            Debug.LogError("CSVファイルが空、またはヘッダーのみです");
            return;
        }

        // ヘッダー行をスキップして処理
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();

            // 空行はスキップ
            if (string.IsNullOrWhiteSpace(line)) continue;

            // カンマで分割
            string[] values = line.Split(',');

            // 列数チェック
            if (values.Length < 8)
            {
                Debug.LogWarning($"行{i + 1}: データが不足しています（{values.Length}列）");
                continue;
            }

            // QuizDataを作成
            QuizData data = new QuizData
            {
                full_phrase = values[2].Trim(),
                correct = values[3].Trim(),
                wrong = values[4].Trim(),
                display = values[5].Trim(),
                read = values[6].Trim(),
                explanation = values[7].Trim()
            };

            quizList.Add(data);
        }

        Debug.Log($"CSVロード完了: {quizList.Count}問のクイズを読み込みました");
    }

    /// <summary>
    /// エディタ用: クイズデータをリロード
    /// </summary>
    [ContextMenu("CSVを再読み込み")]
    void ReloadCSV()
    {
        quizList.Clear();
        LoadCSV();
    }
}