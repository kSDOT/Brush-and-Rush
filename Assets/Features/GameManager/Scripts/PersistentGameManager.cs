using UnityEngine;
using NaughtyAttributes;
using TMPro;
using UnityEditor;
using MarchingCubesProject;
using BNG;

public class PersistentGameManager : MonoBehaviour
{
    //------Serialized Variables------
    public static double score = 0;
    public static bool teleport = true;

    public GameObject canvasFront;

    [InfoBox("You can add missing Scenes from the Build Settings menu.", EInfoBoxType.Normal)]
    [SerializeField]
    [Scene]
    private string mainMenuScene;
    [SerializeField]
    [Scene]
    private string mainLevelScene;
    [SerializeField]
    [Scene]
    private string sculptScene;

    //Main Menu
    [SerializeField]
    [Foldout("Main Menu")]
    private TMP_Text scoreboardText;

    //Pictures
    [SerializeField]
    private Material[] originalPaintings;

    //PlayerMovement
    [BoxGroup("PlayerMovement")]
    [SerializeField]
    private GameObject playerController;
    [BoxGroup("PlayerMovement")]
    [SerializeField]
    private Lever movementSwitchLever;

    //Level
    [SerializeField]
    [Foldout("Main Level")]
    private CameraScreenshot cameraScreenshot;
    [SerializeField]
    [Foldout("Main Level")]
    private SimilarityDetection similarityDetection;
    [SerializeField]
    [Foldout("Main Level")]
    private MarchingImplementation sculpture;
    //-----Private Variables-----
    private static bool gameWasCompleted = false;
    private static bool inMainMenu = true;

    //-----Lifecycle Methods-----
    public void Start()
    {
        System.IO.Directory.CreateDirectory($"{Application.persistentDataPath}/Features/SimilarityDetection/Resources/Images/screenshots/");
        System.IO.Directory.CreateDirectory($"{Application.persistentDataPath}/Assets/Resources/Images/Test/");
        System.IO.Directory.CreateDirectory($"{Application.persistentDataPath}/Features/SimilarityDetection/Cuda/");

        //Check if the Game is currently in the Main Menu
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == mainMenuScene)
        {
            inMainMenu = true;
            updateScoreBoard();
        }
        else
        {
            inMainMenu = false;
            score = 0.0;
        }

        setMovementSystem();
    }

    void Update() {
        if (Input.GetKeyDown("j")) {
            NewRandomPicture();
        }
        
    }
    //-----Methods-----
    private void updateScoreBoard()
    {
        //Check if there is a score to be displayed
        if (gameWasCompleted)
        {
            scoreboardText.enabled = true;
            scoreboardText.text = score.ToString();
        }
        else
        {
            scoreboardText.enabled = false;
        }
    }

    //-----Event Methods------
    public void abortToMainMenu()
    {
        gameWasCompleted = false;
        UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuScene);
    }

    public void finishToMainMenu()
    {
        gameWasCompleted = true;
        UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuScene);
    }
    
    public void loadMainLevel()
    {
       
        UnityEngine.SceneManagement.SceneManager.LoadScene(mainLevelScene);
        
    }

    public void loadScupltLevel()
    {

        UnityEngine.SceneManagement.SceneManager.LoadScene(sculptScene);

    }

    public void NewRandomPicture() {

        int picSize = originalPaintings.Length;
        int newOriginal = Random.Range(0, picSize);
        canvasFront.gameObject.GetComponent<Renderer>().material = originalPaintings[newOriginal];
        Material temp = new Material(originalPaintings[newOriginal]);
        SaveObjectToFile(temp, "Assets/Features/SimilarityDetection/Resources/Images/original_pictures/Materials/cur_original.mat"); 

        // Debug.Log("New picture: "+newOriginal);
    }

    public static void SaveObjectToFile(Object obj, string fileName)
    {

     
        

        //AssetDatabase.CreateAsset(obj, fileName);
       // AssetDatabase.SaveAssets();
        //AssetDatabase.Refresh();

      //  Debug.Log("File saved to: "+AssetDatabase.GetAssetPath(obj));
    }

    public void saveAndEvaluatePicture()
    {
        //take picture
        cameraScreenshot.TakeHiResShot();
        //run similarity detection
        //TODO
        //Old
        //AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        //score = this.similarityDetection.Evaluate(CameraScreenshot.ResourcesPath(0, 0) + "original", CameraScreenshot.ResourcesPath(0, 0) + "duplicate");
        //AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        
        //New
        score = this.similarityDetection.Evaluate(CameraScreenshot.ResourcesPath(0, 0) + "original.png", CameraScreenshot.ResourcesPath(0, 0) + "duplicate.png");
        //return to main Menu
        finishToMainMenu();
    }

    public void evaluateSculpture()
    {
        //run similarity detection
        //TODO
        GameObject parent = new GameObject("Sculpture");
        StartCoroutine(this.sculpture.Compare(parent, returnValue => score = returnValue));
        //AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        //return to main Menu
        finishToMainMenu();
    }

    //Movement Switching

    public void toggleMovement()
    {
        //Switch teleport
        teleport = !teleport;

        setMovementSystem();
    }

    public void toggleMovement(bool shouldTeleport)
    {
        teleport = shouldTeleport;

        setMovementSystem();
    }

    public void setMovementSystem()
    {
        PlayerTeleport locomotionTeleport = playerController.GetComponent<PlayerTeleport>();
        SmoothLocomotion locomotionSmooth = playerController.GetComponent<SmoothLocomotion>();

        //Check which locomotion system to use
        if (teleport)
        {
            locomotionSmooth.enabled = false;
            locomotionTeleport.enabled = true;
        }
        else
        {
            locomotionTeleport.enabled = false;
            locomotionSmooth.enabled = true;
        }
    }

    private void setMovementLever()
    {
        //Check if lever exists
        if (movementSwitchLever)
        {
            //When Teleport is enabled, it will use the minium rotation, otherwise the maximum
            movementSwitchLever.InitialXRotation = teleport ? movementSwitchLever.MinimumXRotation : movementSwitchLever.MaximumXRotation;
        }
    }
}
