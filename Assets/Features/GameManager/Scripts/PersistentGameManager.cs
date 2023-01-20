using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public class PersistentGameManager : MonoBehaviour
{
    //------Serialized Variables------
    public static double score = 0;

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
