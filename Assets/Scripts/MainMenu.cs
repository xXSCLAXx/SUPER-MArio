using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class MainMenu : MonoBehaviour {
    private GameStateManager t_GameStateManager;
    public Text TopText;

    public GameObject VolumePanel;
    public GameObject SoundSlider;
    public GameObject MusicSlider;

    public bool volumePanelActive;

    // Cached slider component references — avoids repeated GetComponent calls
    private Slider soundSliderComponent;
    private Slider musicSliderComponent;

    void Start () {
        t_GameStateManager = FindObjectOfType<GameStateManager> ();
        t_GameStateManager.ConfigNewGame ();

        soundSliderComponent = SoundSlider.GetComponent<Slider> ();
        musicSliderComponent = MusicSlider.GetComponent<Slider> ();

        // Initialise PlayerPrefs keys on first run
        if (!PlayerPrefs.HasKey ("soundVolume")) PlayerPrefs.SetFloat ("soundVolume", 1f);
        if (!PlayerPrefs.HasKey ("musicVolume")) PlayerPrefs.SetFloat ("musicVolume", 1f);

        soundSliderComponent.value = PlayerPrefs.GetFloat ("soundVolume");
        musicSliderComponent.value = PlayerPrefs.GetFloat ("musicVolume");

        int currentHighScore = PlayerPrefs.GetInt ("highScore", 0);
        TopText.text = "TOP- " + currentHighScore.ToString ("D6");
    }

    public void OnMouseHover(Button button) {
        if (!volumePanelActive) {
            button.transform.Find ("Cursor").gameObject.SetActive (true);
        }
    }

    public void OnMouseHoverExit(Button button) {
        if (!volumePanelActive) {
            button.transform.Find ("Cursor").gameObject.SetActive (false);
        }
    }

    // ── World 1 ──────────────────────────────────────────────────────────
    public void StartNewGame() {
        if (!volumePanelActive) {
            t_GameStateManager.sceneToLoad = "World 1-1";
            SceneManager.LoadScene ("Level Start Screen");
        }
    }

    public void StartWorld1_2() {
        if (!volumePanelActive) {
            t_GameStateManager.sceneToLoad = "World 1-2";
            SceneManager.LoadScene ("Level Start Screen");
        }
    }

    public void StartWorld1_3() {
        if (!volumePanelActive) {
            t_GameStateManager.sceneToLoad = "World 1-3";
            SceneManager.LoadScene ("Level Start Screen");
        }
    }

    public void StartWorld1_4() {
        if (!volumePanelActive) {
            t_GameStateManager.sceneToLoad = "World 1-4";
            SceneManager.LoadScene ("Level Start Screen");
        }
    }

    // ── World 2 (nuevos niveles) ─────────────────────────────────────────
    /// Nivel subterráneo — réplica del estilo clásico SMB underground
    public void StartWorld2_Underground() {
        if (!volumePanelActive) {
            t_GameStateManager.sceneToLoad = "World 2-1 - Underground";
            SceneManager.LoadScene ("Level Start Screen");
        }
    }

    /// Nivel acuático — réplica del estilo clásico SMB underwater
    public void StartWorld2_Aquatic() {
        if (!volumePanelActive) {
            t_GameStateManager.sceneToLoad = "World 2-2 - Aquatic";
            SceneManager.LoadScene ("Level Start Screen");
        }
    }

    /// Castillo de Bowser — diseño original, no réplica
    public void StartWorld2_BowserCastle() {
        if (!volumePanelActive) {
            t_GameStateManager.sceneToLoad = "World 2-4 - Bowser Castle";
            SceneManager.LoadScene ("Level Start Screen");
        }
    }

    // ── Settings ─────────────────────────────────────────────────────────
    public void QuitGame() {
        if (!volumePanelActive) {
            Application.Quit ();
        }
    }

    public void SelectVolume() {
        VolumePanel.SetActive (true);
        volumePanelActive = true;
    }

    public void SetVolume() {
        PlayerPrefs.SetFloat ("soundVolume", soundSliderComponent.value);
        PlayerPrefs.SetFloat ("musicVolume", musicSliderComponent.value);
        VolumePanel.SetActive (false);
        volumePanelActive = false;
    }

    public void CancelSelectVolume() {
        soundSliderComponent.value = PlayerPrefs.GetFloat ("soundVolume");
        musicSliderComponent.value = PlayerPrefs.GetFloat ("musicVolume");
        VolumePanel.SetActive (false);
        volumePanelActive = false;
    }
}
