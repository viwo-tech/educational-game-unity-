using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

// ============================================================================
// GAME MANAGER - Handles game state and level management (Singleton)
// ============================================================================
public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    
    private int currentLevel = 1;
    private int totalLevels = 10;
    private int playerScore = 0;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartLevel(int levelNumber)
    {
        currentLevel = levelNumber;
        SceneManager.LoadScene($"Level_{levelNumber}");
    }

    public void NextLevel()
    {
        if (currentLevel < totalLevels)
        {
            currentLevel++;
            SceneManager.LoadScene($"Level_{currentLevel}");
        }
        else
        {
            LoadGameComplete();
        }
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene($"Level_{currentLevel}");
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadGameComplete()
    {
        SceneManager.LoadScene("GameComplete");
    }

    public void AddScore(int points)
    {
        playerScore += points;
        Debug.Log($"Score: {playerScore}");
    }

    public int GetCurrentLevel() => currentLevel;
    public int GetTotalLevels() => totalLevels;
    public int GetPlayerScore() => playerScore;
}

// ============================================================================
// LEVEL DATA - ScriptableObject untuk konfigurasi level
// ============================================================================
[System.Serializable]
public class LevelData
{
    public string levelTitle;
    public string levelDescription;
    public int levelNumber;
    public int difficulty; // 1-10
    public string educationTopic; // Matematika, Bahasa, Sains, etc
    public int maxScore;
    
    [TextArea(3, 5)]
    public string[] questions;
    public string[] answers;
}

// ============================================================================
// LEVEL MANAGER - Setup dan completion level
// ============================================================================
public class LevelManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelTitle;
    [SerializeField] private TextMeshProUGUI levelDescription;
    [SerializeField] private int currentLevelNumber;
    
    private LevelData currentLevelData;

    private void Start()
    {
        currentLevelNumber = GameManager.instance.GetCurrentLevel();
        LoadLevelData();
        SetupLevel();
    }

    private void LoadLevelData()
    {
        // Load level data dari Resources folder
        TextAsset jsonFile = Resources.Load<TextAsset>($"Levels/Level_{currentLevelNumber}");
        
        if (jsonFile != null)
        {
            currentLevelData = JsonUtility.FromJson<LevelData>(jsonFile.text);
            Debug.Log($"Level data loaded: {currentLevelData.levelTitle}");
        }
        else
        {
            Debug.LogWarning($"Level data not found for Level {currentLevelNumber}");
        }
    }

    private void SetupLevel()
    {
        if (currentLevelData != null)
        {
            levelTitle.text = currentLevelData.levelTitle;
            levelDescription.text = currentLevelData.levelDescription;
            Debug.Log($"Level {currentLevelNumber} setup complete");
        }
    }

    public void CompleteLevel(int earnedPoints)
    {
        GameManager.instance.AddScore(earnedPoints);
        Debug.Log($"Level {currentLevelNumber} completed! Points earned: {earnedPoints}");
        Invoke("GoToNextLevel", 2f);
    }

    private void GoToNextLevel()
    {
        GameManager.instance.NextLevel();
    }

    public LevelData GetCurrentLevelData()
    {
        return currentLevelData;
    }
}

// ============================================================================
// UI MANAGER - Kontrol semua UI elements
// ============================================================================
public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private GameObject pausePanel;

    private bool isPaused = false;

    private void Start()
    {
        if (pauseButton != null) pauseButton.onClick.AddListener(TogglePause);
        if (restartButton != null) restartButton.onClick.AddListener(RestartLevel);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(GoToMainMenu);
        
        UpdateUI();
    }

    private void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (GameManager.instance != null)
        {
            if (scoreText != null)
                scoreText.text = $"Score: {GameManager.instance.GetPlayerScore()}";
            
            if (levelText != null)
                levelText.text = $"Level: {GameManager.instance.GetCurrentLevel()}/{GameManager.instance.GetTotalLevels()}";
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        if (pausePanel != null)
            pausePanel.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        GameManager.instance.RestartLevel();
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        GameManager.instance.LoadMainMenu();
    }

    public bool IsPaused() => isPaused;
}

// ============================================================================
// LEVEL SELECTOR - Menampilkan dan handle level selection
// ============================================================================
public class LevelSelector : MonoBehaviour
{
    [SerializeField] private int totalLevels = 10;
    [SerializeField] private Transform levelButtonsContainer;
    [SerializeField] private Button levelButtonPrefab;

    private void Start()
    {
        CreateLevelButtons();
    }

    private void CreateLevelButtons()
    {
        for (int i = 1; i <= totalLevels; i++)
        {
            Button newButton = Instantiate(levelButtonPrefab, levelButtonsContainer);
            
            TextMeshProUGUI buttonText = newButton.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = $"Level {i}";
            
            int levelNumber = i;
            newButton.onClick.AddListener(() => StartLevel(levelNumber));
        }
    }

    private void StartLevel(int levelNumber)
    {
        GameManager.instance.StartLevel(levelNumber);
    }
}

// ============================================================================
// PUZZLE GAME - Logic permainan puzzle dan answer checking
// ============================================================================
public class PuzzleGame : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private Button[] answerButtons;
    [SerializeField] private TextMeshProUGUI feedbackText;
    
    private LevelManager levelManager;
    private string correctAnswer;
    private int questionsAnswered = 0;
    private int correctAnswersCount = 0;
    private bool waitingForNextQuestion = false;

    private void Start()
    {
        levelManager = GetComponent<LevelManager>();
        SetupPuzzle();
    }

    private void SetupPuzzle()
    {
        LevelData levelData = levelManager.GetCurrentLevelData();
        
        if (levelData != null && levelData.questions.Length > 0)
        {
            int randomIndex = Random.Range(0, levelData.questions.Length);
            questionText.text = levelData.questions[randomIndex];
            correctAnswer = levelData.answers[randomIndex];
            
            for (int i = 0; i < answerButtons.Length; i++)
            {
                Button button = answerButtons[i];
                button.onClick.AddListener(() => CheckAnswer(button));
            }
            
            waitingForNextQuestion = false;
        }
    }

    private void CheckAnswer(Button clickedButton)
    {
        if (waitingForNextQuestion) return;
        
        TextMeshProUGUI buttonText = clickedButton.GetComponentInChildren<TextMeshProUGUI>();
        string selectedAnswer = buttonText.text;
        
        questionsAnswered++;

        if (selectedAnswer == correctAnswer)
        {
            correctAnswersCount++;
            feedbackText.text = "✓ Correct!";
            feedbackText.color = Color.green;
        }
        else
        {
            feedbackText.text = "✗ Wrong! Try again.";
            feedbackText.color = Color.red;
        }

        waitingForNextQuestion = true;

        if (questionsAnswered >= 3)
        {
            Invoke("CompleteLevel", 2f);
        }
        else
        {
            Invoke("SetupPuzzle", 2f);
        }
    }

    private void CompleteLevel()
    {
        int earnedPoints = correctAnswersCount * 10;
        levelManager.CompleteLevel(earnedPoints);
    }
}
