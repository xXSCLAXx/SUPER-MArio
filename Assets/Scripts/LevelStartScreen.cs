using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;


public class LevelStartScreen : MonoBehaviour {
    private GameStateManager t_GameStateManager;
    private const float LoadScreenDelay = 2f;

    public Text WorldTextHUD;
    public Text ScoreTextHUD;
    public Text CoinTextHUD;
    public Text WorldTextMain;
    public Text livesText;

    void Start () {
        Time.timeScale = 1;

        t_GameStateManager = FindObjectOfType<GameStateManager>();
        string sceneName = t_GameStateManager.sceneToLoad;

        // Extract everything after "World " for the HUD world indicator
        string[] parts = Regex.Split(sceneName, "World ");
        string worldCode = parts.Length > 1 ? parts[1] : sceneName;

        WorldTextHUD.text  = worldCode;
        ScoreTextHUD.text  = t_GameStateManager.scores.ToString("D6");
        CoinTextHUD.text   = "x" + t_GameStateManager.coins.ToString("D2");
        WorldTextMain.text = sceneName.ToUpper();
        livesText.text     = t_GameStateManager.lives.ToString();

        StartCoroutine(LoadSceneDelayCo(sceneName, LoadScreenDelay));
    }

    IEnumerator LoadSceneDelayCo(string sceneName, float delay) {
        yield return new WaitForSecondsRealtime(delay);
        SceneManager.LoadScene(sceneName);
    }
}
