using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class LoadTexToCanvas : MonoBehaviour
{
    public Material duplicate;
    public Material overlay;

    public void DisplayImage()
    {
        //(Texture2D duplicateTex, Texture2D overlayTex) = this.LoadTextures("Images/screenshots/cur_duplicate", "Images/Test/img-overlay");
        (Texture2D duplicateTex, Texture2D overlayTex) = this.LoadTextures($"{Application.persistentDataPath}/Features/SimilarityDetection/Resources/Images/screenshots/cur_duplicate.png", $"{Application.persistentDataPath}/Assets/Resources/Images/Test/img-overlay.png");
        duplicate.SetTexture("_MainTex", duplicateTex);
        overlay.SetTexture("_MainTex", overlayTex);
    }

    public (Texture2D, Texture2D) LoadTextures(string path1, string path2)
    {
        var tmpTex1 = new Texture2D(1, 1);
        var tmpTex2 = new Texture2D(1, 1);
        byte[] tmp1 = File.ReadAllBytes(path1);
        byte[] tmp2 = File.ReadAllBytes(path2);
        tmpTex1.LoadImage(tmp1);
        tmpTex2.LoadImage(tmp1);
        var texture1 = ReadableDuplicate(tmpTex1);
        var texture2 = ReadableDuplicate(tmpTex2);
        //var texture1 = ReadableDuplicate(Resources.Load<Texture2D>(path1));
        //var texture2 = ReadableDuplicate(Resources.Load<Texture2D>(path2));

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

