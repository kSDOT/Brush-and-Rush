using UnityEngine;
using NaughtyAttributes;
using TMPro;
using UnityEditor;
public class PersistentGameManager : MonoBehaviour
{
    //------Serialized Variables------
    public static double score = 0;

    public GameObject canvasFront;

    [InfoBox("You can add missing Scenes from the Build Settings menu.", EInfoBoxType.Normal)]
    [SerializeField]
    [Scene]
    private string mainMenuScene;
    [SerializeField]
    [Scene]
    private string mainLevelScene;

    //Main Menu
    [SerializeField]
    [Foldout("Main Menu")]
    private TMP_Text scoreboardText;

    //Pictures
    [SerializeField]
    private Material[] originalPaintings;

    //Level
    [SerializeField]
    [Foldout("Main Level")]
    private CameraScreenshot cameraScreenshot;
    [SerializeField]
    [Foldout("Main Level")]
    private SimilarityDetection similarityDetection;

    //-----Private Variables-----
    private static bool gameWasCompleted = false;
    private static bool inMainMenu = true;

    //-----Lifecycle Methods-----
    public void Start()
    {
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

     
        

        AssetDatabase.CreateAsset(obj, fileName);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

      //  Debug.Log("File saved to: "+AssetDatabase.GetAssetPath(obj));
    }

    public void saveAndEvaluatePicture()
    {
        //take picture
        cameraScreenshot.TakeHiResShot();
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        

        //run similarity detection
        //TODO
        score = this.similarityDetection.Evaluate(CameraScreenshot.ResourcesPath(0,0)+"original", CameraScreenshot.ResourcesPath(0,0)+"duplicate");
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        //return to main Menu
        finishToMainMenu();
    }
}
