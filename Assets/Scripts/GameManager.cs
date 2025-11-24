using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int collection = 0;
    public int gameLevel = 1;

    // 引用UI和控制器
    public UIManager ui;
    public PauseMenuController pauseMenuController;
    // --- 修改点 1: 移除了对 GameTimer 的公共引用 ---
    // public GameTimer gameTimer;

    // FPS计算相关变量
    private float deltaTime = 0.0f;
    private float fpsUpdateInterval = 0.5f;
    private float fpsTimer = 0.0f;

    public static bool IsGamePaused { get; private set; } = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name != "DemoMenu")
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }

        if (!IsGamePaused)
        {
            // --- 修改点 2: 使用 GameTimer.Instance 访问计时器 ---
            // 确保 GameTimer 实例存在
            if (GameTimer.Instance != null && ui != null && pauseMenuController != null)
            {
                float elapsedTime = GameTimer.Instance.GetElapsedTime();
                ui.UpdateTime(elapsedTime);
                pauseMenuController.UpdateTime(GameTimer.FormatTime(elapsedTime));
            }
            UpdateFPS();
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ui = FindObjectOfType<UIManager>();
        pauseMenuController = FindObjectOfType<PauseMenuController>();

        if (scene.name.StartsWith("Demo") && scene.name != "DemoMenu")
        {
            if (scene.name == "Demo")
            {
                // --- 修改点 3: 使用 GameTimer.Instance ---
                if (GameTimer.Instance != null)
                    GameTimer.Instance.ResetAndStart();
            }
            ResumeGame();
        }
        else if (scene.name == "DemoMenu")
        {
            PauseGame(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void TogglePause()
    {
        if (IsGamePaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame(true);
        }
    }

    private void PauseGame(bool showPauseMenu)
    {
        IsGamePaused = true;
        Time.timeScale = 0f;
        if (showPauseMenu && pauseMenuController != null)
        {
            pauseMenuController.pauseMenu.SetActive(true);
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        IsGamePaused = false;
        Time.timeScale = 1f;
        if (pauseMenuController != null)
        {
            pauseMenuController.pauseMenu.SetActive(false);
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UpdateFPS()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        fpsTimer += Time.deltaTime;

        if (fpsTimer >= fpsUpdateInterval)
        {
            int fps = Mathf.CeilToInt(1.0f / deltaTime);
            if (ui != null)
            {
                ui.UpdateFPS(fps);
            }
            fpsTimer = 0.0f;
        }
    }
}