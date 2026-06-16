using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// 結果シーンを管理するクラス
// 「正解！」または「残念…」を表示して、解説シーンへ誘導する
public class ResultManager : MonoBehaviour
{
    [Header("UI要素")]
    public Text resultText;    // 「正解！」「残念…」を表示するテキスト
    public Button nextButton;  // 解説へ進むボタン
    public Image[] heartImages; // ①ハート画像3つ。Inspectorでドラッグして設定する

    [Header("表示設定")]
    // Inspectorから文言を変えられるよう変数にしている
    public string correctMessage = "正解！";
    public string wrongMessage = "残念…";

    [Header("デバッグ")]
    public bool showDebugInfo = true;

    // ── シーンが始まったときに自動で呼ばれる ─────────────────────
    void Start()
    {
        // クイズ中と同じBGMを継続再生（途切れさせない）
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameBGM();
            if (showDebugInfo) Debug.Log("[ResultManager] ゲームBGM継続");
        }
        else
        {
            Debug.LogError("[ResultManager] AudioManagerが見つかりません！");
        }

        // ①現在の残機に合わせてハートの色を更新する
        UpdateHearts();

        // PlayerPrefsに保存しておいた正誤結果を読み出す
        // GetStringの第2引数は「キーがなかった場合のデフォルト値」
        string result = PlayerPrefs.GetString("LastResult", "correct");
        bool isCorrect = (result == "correct");

        // 正誤に合わせてテキストを切り替える
        DisplayResult(isCorrect);

        // 正解・不正解に対応したSEを鳴らす
        if (AudioManager.Instance != null)
        {
            if (isCorrect)
            {
                AudioManager.Instance.PlayCorrectSE();
                if (showDebugInfo) Debug.Log("[ResultManager] 正解SE再生");
            }
            else
            {
                AudioManager.Instance.PlayWrongSE();
                if (showDebugInfo) Debug.Log("[ResultManager] 不正解SE再生");
            }
        }

        // ボタンが押されたときに呼ぶ関数を登録する
        nextButton.onClick.AddListener(OnNextButtonClicked);
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

    // ── 正誤テキストを切り替える ──────────────────────────────
    void DisplayResult(bool isCorrect)
    {
        // 三項演算子：条件 ? trueの値 : falseの値
        // isCorrectがtrueなら correctMessage、falseなら wrongMessage をセット
        resultText.text = isCorrect ? correctMessage : wrongMessage;
    }

    // ── 「次へ」ボタンが押されたときの処理 ───────────────────────
    void OnNextButtonClicked()
    {
        // 連打防止
        nextButton.interactable = false;

        // ページめくりSEを鳴らす
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPageFlipSE();
        }

        // 解説シーンへ移動する
        // 「次の問題に進むかどうか」の判断はExplanationManagerに任せる
        SceneManager.LoadScene("ExplanationScene");
    }
}