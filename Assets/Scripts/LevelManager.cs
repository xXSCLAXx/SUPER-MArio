using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;


public class LevelManager : MonoBehaviour {
	private const float LoadSceneDelay = 1f;

	public bool hurryUp;
	public int marioSize; // 0=small, 1=big, 2=fire
	public int lives;
	public int coins;
	public int scores;
	public float timeLeft;
	private int timeLeftInt;

	private bool isRespawning;
	private bool isPoweringDown;

	public bool isInvinciblePowerdown;
	public bool isInvincibleStarman;
	private const float MarioInvinciblePowerdownDuration = 2f;
	private const float MarioInvincibleStarmanDuration = 12f;
	private const float TransformDuration = 1f;

	private GameStateManager t_GameStateManager;
	private Mario mario;
	private Animator mario_Animator;
	private Rigidbody2D mario_Rigidbody2D;

	public Text scoreText;
	public Text coinText;
	public Text timeText;
	public GameObject FloatingTextEffect;
	private const float FloatingTextOffsetY = 2f;

	public AudioSource musicSource;
	public AudioSource soundSource;
	public AudioSource pauseSoundSource;

	public AudioClip levelMusic;
	public AudioClip levelMusicHurry;
	public AudioClip starmanMusic;
	public AudioClip starmanMusicHurry;
	public AudioClip levelCompleteMusic;
	public AudioClip castleCompleteMusic;

	public AudioClip oneUpSound;
	public AudioClip bowserFallSound;
	public AudioClip bowserFireSound;
	public AudioClip breakBlockSound;
	public AudioClip bumpSound;
	public AudioClip coinSound;
	public AudioClip deadSound;
	public AudioClip fireballSound;
	public AudioClip flagpoleSound;
	public AudioClip jumpSmallSound;
	public AudioClip jumpSuperSound;
	public AudioClip kickSound;
	public AudioClip pipePowerdownSound;
	public AudioClip powerupSound;
	public AudioClip powerupAppearSound;
	public AudioClip stompSound;
	public AudioClip warningSound;

	public int coinBonus      = 200;
	public int powerupBonus   = 1000;
	public int starmanBonus   = 1000;
	public int oneupBonus     = 0;
	public int breakBlockBonus = 50;

	// Aerial stomp combo: each consecutive stomp before landing gives increasing points.
	// After 8 stomps the player earns a 1-UP, matching original SMB behaviour.
	private int stompCombo = 0;
	private static readonly int[] StompComboBonuses = { 100, 200, 400, 800, 1000, 2000, 4000, 8000 };

	public Vector2 stompBounceVelocity = new Vector2 (0, 15);

	public bool gamePaused;
	public bool timerPaused;
	public bool musicPaused;


	void Awake() {
		Time.timeScale = 1;
		// Cache early so FindSpawnPosition works even when called from other Start()s
		t_GameStateManager = FindObjectOfType<GameStateManager>();
	}

	void Start () {
		RetrieveGameState ();

		mario = FindObjectOfType<Mario> ();
		mario_Animator = mario.gameObject.GetComponent<Animator> ();
		mario_Rigidbody2D = mario.gameObject.GetComponent<Rigidbody2D> ();
		mario.UpdateSize ();

		musicSource.volume = PlayerPrefs.GetFloat("musicVolume", 1f);
		soundSource.volume = PlayerPrefs.GetFloat("soundVolume", 1f);
		pauseSoundSource.volume = PlayerPrefs.GetFloat("soundVolume", 1f);

		SetHudCoin ();
		SetHudScore ();
		SetHudTime ();
		ChangeMusic (hurryUp ? levelMusicHurry : levelMusic);
	}

	void RetrieveGameState() {
		marioSize = t_GameStateManager.marioSize;
		lives     = t_GameStateManager.lives;
		coins     = t_GameStateManager.coins;
		scores    = t_GameStateManager.scores;
		timeLeft  = t_GameStateManager.timeLeft;
		hurryUp   = t_GameStateManager.hurryUp;
	}


	/****************** Timer */
	void Update() {
		if (!timerPaused) {
			timeLeft -= Time.deltaTime / .4f; // 1 game-sec ≈ 0.4 real-time sec
			SetHudTime ();
		}

		if (timeLeftInt < 100 && !hurryUp) {
			hurryUp = true;
			PauseMusicPlaySound (warningSound, true);
			ChangeMusic (isInvincibleStarman ? starmanMusicHurry : levelMusicHurry, warningSound.length);
		}

		if (timeLeftInt <= 0) {
			MarioRespawn (true);
		}

		if (Input.GetButtonDown ("Pause")) {
			if (!gamePaused) {
				StartCoroutine (PauseGameCo ());
			} else {
				StartCoroutine (UnpauseGameCo ());
			}
		}
	}


	/****************** Game pause */
	List<Animator> unscaledAnimators = new List<Animator> ();
	float pauseGamePrevTimeScale;
	bool pausePrevMusicPaused;

	IEnumerator PauseGameCo() {
		gamePaused = true;
		pauseGamePrevTimeScale = Time.timeScale;

		Time.timeScale = 0;
		pausePrevMusicPaused = musicPaused;
		musicSource.Pause ();
		musicPaused = true;
		soundSource.Pause ();

		// Animators running in UnscaledTime would still advance while paused — switch to Normal
		unscaledAnimators.Clear();
		foreach (Animator animator in FindObjectsOfType<Animator>()) {
			if (animator.updateMode == AnimatorUpdateMode.UnscaledTime) {
				unscaledAnimators.Add (animator);
				animator.updateMode = AnimatorUpdateMode.Normal;
			}
		}

		pauseSoundSource.Play();
		yield return new WaitForSecondsRealtime (pauseSoundSource.clip.length);
	}

	IEnumerator UnpauseGameCo() {
		pauseSoundSource.Play();
		yield return new WaitForSecondsRealtime (pauseSoundSource.clip.length);

		musicPaused = pausePrevMusicPaused;
		if (!musicPaused) {
			musicSource.UnPause ();
		}
		soundSource.UnPause ();

		foreach (Animator animator in unscaledAnimators) {
			animator.updateMode = AnimatorUpdateMode.UnscaledTime;
		}
		unscaledAnimators.Clear ();

		Time.timeScale = pauseGamePrevTimeScale;
		gamePaused = false;
	}


	/****************** Invincibility */
	public bool isInvincible() {
		return isInvinciblePowerdown || isInvincibleStarman;
	}

	public void MarioInvincibleStarman() {
		StartCoroutine (MarioInvincibleStarmanCo ());
		AddScore (starmanBonus, mario.transform.position);
	}

	IEnumerator MarioInvincibleStarmanCo() {
		isInvincibleStarman = true;
		mario_Animator.SetBool ("isInvincibleStarman", true);
		mario.gameObject.layer = LayerMask.NameToLayer ("Mario After Starman");
		ChangeMusic (hurryUp ? starmanMusicHurry : starmanMusic);
		yield return new WaitForSeconds (MarioInvincibleStarmanDuration);
		isInvincibleStarman = false;
		mario_Animator.SetBool ("isInvincibleStarman", false);
		mario.gameObject.layer = LayerMask.NameToLayer ("Mario");
		ChangeMusic (hurryUp ? levelMusicHurry : levelMusic);
	}

	void MarioInvinciblePowerdown() {
		StartCoroutine (MarioInvinciblePowerdownCo ());
	}

	IEnumerator MarioInvinciblePowerdownCo() {
		isInvinciblePowerdown = true;
		mario_Animator.SetBool ("isInvinciblePowerdown", true);
		mario.gameObject.layer = LayerMask.NameToLayer ("Mario After Powerdown");
		yield return new WaitForSeconds (MarioInvinciblePowerdownDuration);
		isInvinciblePowerdown = false;
		mario_Animator.SetBool ("isInvinciblePowerdown", false);
		mario.gameObject.layer = LayerMask.NameToLayer ("Mario");
	}


	/****************** Powerup / Powerdown / Die */
	public void MarioPowerUp() {
		soundSource.PlayOneShot (powerupSound);
		if (marioSize < 2) {
			StartCoroutine (MarioPowerUpCo ());
		}
		AddScore (powerupBonus, mario.transform.position);
	}

	IEnumerator MarioPowerUpCo() {
		mario_Animator.SetBool ("isPoweringUp", true);
		Time.timeScale = 0f;
		mario_Animator.updateMode = AnimatorUpdateMode.UnscaledTime;

		yield return new WaitForSecondsRealtime (TransformDuration);
		yield return new WaitWhile(() => gamePaused);

		Time.timeScale = 1;
		mario_Animator.updateMode = AnimatorUpdateMode.Normal;

		marioSize++;
		mario.UpdateSize ();
		mario_Animator.SetBool ("isPoweringUp", false);
	}

	public void MarioPowerDown() {
		if (!isPoweringDown) {
			isPoweringDown = true;

			if (marioSize > 0) {
				StartCoroutine (MarioPowerDownCo ());
				soundSource.PlayOneShot (pipePowerdownSound);
			} else {
				MarioRespawn ();
			}
		}
	}

	IEnumerator MarioPowerDownCo() {
		mario_Animator.SetBool ("isPoweringDown", true);
		Time.timeScale = 0f;
		mario_Animator.updateMode = AnimatorUpdateMode.UnscaledTime;

		yield return new WaitForSecondsRealtime (TransformDuration);
		yield return new WaitWhile(() => gamePaused);

		Time.timeScale = 1;
		mario_Animator.updateMode = AnimatorUpdateMode.Normal;
		MarioInvinciblePowerdown ();

		marioSize = 0;
		mario.UpdateSize ();
		mario_Animator.SetBool ("isPoweringDown", false);
		isPoweringDown = false;
	}

	public void MarioRespawn(bool timeup = false) {
		if (!isRespawning) {
			isRespawning = true;

			marioSize = 0;
			lives--;

			soundSource.Stop ();
			musicSource.Stop ();
			musicPaused = true;
			soundSource.PlayOneShot (deadSound);

			Time.timeScale = 0f;
			mario.FreezeAndDie ();

			if (lives > 0) {
				ReloadCurrentLevel (deadSound.length, timeup);
			} else {
				LoadGameOver (deadSound.length, timeup);
			}
		}
	}


	/****************** Enemy kills */
	public void MarioStompEnemy(Enemy enemy) {
		mario_Rigidbody2D.velocity = new Vector2 (mario_Rigidbody2D.velocity.x + stompBounceVelocity.x, stompBounceVelocity.y);
		enemy.StompedByMario ();
		soundSource.PlayOneShot (stompSound);

		// Combo scoring: each consecutive aerial stomp increases the bonus.
		// After 8 stomps the player earns a 1-UP (original SMB behaviour).
		if (stompCombo >= StompComboBonuses.Length) {
			AddLife (enemy.gameObject.transform.position);
		} else {
			AddScore (StompComboBonuses[stompCombo], enemy.gameObject.transform.position);
		}
		stompCombo++;
	}

	// Called by Mario when it lands on the ground, resetting the aerial combo chain.
	public void ResetStompCombo() {
		stompCombo = 0;
	}

	public void MarioStarmanTouchEnemy(Enemy enemy) {
		enemy.TouchedByStarmanMario ();
		soundSource.PlayOneShot (kickSound);
		AddScore (enemy.starmanBonus, enemy.gameObject.transform.position);
	}

	public void RollingShellTouchEnemy(Enemy enemy) {
		enemy.TouchedByRollingShell ();
		soundSource.PlayOneShot (kickSound);
		AddScore (enemy.rollingShellBonus, enemy.gameObject.transform.position);
	}

	public void BlockHitEnemy(Enemy enemy) {
		enemy.HitBelowByBlock ();
		AddScore (enemy.hitByBlockBonus, enemy.gameObject.transform.position);
	}

	public void FireballTouchEnemy(Enemy enemy) {
		enemy.HitByMarioFireball ();
		soundSource.PlayOneShot (kickSound);
		AddScore (enemy.fireballBonus, enemy.gameObject.transform.position);
	}


	/****************** Scene loading */
	void LoadSceneDelay(string sceneName, float delay = LoadSceneDelay) {
		timerPaused = true;
		StartCoroutine (LoadSceneDelayCo (sceneName, delay));
	}

	IEnumerator LoadSceneDelayCo(string sceneName, float delay) {
		float waited = 0;
		while (waited < delay) {
			if (!gamePaused) {
				waited += Time.unscaledDeltaTime;
			}
			yield return null;
		}
		yield return new WaitWhile (() => gamePaused);

		isRespawning = false;
		isPoweringDown = false;
		SceneManager.LoadScene (sceneName);
	}

	public void LoadNewLevel(string sceneName, float delay = LoadSceneDelay) {
		t_GameStateManager.SaveGameState ();
		t_GameStateManager.ConfigNewLevel ();
		t_GameStateManager.sceneToLoad = sceneName;
		LoadSceneDelay ("Level Start Screen", delay);
	}

	public void LoadSceneCurrentLevel(string sceneName, float delay = LoadSceneDelay) {
		t_GameStateManager.SaveGameState ();
		t_GameStateManager.ResetSpawnPosition ();
		LoadSceneDelay (sceneName, delay);
	}

	public void LoadSceneCurrentLevelSetSpawnPipe(string sceneName, int spawnPipeIdx, float delay = LoadSceneDelay) {
		t_GameStateManager.SaveGameState ();
		t_GameStateManager.SetSpawnPipe (spawnPipeIdx);
		LoadSceneDelay (sceneName, delay);
	}

	public void ReloadCurrentLevel(float delay = LoadSceneDelay, bool timeup = false) {
		t_GameStateManager.SaveGameState ();
		t_GameStateManager.ConfigReplayedLevel ();
		t_GameStateManager.sceneToLoad = SceneManager.GetActiveScene ().name;
		LoadSceneDelay (timeup ? "Time Up Screen" : "Level Start Screen", delay);
	}

	public void LoadGameOver(float delay = LoadSceneDelay, bool timeup = false) {
		t_GameStateManager.UpdateHighScore (scores);
		t_GameStateManager.timeup = timeup;
		LoadSceneDelay ("Game Over Screen", delay);
	}


	/****************** HUD and sound effects */
	public void SetHudCoin() {
		coinText.text = "x" + coins.ToString ("D2");
	}

	public void SetHudScore() {
		scoreText.text = scores.ToString ("D6");
	}

	public void SetHudTime() {
		timeLeftInt = Mathf.RoundToInt (timeLeft);
		timeText.text = timeLeftInt.ToString ("D3");
	}

	public void CreateFloatingText(string text, Vector3 spawnPos) {
		GameObject textEffect = Instantiate (FloatingTextEffect, spawnPos, Quaternion.identity);
		textEffect.GetComponentInChildren<TextMesh> ().text = text.ToUpper ();
	}


	public void ChangeMusic(AudioClip clip, float delay = 0) {
		StartCoroutine (ChangeMusicCo (clip, delay));
	}

	IEnumerator ChangeMusicCo(AudioClip clip, float delay) {
		musicSource.clip = clip;
		yield return new WaitWhile (() => gamePaused);
		yield return new WaitForSecondsRealtime (delay);
		yield return new WaitWhile (() => gamePaused || musicPaused);
		if (!isRespawning) {
			musicSource.Play ();
		}
	}

	public void PauseMusicPlaySound(AudioClip clip, bool resumeMusic) {
		StartCoroutine (PauseMusicPlaySoundCo (clip, resumeMusic));
	}

	IEnumerator PauseMusicPlaySoundCo(AudioClip clip, bool resumeMusic) {
		musicPaused = true;
		musicSource.Pause ();
		soundSource.PlayOneShot (clip);
		yield return new WaitForSeconds (clip.length);
		if (resumeMusic) {
			musicSource.UnPause ();
		}
		musicPaused = false;
	}


	/****************** Game state */
	public void AddLife() {
		lives++;
		soundSource.PlayOneShot (oneUpSound);
	}

	public void AddLife(Vector3 spawnPos) {
		AddLife ();
		CreateFloatingText ("1UP", spawnPos);
	}

	// Shared logic for both AddCoin overloads
	private void IncrementCoin() {
		coins++;
		soundSource.PlayOneShot (coinSound);
		if (coins >= 100) {
			AddLife ();
			coins = 0;
		}
		SetHudCoin ();
	}

	public void AddCoin() {
		IncrementCoin ();
		AddScore (coinBonus);
	}

	public void AddCoin(Vector3 spawnPos) {
		IncrementCoin ();
		AddScore (coinBonus, spawnPos);
	}

	public void AddScore(int bonus) {
		scores += bonus;
		SetHudScore ();
	}

	public void AddScore(int bonus, Vector3 spawnPos) {
		scores += bonus;
		SetHudScore ();
		if (bonus > 0) {
			CreateFloatingText (bonus.ToString (), spawnPos);
		}
	}


	/****************** Misc */
	public Vector3 FindSpawnPosition() {
		// t_GameStateManager is cached in Awake; the null fallback guards against
		// any edge-case where this is called before Awake completes.
		GameStateManager gsm = t_GameStateManager != null
			? t_GameStateManager
			: FindObjectOfType<GameStateManager>();

		if (gsm.spawnFromPoint) {
			return GameObject.Find ("Spawn Points")
				.transform.GetChild (gsm.spawnPointIdx)
				.transform.position;
		} else {
			return GameObject.Find ("Spawn Pipes")
				.transform.GetChild (gsm.spawnPipeIdx)
				.transform.Find("Spawn Pos")
				.transform.position;
		}
	}

	public string GetWorldName(string sceneName) {
		string[] sceneNameParts = Regex.Split (sceneName, " - ");
		return sceneNameParts[0];
	}

	public bool isSceneInCurrentWorld(string sceneName) {
		return GetWorldName (sceneName) == GetWorldName (SceneManager.GetActiveScene ().name);
	}

	public void MarioCompleteCastle() {
		timerPaused = true;
		ChangeMusic (castleCompleteMusic);
		musicSource.loop = false;
		mario.AutomaticWalk(mario.castleWalkSpeedX);
	}

	public void MarioCompleteLevel() {
		timerPaused = true;
		ChangeMusic (levelCompleteMusic);
		musicSource.loop = false;
	}

	public void MarioReachFlagPole() {
		timerPaused = true;
		PauseMusicPlaySound (flagpoleSound, false);
		mario.ClimbFlagPole ();
	}
}
