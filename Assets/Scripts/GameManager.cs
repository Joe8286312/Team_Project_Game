using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI References")]
    public UIManager ui;
    public PauseMenuController pauseMenuController;

    [Tooltip("胜利界面")]
    public GameObject victoryUIPanel;

    // FPS 计算相关
    private float deltaTime = 0.0f;
    private float fpsUpdateInterval = 0.5f;
    private float fpsTimer = 0.0f;

    public static bool IsGamePaused { get; private set; } = false;

    public delegate void PauseStateChanged(bool isPaused);
    public static event PauseStateChanged OnPauseStateChanged;

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

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ui = FindObjectOfType<UIManager>();
        pauseMenuController = FindObjectOfType<PauseMenuController>();

        if (scene.name == "DemoMenu")
        {
            UnlockCursor();
            if (victoryUIPanel != null) victoryUIPanel.SetActive(false);
        }
        else
        {
            if (victoryUIPanel == null || !victoryUIPanel.activeSelf)
            {
                ResumeGame();
            }
        }
    }

    void Update()
    {
        if (victoryUIPanel != null && victoryUIPanel.activeSelf) return;

        if (SceneManager.GetActiveScene().name != "DemoMenu")
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }

        if (!IsGamePaused)
        {
            // --- 修改点：使用 GetDisplayTime 获取虚假时间 ---
            if (GameTimer.Instance != null && ui != null)
            {
                // 这里获取的是可能被异常扭曲过的时间
                float displayTime = GameTimer.Instance.GetDisplayTime();
                ui.UpdateTime(displayTime);

                if (pauseMenuController != null)
                    pauseMenuController.UpdateTime(GameTimer.FormatTime(displayTime));
            }
            UpdateFPS();
        }
    }

    public void ShowVictory()
    {
        IsGamePaused = true;
        Time.timeScale = 0f;

        if (victoryUIPanel != null)
        {
            victoryUIPanel.SetActive(true);
        }
        UnlockCursor();
    }

    public void ReturnToMainMenu()
    {
        IsGamePaused = false;
        Time.timeScale = 1f;

        if (victoryUIPanel != null)
            victoryUIPanel.SetActive(false);

        SceneManager.LoadScene("DemoMenu");
    }

    public void TogglePause()
    {
        if (IsGamePaused) ResumeGame();
        else PauseGame();
    }

    private void PauseGame()
    {
        IsGamePaused = true;
        Time.timeScale = 0f;
        if (pauseMenuController != null)
        {
            pauseMenuController.pauseMenu.SetActive(true);
        }
        UnlockCursor();
        // 新增：广播暂停事件
        if (OnPauseStateChanged != null) OnPauseStateChanged(true);
    }

    public void ResumeGame()
    {
        IsGamePaused = false;
        Time.timeScale = 1f;
        if (pauseMenuController != null)
        {
            pauseMenuController.pauseMenu.SetActive(false);
        }
        LockCursor();
        // 新增：广播恢复事件
        if (OnPauseStateChanged != null) OnPauseStateChanged(false);
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void LockCursor()
    {
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