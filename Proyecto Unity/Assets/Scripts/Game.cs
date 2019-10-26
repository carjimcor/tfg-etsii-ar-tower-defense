using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Vuforia;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(WaveSpawner))]
public class Game : MonoBehaviour
{
    #region Variables
    public static Game instance;
    Ray TouchRay => Camera.main.ScreenPointToRay(Input.mousePosition);

    GameBehaviorCollection nonEnemies = new GameBehaviorCollection();
    GameBehaviorCollection enemies = new GameBehaviorCollection();
    public int EnemiesCount => enemies.Count;
    WaveSpawner waveSpawner = default;
    bool validSingleTouch = false;
    float previousTimeScale = 1f;

    // Inspector Variables
    [SerializeField]
    VuforiaBehaviour vuforiaBehaviour = default;
    [SerializeField]
    CameraController cameraController = default;
    Vector3 cameraStartPosition = default;
    Quaternion cameraStartRotation = default;
    float cameraFieldOfView = default;

    [SerializeField]
    GameObject gameCanvas = default;

    [SerializeField]
    AnimationCurve enemyHealth = default;
    [SerializeField]
    int topWave = 40;
    float healthMultiplier = default;

    [Header("Board")]
    [SerializeField]
    bool playing = true;
    [SerializeField]
    GameBoard board = default;
    [SerializeField]
    Vector2Int boardSize = new Vector2Int(11, 11);

    [Header("Factories")]
    [SerializeField]
    GameTileContentFactory tileContentFactory = default;
    [SerializeField]
    EnemyFactory enemyFactory = default;
    [SerializeField]
    WarFactory warFactory = default;

    [Header("UserControl")]
    [SerializeField]
    TowerSelection towerSelection = default;
    [SerializeField]
    TowerType selectedTowerType = TowerType.Shooting;

    [Header("Starting Attributes")]
    [SerializeField]
    int health = 1;
    [SerializeField]
    int credits = 100;

    [Header("Canvas")]
    [SerializeField]
    GameObject normalCanvas = default;
    [SerializeField]
    GameObject gameOverCanvas = default;

    [Header("Texts")]
    [SerializeField]
    Text wavesSurvived = default;
    [SerializeField]
    Text healthText = default;
    [SerializeField]
    Text creditsText = default;

    [Header("WaveSpawner")]
    [SerializeField]
    float startTime = 5f;

    [Header("Templates")]
    [SerializeField]
    int selectedTemplate = -1;
    [SerializeField]
    string[] templates = default;

    private bool IsPointerOverUIObject()
    {
        // Checks if the 'mouse' or 'finger' is on top of an UI element
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
    #endregion

    #region Static Methods
    public static void ReceiveDamage()
    {
        // Applies 1 point of damage to the player and checks for gameover
        instance.health -= 1;
        instance.healthText.text = instance.health.ToString();

        if (instance.health <= 0)
        {
            instance.GameOver();
        }

    }

    public static void EarnCredits(int _credits)
    {
        // Adds money to the player and updates de upgrade button if neccesary
        instance.credits += _credits;
        instance.creditsText.text = instance.credits.ToString();
        if (instance.towerSelection.Tower != null)
        {
            instance.towerSelection.UpdateCanBuy();
        }
    }

    public static void LoseCredits(int _credits)
    {
        // Takes away money from the player
        int credits_before = instance.credits;
        Debug.Assert(instance.credits >= _credits, "Not enough credits!");
        instance.credits -= _credits;
        instance.creditsText.text = instance.credits.ToString();
    }

    public static bool EnoughCredits(int _credits)
    {
        // Checks if the player has more money than the variable _credits
        return instance.credits >= _credits;
    }

    public static void EnemyDestroyed(int _credits)
    {
        // Method for when an enemy is killed, adds money to the player
        EarnCredits(_credits);
    }

    public static void LevelWon()
    {
        // Placeholder method in case a winning mechanic is needed
        // Since all levels are endless right now this method is worthless, it justs prints a message on the Unity console
        Debug.Log("You won!");
    }

    public static int enemyTypesCount()
    {
        // Return the number of enemies available
        return instance.enemyFactory.PrefabCount;
    }

    public static void UpdateEnemyHealth(int currentWave)
    {
        // Updates the enemies health according to the current wave as long at the current wave is lower than the limit wave
        if (currentWave > instance.topWave)
        {
            return;
        }
        instance.healthMultiplier = instance.enemyHealth.Evaluate(1f * currentWave / instance.topWave);
    }

    public static Enemy SpawnEnemy(int spawnPointIndex, int enemyPrefabIndex)
    {
        // Spawns an enemy of some type on a spawnpoint
        GameTile spawnPoint = instance.board.GetSpawnPoint(spawnPointIndex);
        Enemy enemy = instance.enemyFactory.Get(enemyPrefabIndex);
        enemy.UpdateHealth(instance.healthMultiplier);
        enemy.SpawnOn(spawnPoint);
        instance.enemies.Add(enemy);

        return enemy;
    }

    public static Missile SpawnMissile()
    {
        // Instantiates a Missile prefab
        Missile missile = instance.warFactory.Missile;
        instance.nonEnemies.Add(missile);
        return missile;
    }

    public static Bullet SpawnBullet()
    {
        // Instantiates a Bullet prefab
        Bullet bullet = instance.warFactory.Bullet;
        instance.nonEnemies.Add(bullet);
        return bullet;
    }

    public static Explosion SpawnExplosion()
    {
        // Instantiates a explosion prefab
        Explosion explosion = instance.warFactory.Explosion;
        instance.nonEnemies.Add(explosion);
        return explosion;
    }

    #endregion

    #region Unity Methods
    void OnValidate()
    {
        // Validates the board size
        if (boardSize.x < 2)
        {
            boardSize.x = 2;
        }
        if (boardSize.y < 2)
        {
            boardSize.y = 2;
        }
    }

    void OnEnable()
    {
        // Inits the singleton mechanic
        instance = this;
    }

    void Awake()
    {
        // Inits all the variables, the gameboard and the wave spawner
        Time.timeScale = 1f;
        int templateIndex = PlayerPrefs.GetInt("templateIndex");
        if (templateIndex >= -1)
        {
            selectedTemplate = templateIndex;
        }

        Camera camera = Camera.main;
        cameraStartPosition = camera.transform.position;
        cameraStartRotation = camera.transform.rotation;
        cameraFieldOfView = camera.fieldOfView;

        healthMultiplier = enemyHealth.Evaluate(0f);
        creditsText.text = credits.ToString();
        healthText.text = health.ToString();
        if (selectedTemplate >= 0)
        {
            board.Initialize(boardSize, tileContentFactory, templates[selectedTemplate]);
        }
        else
        {
            board.Initialize(boardSize, tileContentFactory);
        }
        if (playing)
        {
            waveSpawner = GetComponent<WaveSpawner>();
            StartCoroutine(StartWaveSpawner());
        }
        else
        {
            gameCanvas.SetActive(false);
            camera.transform.position = new Vector3(0, 20, 0);
            camera.transform.rotation = Quaternion.Euler(90, 0, 0);
        }
    }

    void Update()
    {

        // Manages the flow of the game
        // To be able to create and edit maps, uncomment the next commented parts, disable 'playing' on the Game component
        // in the Unity editor and uncomment the mouse click section in lines 354,355

        //if (playing)
        //{
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            selectedTowerType = TowerType.Shooting;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            selectedTowerType = TowerType.Missile;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            selectedTowerType = TowerType.Area;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            selectedTowerType = TowerType.Laser;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            selectedTowerType = TowerType.Slow;
        }

        if (SingleFingerTouchEnded())
            HandleTouch();

        enemies.GameUpdate();
        Physics.SyncTransforms();
        board.GameUpdate();
        nonEnemies.GameUpdate();
        //}
        //else
        //{
        //    if (SingleFingerTouchEnded())
        //        HandleEditingTouch();
        //    else if (Input.GetMouseButtonDown(1))
        //    {
        //        HandleEditingTouch(false);
        //    }
        //    if (Input.GetKeyDown(KeyCode.Space))
        //    {
        //        Debug.Log(board.getTemplate());
        //    }
        //}
        //if (Input.GetKeyDown(KeyCode.V))
        //{
        //    board.ShowPaths = !board.ShowPaths;
        //}
    }

    void GameOver()
    {
        // Stops the current game and displays the gameover screen
        wavesSurvived.text = (waveSpawner.currentWave - 1).ToString();
        normalCanvas.SetActive(false);
        gameOverCanvas.SetActive(true);
        waveSpawner.enabled = false;
        ToggleTime();
    }

    IEnumerator StartWaveSpawner()
    {
        // Initial wait before starting the wave spawner
        yield return new WaitForSeconds(startTime);
        waveSpawner.enabled = true;
        yield break;
    }

    #endregion

    #region User Methods
    bool SingleFingerTouchEnded()
    {
        // Checks if the player input is valid: single finger touch without moving and leaving the screen
        // To edit maps in the unity editor uncomment the next 2 lines of code

        // Only for PC testing:
        //if (Input.GetMouseButtonUp(0))
        //    return true;

        if (Input.touchCount != 1)
        {
            if (validSingleTouch)
                validSingleTouch = false;
            return false;
        }

        Touch touch = Input.GetTouch(0);

        if (!validSingleTouch && touch.phase == TouchPhase.Began)
        {
            if (IsPointerOverUIObject())
                return false;
            validSingleTouch = true;
            return false;
        }

        if (validSingleTouch)
        {
            if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Canceled)
            {
                validSingleTouch = false;
                return false;
            }

            if (touch.phase == TouchPhase.Ended)
            {
                validSingleTouch = false;
                return true;
            }
        }

        return false;
    }

    void HandleTouch()
    {
        // Manages turret placement system or 'buying' mechanic

        if (IsPointerOverUIObject())
        {
            return;
        }

        if (towerSelection.Tower != null)
        {
            towerSelection.Tower = null;
            return;
        }

        GameTile tile = board.TouchCell(TouchRay);
        if (tile != null)
        {
            if (tile.BlocksInteraction)
                return;

            if (tile.Content.Type == GameTileContentType.Tower)
            {
                Tower tower = (Tower)tile.Content;
                towerSelection.Tower = tower;
            }
            else
                board.ToggleTower(tile, selectedTowerType);
        }
    }

    void HandleEditingTouch(bool leftClick = true)
    {
        // Manages the map editing input system
        // Controls are:
        // Left click: toogle spawnpoint
        // Left click + Left shift: toogle destination
        // Right click: toggle wall
        // Right click + Left shift: toogle blocking

        // Not managed here but also editing controls:
        // Space: prints the current map layout in the console
        // V: toggles path arrows
        if (EventSystem.current.IsPointerOverGameObject(0) || EventSystem.current.IsPointerOverGameObject())
            return;

        GameTile tile = board.TouchCell(TouchRay);
        if (tile != null)
        {
            if (leftClick)
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    board.ToggleDestination(tile);
                }
                else
                {
                    board.ToggleSpawnPoint(tile);
                }
            }
            else
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    board.ToggleBlocking(tile);
                }
                else
                {
                    board.ToggleWall(tile);
                }
            }
        }
    }

    public void ChooseTurret(int turretIndex)
    {
        // Changes the selected turret type, used in UI buttons
        selectedTowerType = (TowerType)turretIndex;
    }

    public void ToggleSpeed()
    {
        // Speeds up the game or resets the speed if it reached the maximum (x3)
        if (Time.timeScale == 0f)
        {
            ToggleTime();
        }

        if (Time.timeScale < 3f)
        {
            Time.timeScale += 1f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    public void ToggleTime(bool ignoreIfAlreadyPaused = false)
    {
        // Pause/resume the game time
        if (ignoreIfAlreadyPaused && Time.timeScale <= 0f)
        {
            return;
        }

        if (Time.timeScale > 0f)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = previousTimeScale;
        }
        //speedText.text = "Speed: x" + Time.timeScale;
    }

    public void ToggleAR()
    {
        // Toggles AR on or off
        // When its on, the tower selection camera controller is enabled
        // When its off, the camera starting position, rotation and field of view are reset
        vuforiaBehaviour.enabled = !vuforiaBehaviour.enabled;
        cameraController.enabled = !cameraController.enabled;

        towerSelection.ARenabled(vuforiaBehaviour.enabled);

        if (!vuforiaBehaviour.enabled)
        {
            Camera camera = Camera.main;
            camera.transform.position = cameraStartPosition;
            camera.transform.rotation = cameraStartRotation;
            // For some reason when returning to no AR mode, changing the camera fov doesn't make anything
            // If you turn on and off orthografic mode, it starts working as normal
            camera.orthographic = true;
            camera.orthographic = false;
            camera.fieldOfView = cameraFieldOfView;
        }
    }


    public void RetryLevel()
    {
        // Reloads the current level
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Menu()
    {
        // Loads the Menu Scene
        SceneManager.LoadScene("Menu");
    }

    public void ExitGame()
    {
        // Quits the game
        Application.Quit();
    }
    #endregion
}
