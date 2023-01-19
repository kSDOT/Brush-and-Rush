using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using ManagedCuda;
using System.Linq;
using System.Runtime.InteropServices;

public class SimilarityDetection : MonoBehaviour
{
    public float Threshold = 0.7f;
    Texture2D? errorOverlay;

    int fRows = 50;
    int fColumns = 50;
    int lengthWithWeights = 0;

    public int MaxError = 3611;

    /// <summary>
    /// Takes two images and returns the score difference between the two
    /// Also calculates an erroroverlay and saves it into img-overlay.png
    /// </summary>
    /// <param name="img1"></param>
    /// <param name="img2"></param>
    /// <returns></returns>
    public double Evaluate(string img1, string img2)
    {
        (Texture2D referenceTexture, Texture2D inputTexture) = this.LoadTextures(img1, img2);
        CudaDeviceVariable<Color> output;

        int width; int height;
        var score = this.CompareBlur(referenceTexture, inputTexture, out output, out width, out height);
        this.CreateOverlay(output, out errorOverlay, width, height);

        SaveTexture(errorOverlay, "Assets/Resources/Images/Test/img-overlay.png");
        output.Dispose();

        return MaxError - score;

    }

    CudaKernel CudaConvolution;
    CudaKernel CudaDiff;
    CudaKernel CudaOverlay;

    CudaContext ctx;
    CudaDeviceVariable<int> d_filter;
    private void Start()
    {
        this.ctx = new CudaContext();
        this.CudaConvolution = ctx.LoadKernel(Application.dataPath + "\\Features\\SimilarityDetection\\Cuda\\convolution.ptx", "convolution");
        this.CudaDiff = ctx.LoadKernel(Application.dataPath + "\\Features\\SimilarityDetection\\Cuda\\convolution.ptx", "diff");
        this.CudaOverlay = ctx.LoadKernel(Application.dataPath + "\\Features\\SimilarityDetection\\Cuda\\convolution.ptx", "overlay");

        // Load kernel from file into gpu memory
        #region LoadKernel
        String input = File.ReadAllText(Application.dataPath + "\\Features\\SimilarityDetection\\Filter.txt");
        int i = 0, j = 0;
        int[,] filter = new int[fRows, fColumns];
        foreach (var row in input.Split('\n'))
        {
            j = 0;
            foreach (var col in row.Trim().Split(' '))
            {
                filter[i, j] = int.Parse(col.Trim());
                lengthWithWeights += filter[i, j];
                j++;
            }
            i++;
        }

        d_filter = filter.Cast<int>().ToArray();
        #endregion
    }

    private void OnDestroy()
    {
        if (this.d_filter!= null) { this.d_filter.Dispose(); }
        if (this.ctx!= null) { this.ctx.Dispose(); }
    }
    /// <summary>
    /// Test function from editor
    /// </summary>
    /// <returns></returns>
    [ContextMenu("Test Images")]
    public double Evaluate()
    {

        Debug.Log("Entere Evaluate");

        (Texture2D referenceTexture, Texture2D inputTexture) = this.LoadTextures();
        CudaDeviceVariable<Color> output;
        int width; int height;
        var score = this.CompareBlur(referenceTexture, inputTexture, out output, out width, out height);
        this.CreateOverlay(output, out errorOverlay, width, height);

        SaveTexture(errorOverlay, "Assets/Resources/Images/Test/img1-overlay.png");

        Debug.Log("SaveTexture");

        return MaxError - score;
    }


    /// <summary>
    /// Returns a duplicate of the texture that can be read/written. Original Texture doesn't support this
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    private Texture2D ReadableDuplicate(Texture2D source)
    {
        var pix = source.GetRawTextureData();
        var readableText = new Texture2D(source.width, source.height, source.format, false);
        readableText.LoadRawTextureData(pix);
        readableText.Apply();
        return readableText;
    }
    /// <summary>
    /// Used for testing purposes from editor
    /// </summary>
    /// <returns></returns>
    private (Texture2D, Texture2D) LoadTextures()
    {
       // return LoadTextures("{0}/Features/SimilarityDetection/Resources/Images/screenshots/cur_duplicate.png",
        //    "{0}/Features/SimilarityDetection/Resources/Images/screenshots/cur_original.png");
        //return LoadTextures("Images/Test/cur_original", "Images/Test/cur_duplicate");
        return LoadTextures("Images/Test/pure_white", "Images/Test/pure_black");
        //LoadTextures("Images/Test/colorful1", "Images/Test/colorful2", "Assets/Resources/Images/Test/colorful3.jpg");
    }

    /// <summary>
    /// Returns a read/write duplicate of the textures
    /// </summary>
    /// <param name="path1"></param>
    /// <param name="path2"></param>
    /// <returns></returns>
    public (Texture2D, Texture2D) LoadTextures(string path1, string path2)
    {
        var texture1 = ReadableDuplicate(Resources.Load<Texture2D>(path1));
        var texture2 = ReadableDuplicate(Resources.Load<Texture2D>(path2));
        return (texture1, texture2);
    }

    /// <summary>
    /// Save texture to file
    /// </summary>
    /// <param name="t">Texture to save</param>
    /// <param name="s">Filepath</param>
    private static void SaveTexture(Texture2D t, string s)
    {
        byte[] temp = t.EncodeToPNG();
        File.WriteAllBytes(s, temp);
    }

    /// <summary>
    /// The other method of doing the difference detection, uses Convolution
    /// </summary>
    /// <param name="pic1"></param>
    /// <param name="pic2"></param>
    /// <param name="picDiff">Overlay image consisting of the differences between the two</param>
    /// <returns></returns>
    float CompareBlur(Texture2D pic1, Texture2D pic2, out CudaDeviceVariable<Color> picDiff, out int width, out int height)
    {

        if (pic1.height != pic2.height || pic1.width != pic2.width)
        {
            throw new Exception("Non-matching image dimensions");
        }
        #region FilterCreator
        //int[,] filter = new int[fRows, fColumns];
        //for (int i = 0; i < fRows; i++)
        //{
        //    for (int j = 0; j < fColumns; j++)
        //    {
        //        if ((i > 20 && i < 30) && (j > 20 && j < 30))
        //        {
        //            filter[i, j] = 5;
        //        }
        //        else if ((i > 10 && i < 40) && (j > 10 && j < 40))
        //        {
        //            filter[i, j] = 4;
        //        }
        //        else
        //        {
        //            filter[i, j] = 2;
        //        }
        //    }
        //}
        //FileStream filestream = new FileStream(Application.dataPath + "\\Features\\SimilarityDetection\\Filter.txt", FileMode.Create);
        //var streamwriter = new StreamWriter(filestream);
        //streamwriter.AutoFlush = true;
        //Console.SetOut(streamwriter);
        //Console.SetError(streamwriter);

        //var rowCount = fRows;
        //var colCount = fColumns;
        //System.Text.StringBuilder output = new System.Text.StringBuilder("");

        //for (int row = 0; row < rowCount; row++)
        //{
        //    for (int col = 0; col < colCount; col++)
        //    {
        //        if (col != colCount - 1) { Console.Write(String.Format("{0} ", filter[row, col])); }
        //        else { Console.Write(String.Format("{0}", filter[row, col])); }
        //    }
        //    if (row != rowCount - 1) { Console.WriteLine(); }

        //}
        //Console.SetOut(null);
        //Console.SetError(null);
        #endregion


        // original size before expanding image for convolution
        int original_width = pic1.width; int original_height = pic1.height;
        int N_original = original_width * original_height;

        int conv_padding = (int)Math.Ceiling((fRows - 1) / 2.0f);
        pic1 = BufferImage(pic1, conv_padding, Color.white);
        pic2 = BufferImage(pic2, conv_padding, Color.white);

        height = pic1.height; width = pic1.width;

        // Cuda Convolution
        CudaConvolution.GridDimensions = (N_original + 255) / 256;
        CudaConvolution.BlockDimensions = 256;

        CudaDeviceVariable<Color> d_pic1Pixels = pic1.GetPixels();
        CudaDeviceVariable<Color> d_pic2Pixels = pic2.GetPixels();

        CudaConvolution.Run(d_pic1Pixels.DevicePointer, width, height, d_filter.DevicePointer,
                            fColumns, fRows, lengthWithWeights);

        CudaConvolution.Run(d_pic2Pixels.DevicePointer, width, height, d_filter.DevicePointer,
                          fColumns, fRows, lengthWithWeights);

        ctx.Synchronize();

        // Cuda calculate differences
        CudaDiff.GridDimensions = (width * height + 255) / 256;
        CudaDiff.BlockDimensions = 256;
        float diffAccumulator = 0;
        CudaDeviceVariable<float> d_output = diffAccumulator;

        CudaDiff.Run(d_pic1Pixels.DevicePointer, d_pic2Pixels.DevicePointer, width, height, Threshold, d_output.DevicePointer);

        ctx.Synchronize();

        diffAccumulator = d_output;
        picDiff = d_pic1Pixels;

        d_pic2Pixels.Dispose();
        d_output.Dispose();

        return diffAccumulator;
    }
    /// <summary>
    /// Creates an overlay image of red color, with varying transparency in regards to the error
    /// </summary>
    /// <param name="input"></param>
    /// <param name="error"></param>
    void CreateOverlay(CudaDeviceVariable<Color> input, out Texture2D error, int width, int height)
    {
        error  = new Texture2D(width, height, TextureFormat.RGBAFloat, false);

        CudaOverlay.GridDimensions = (width * height + 255) / 256;
        CudaOverlay.BlockDimensions = 256;

        CudaOverlay.Run(input.DevicePointer, width, height, Threshold);

        ctx.Synchronize();
        error.SetPixelData<Color>(input, 0);
    }
   
    /// <summary>
    /// Buffer image to enable any filters
    /// </summary>
    /// <param name="img"></param>
    /// <param name="pixel">Pixels added on each side of the picture</param>
    /// <param name="color">Color the added color</param>
    /// <returns></returns>

    Texture2D BufferImage(Texture2D img, int pixel, Color color) {
        Texture2D bufImg = new Texture2D(img.width+ pixel * 2, img.height+ pixel * 2, TextureFormat.RGBAFloat, false);
        bufImg.SetPixels(0, 0, img.width, img.height, img.GetPixels());
        return bufImg;
    }

}
