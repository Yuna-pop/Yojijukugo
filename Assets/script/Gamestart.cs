using UnityEngine;
using UnityEngine.SceneManagement;

// タイトル画面を管理するクラス
// 「ゲーム開始」ボタンが押されたときの処理を担当する
public class Gamestart : MonoBehaviour
{
    [Header("設定")]
    public string stageSceneName = "Stage"; // 遷移先のシーン名（Inspectorで変更可能）

    [Header("デバッグ")]
    public bool testBGMOnStart = true; // trueにするとタイトル表示時にBGMが流れる

    // 画面遷移中かどうかのフラグ（連打防止に使う）
    private bool isTransitioning = false;

    // ── シーンが始まったときに自動で呼ばれる ─────────────────────
    void Start()
    {
        Debug.Log("[GameStart] Start() 実行");

        // タイトルBGMを再生する
        if (AudioManager.Instance != null)
        {
            if (testBGMOnStart)
            {
                AudioManager.Instance.PlayTitleBGM();
            }
        }
        else
        {
            Debug.LogError("[GameStart] AudioManagerインスタンスがnull！");
        }
    }

    // ── 「ゲーム開始」ボタンが押されたときの処理 ──────────────────
    // Inspectorのボタンの OnClick() にこのメソッドを登録して使う
    public void PressStart()
    {
        Debug.Log("[GameStart] PressStart() 呼び出し");

        // 遷移中に何度も呼ばれないよう、フラグで弾く
        if (isTransitioning) return;
        isTransitioning = true;

        // ボタンクリック音を鳴らす
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClickSE();

        // 前回のゲームデータが残っていたら全部リセットする
        // （2週目プレイや「最初からやり直す」のあとでも必ずきれいな状態で始まる）
        GameManager.Instance.ResetGame();

        Debug.Log($"[GameStart] シーン遷移: {stageSceneName}");

        // ステージ（クイズ）シーンへ移動する
        SceneManager.LoadScene(stageSceneName);
    }

    // ── インスペクターの右クリックメニューから呼べるテスト用関数 ────
    [ContextMenu("BGM再生テスト")]
    void TestPlayBGM()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayTitleBGM();
        else
            Debug.LogError("AudioManagerが見つかりません");
    }
}