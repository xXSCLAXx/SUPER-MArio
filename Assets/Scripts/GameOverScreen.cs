using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;


public class GameOverScreen : MonoBehaviour {
    private GameStateManager t_GameStateManager;

    [Header("HUD references")]
    public Text WorldTextHUD;
    public Text ScoreTextHUD;
    public Text CoinTextHUD;

    [Header("Message")]
    public Text MessageText;

    [Header("Final score panel (optional — assign in inspector)")]
    public Text FinalScoreLabel;     // shows "SCORE"
    public Text FinalScoreValue;     // e.g. "012300"
    public Text HighScoreLabel;      // shows "BEST"
    public Text HighScoreValue;      // e.g. "015400"
    public Text NewRecordText;       // "NEW RECORD!" — shown only when score > previous best

    [Header("Audio")]
    public AudioSource gameOverMusicSource;

    private const float MainMenuDelay = 5f; // seconds on the Game Over screen before auto-redirect


    void Start () {
        Time.timeScale = 1;

        t_GameStateManager = FindObjectOfType<GameStateManager>();

        // ── HUD strip at top ──────────────────────────────────────────────
        string worldName = t_GameStateManager.sceneToLoad ?? "World 1-1";
        string[] parts = Regex.Split(worldName, "World ");
        WorldTextHUD.text  = parts.Length > 1 ? parts[1] : worldName;
        ScoreTextHUD.text  = t_GameStateManager.scores.ToString("D6");
        CoinTextHUD.text   = "x" + t_GameStateManager.coins.ToString("D2");

        // ── Final score panel ─────────────────────────────────────────────
        int finalScore   = t_GameStateManager.scores;
        int previousBest = PlayerPrefs.GetInt("highScore", 0);
        bool isNewRecord = finalScore > previousBest;

        // UpdateHighScore is idempotent — safe to call even if already saved
        t_GameStateManager.UpdateHighScore(finalScore);
        int bestScore = PlayerPrefs.GetInt("highScore", 0);

        if (FinalScoreValue != null) FinalScoreValue.text = finalScore.ToString("D6");
        if (HighScoreValue  != null) HighScoreValue.text  = bestScore.ToString("D6");
        if (NewRecordText   != null) NewRecordText.gameObject.SetActive(isNewRecord);

        // ── Main message ──────────────────────────────────────────────────
        bool timeup = t_GameStateManager.timeup;
        if (!timeup) {
            MessageText.text = "GAME OVER";
        } else {
            StartCoroutine(TimeUpThenGameOverCo());
        }

        // ── Music & auto-return ───────────────────────────────────────────
        if (gameOverMusicSource != null) {
            gameOverMusicSource.volume = PlayerPrefs.GetFloat("musicVolume", 1f);
            gameOverMusicSource.Play();
        }

        float musicDuration = (gameOverMusicSource != null && gameOverMusicSource.clip != null)
            ? gameOverMusicSource.clip.length
            : 0f;
        float delay = Mathf.Max(musicDuration, MainMenuDelay);
        StartCoroutine(LoadSceneDelayCo("Main Menu", delay));
    }


    IEnumerator TimeUpThenGameOverCo() {
        MessageText.text = "TIME UP";
        yield return new WaitForSecondsRealtime(1.5f);
        MessageText.text = "GAME OVER";
    }

    IEnumerator LoadSceneDelayCo(string sceneName, float delay) {
        yield return new WaitForSecondsRealtime(delay);
        SceneManager.LoadScene(sceneName);
    }

    void Update() {
        // Pressing Pause/Start skips straight to the main menu
        if (Input.GetButtonDown("Pause")) {
            StopAllCoroutines();
            SceneManager.LoadScene("Main Menu");
        }
    }
}
