using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LoadTexToCanvas : MonoBehaviour
{
    public Material duplicate;
    public Material overlay;

    public string duplicateString = "Images/screenshots/cur_duplicate";
    public string overlayString = "Images/Test/img-overlay";

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DisplayImage()
    {
        //AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        (Texture2D duplicateTex, Texture2D overlayTex) = this.LoadTextures("Images/screenshots/cur_duplicate", "Images/Test/img-overlay");
        //Texture2D duplicateTex;
        //Texture2D overlayTex;
        //duplicateTex = new Texture2D(2048, 2048);
        //overlayTex = new Texture2D(2048, 2048);
        //WWW dT = new WWW("{0}/Features/SimilarityDetection/Resources/Images/screenshots/cur_duplicate.png");
        //WWW oT = new WWW("{0}/FeatureResources/Images/Test/image-overlay.png");
        //dT.LoadImageIntoTexture(duplicateTex);
        //oT.LoadImageIntoTexture(overlayTex);
        duplicate.SetTexture("_MainTex", duplicateTex);
        overlay.SetTexture("_MainTex", overlayTex);
    }

    public (Texture2D, Texture2D) LoadTextures(string path1, string path2)
    {
        var texture1 = ReadableDuplicate(Resources.Load<Texture2D>(path1));
        var texture2 = ReadableDuplicate(Resources.Load<Texture2D>(path2));
        return (texture1, texture2);
    }

    private Texture2D ReadableDuplicate(Texture2D source)
    {
        var pix = source.GetRawTextureData();
        var readableText = new Texture2D(source.width, source.height, source.format, false);
        readableText.LoadRawTextureData(pix);
        readableText.Apply();
        return readableText;
    }
}

