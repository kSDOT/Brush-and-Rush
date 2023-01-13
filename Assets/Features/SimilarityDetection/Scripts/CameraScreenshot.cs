using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScreenshot : MonoBehaviour
{
    public int resWidth = 2550;
    public int resHeight = 3300;

    public GameObject CameraFlash;
    public GameObject Canvas;
    public Material cur_original;
    public Material default_mat;

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

        //use this for having only one picture that is overwriting itsself
        return string.Format("Images/screenshots/cur_", Application.dataPath);
    }

    public static string ScreenShotName(int width, int height)
    {
        return string.Format("{0}/Features/SimilarityDetection/Resources/Images/screenshots/cur_", Application.dataPath);
    }

    public void TakeHiResShot()
    {
        takeHiResShot = true;
    }

    void LateUpdate()
    {
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
        }
    }
}
