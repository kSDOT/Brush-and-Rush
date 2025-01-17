using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CameraScreenshot : MonoBehaviour
{
    public int resWidth = 2550;
    public int resHeight = 3300;

    public GameObject CameraFlash;
    public GameObject Canvas;
    public Material cur_original;
    public Material default_mat;
    public GameObject Pens;

    [SerializeField]
    private GameObject canvasPaintHolder;

    private bool takeHiResShot = false;

    public static string ResourcesPath(int width, int height)
    {
        /* //use this for saving multiple screenshots

         return string.Format("{0}/Features/SimilarityDetection/Resources/Images/screenshots/screen_{1}x{2}_{3}.png",
                              Application.dataPath,
                              width, height,
                              System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")); */
        //AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        //use this for having only one picture that is overwriting itsself

        //Original
        //return string.Format("Images/screenshots/cur_", Application.dataPath);
        return $"{Application.persistentDataPath}/Features/SimilarityDetection/Resources/Images/screenshots/cur_";
    }

    public static string ScreenShotName(int width, int height)
    {
        //return string.Format("{0}/Features/SimilarityDetection/Resources/Images/screenshots/cur_", Application.dataPath);
        return $"{Application.persistentDataPath}/Features/SimilarityDetection/Resources/Images/screenshots/cur_";
    }

    public void TakeHiResShot()
    {
        Pens.SetActive(false);
        takeHiResShot = true;
        takeHiResShot |= Input.GetKeyDown("k");
        if (takeHiResShot)
        {
            CameraFlash.GetComponent<Light>().enabled = true;

            RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
            GetComponent<Camera>().targetTexture = rt;
            Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
            GetComponent<Camera>().Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
            GetComponent<Camera>().targetTexture = null;
            RenderTexture.active = null; // JC: added to avoid errors
            Destroy(rt);
            byte[] bytes = screenShot.EncodeToPNG();
            string filename = ScreenShotName(resWidth, resHeight) + "duplicate.png";
            System.IO.File.WriteAllBytes(filename, bytes);
            Debug.Log(string.Format("Took screenshot to: {0}", filename));
            //takeHiResShot = false;

            CameraFlash.GetComponent<Light>().enabled = false;
            //AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        //take a second screenshot with the OG picture as texture
        if (takeHiResShot)
        {
            //hide Canvas Paint Holder
            canvasPaintHolder.SetActive(false);

            Canvas.GetComponent<Renderer>().material = cur_original;
            CameraFlash.GetComponent<Light>().enabled = true;

            RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
            GetComponent<Camera>().targetTexture = rt;
            Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
            GetComponent<Camera>().Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
            GetComponent<Camera>().targetTexture = null;
            RenderTexture.active = null; // JC: added to avoid errors
            Destroy(rt);
            byte[] bytes = screenShot.EncodeToPNG();
            string filename = ScreenShotName(resWidth, resHeight) + "original.png";
            System.IO.File.WriteAllBytes(filename, bytes);
            Debug.Log(string.Format("Took screenshot to: {0}", filename));
            takeHiResShot = false;

            CameraFlash.GetComponent<Light>().enabled = false;
            Canvas.GetComponent<Renderer>().material = default_mat;
            //AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }
    }

    void LateUpdate()
    {
        //takeHiResShot |= Input.GetKeyDown("k");
        //if (takeHiResShot)
        //{
        //    CameraFlash.GetComponent<Light>().enabled = true;
            
        //    RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        //    GetComponent<Camera>().targetTexture = rt;
        //    Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
        //    GetComponent<Camera>().Render();
        //    RenderTexture.active = rt;
        //    screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        //    GetComponent<Camera>().targetTexture = null;
        //    RenderTexture.active = null; // JC: added to avoid errors
        //    Destroy(rt);
        //    byte[] bytes = screenShot.EncodeToPNG();
        //    string filename = ScreenShotName(resWidth, resHeight) + "duplicate.png";
        //    System.IO.File.WriteAllBytes(filename, bytes);
        //    Debug.Log(string.Format("Took screenshot to: {0}", filename));
        //    //takeHiResShot = false;

        //    CameraFlash.GetComponent<Light>().enabled = false;
        //    AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        //}

        ////take a second screenshot with the OG picture as texture
        //if (takeHiResShot)
        //{
        //    //hide Canvas Paint Holder
        //    canvasPaintHolder.SetActive(false);

        //    Canvas.GetComponent<Renderer>().material = cur_original;
        //    CameraFlash.GetComponent<Light>().enabled = true;

        //    RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        //    GetComponent<Camera>().targetTexture = rt;
        //    Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
        //    GetComponent<Camera>().Render();
        //    RenderTexture.active = rt;
        //    screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        //    GetComponent<Camera>().targetTexture = null;
        //    RenderTexture.active = null; // JC: added to avoid errors
        //    Destroy(rt);
        //    byte[] bytes = screenShot.EncodeToPNG();
        //    string filename = ScreenShotName(resWidth, resHeight) + "original.png";
        //    System.IO.File.WriteAllBytes(filename, bytes);
        //    Debug.Log(string.Format("Took screenshot to: {0}", filename));
        //    takeHiResShot = false;

        //    CameraFlash.GetComponent<Light>().enabled = false;
        //    Canvas.GetComponent<Renderer>().material = default_mat;
        //    AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        //}
    }
}
