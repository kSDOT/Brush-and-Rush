using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class MainMenuManager : MonoBehaviour
{
    //-----Serialized Variables-----
    [SerializeField]
    [Scene]
    private string mainLevelScene;

    //-----Event Methods-----
    public void loadMainLevel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainLevel");
    }
}
