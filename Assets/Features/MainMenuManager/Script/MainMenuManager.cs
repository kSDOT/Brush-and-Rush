using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class MainMenuManager : MonoBehaviour
{
    //-----Serialized Variables-----
    [InfoBox("You can add missing Scenes from the Build Settings menu.", EInfoBoxType.Normal)]
    [SerializeField]
    [Scene]
    private string mainLevelScene;

    //-----Event Methods-----
    public void loadMainLevel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainLevel");
    }
}
