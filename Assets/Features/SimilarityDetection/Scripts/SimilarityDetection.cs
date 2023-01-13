using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Unity.VisualScripting;

public class SimilarityDetection : MonoBehaviour
{
    int test = 0;
    public double Threshold = 40;
    Texture2D? errorOverlay;
   /// <summary>
   /// Substracting two colors can yield negative, we want only positive values
   /// </summary>
   /// <param name="c1"></param>
   /// <returns></returns>
    public static Color abs(Color c1)
    {
        c1.r = Mathf.Abs(c1.r);
        c1.g = Mathf.Abs(c1.g);
        c1.b = Mathf.Abs(c1.b);
        c1.a = Mathf.Abs(c1.a);

        return c1;
    }
    /// <summary>
    /// Treats the color as vector, returns l2 norm 
    /// </summary>
    /// <param name="c1"></param>
    /// <returns></returns>
    public static double getDistance(Color c1)
    {
       return Math.Sqrt(c1.r * c1.r + c1.g * c1.g + c1.b * c1.b);
    }
    /// <summary>
    /// Takes two images and returns the score difference between the two
    /// Also calculates an erroroverlay and saves it
    /// </summary>
    /// <param name="img1"></param>
    /// <param name="img2"></param>
    /// <returns></returns>
    public double Evaluate(string img1, string img2)
    {

        (Texture2D referenceTexture, Texture2D inputTexture) = this.LoadTextures(img1, img2);
        Texture2D output;

        //var t = CompareProx(referenceTexture, inputTexture, out output);
        var score = this.CompareBlur(referenceTexture, inputTexture, out output);


        this.CreateOverlay(output, out errorOverlay);

        SaveTexture(errorOverlay, "Assets/Resources/Images/Test/img1-overlay.png");

        return 100 / Math.Sqrt(score);

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
        Texture2D output;
        Debug.Log("Loaded Textures");

        //var t = CompareProx(referenceTexture, inputTexture, out output);
        var score = this.CompareBlur(referenceTexture, inputTexture, out output);
        Debug.Log("CompareBlur");


        this.CreateOverlay(output, out errorOverlay);
        Debug.Log("CreateOverlay");

        SaveTexture(output, "Assets/Resources/Images/Test/img1-2.png");
        SaveTexture(errorOverlay, "Assets/Resources/Images/Test/img1-overlay.png");
        Debug.Log("SaveTexture");

        return 100 / Math.Sqrt(score);

    }
    
    //GEORG
    //TODO:
    // get picture input. 
    //Convert picture to greyscale or compare individual color values 
    //add buffer? right now cuts off outer rows and columns
    //actually test this code kekW

    // KLEJDI: Maybe dont convert to greyscale, we want to compare color average
    // 1. Image size is wrong
    // 2. Change convolution size

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
        return LoadTextures("Images/Test/cur_duplicate", "Images/Test/cur_original");
        //LoadTextures("Images/Test/colorful1", "Images/Test/colorful2", "Assets/Resources/Images/Test/colorful3.jpg");
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
    /// Returns a read/write duplicate of the textures
    /// </summary>
    /// <param name="path1"></param>
    /// <param name="path2"></param>
    /// <returns></returns>
    public (Texture2D, Texture2D) LoadTextures(string path1, string path2) {
        var texture1 = ReadableDuplicate(Resources.Load<Texture2D>(path1));
        var texture2 = ReadableDuplicate(Resources.Load<Texture2D>(path2));
        return (texture1, texture2);
    }
    /// <summary>
    /// One method of doing the difference detection, using SSD (i think)
    /// </summary>
    /// <param name="pic1"></param>
    /// <param name="pic2"></param>
    /// <param name="picDiff">Overlay image consisting of the differences between the two</param>
    /// <returns></returns>
    double CompareProx(Texture2D pic1, Texture2D pic2, out Texture2D picDiff) {
        //filter size, higher number = less strict. could be done by percentage of image size
        int fRows = 13;
        int fColumns = 13;

        pic1 = BufferImage(pic1, fRows / 2, Color.white);
        pic2 = BufferImage(pic2, fRows / 2, Color.white);

        int rows = pic1.height; int columns = pic1.width;
    ;
        if (pic1.height != pic2.height || pic1.width != pic2.width) {
            picDiff = null;
            return -1;
        }

        // Reduced dimensionality, should be the best imo, but we might need padding
        Texture2D result1 = new Texture2D(rows - (fRows - 1),  columns - (fColumns - 1));
        picDiff = new Texture2D((rows - (fRows - 1)), (columns - (fColumns - 1)));


        for (int i = 0; i < rows - (fRows - 1); i++) {
            for (int j = 0; j < columns - (fColumns - 1); j++) {
                var accum1 = new Color(0.0f, 0.0f, 0.0f);
                accum1 = pic1.GetPixel(i , j);
                var accum2 = new Color(0.0f, 0.0f, 0.0f);
                accum2 = pic2.GetPixel(i, j);

                var diffColor = SimilarityDetection.abs(accum1 - accum2);
                var diff = ColorSum(SimilarityDetection.abs(accum1 - accum2));
                // Apply filter
                for (int fR = 0; fR < fRows; fR++) {                        
                    for (int fC = 0; fC < fColumns; fC++) {
                        accum2 = pic2.GetPixel(i + fR, j + fC);
                        if (diff > ColorSum(SimilarityDetection.abs(accum1 - accum2))) {
                            diffColor = SimilarityDetection.abs(accum1 - accum2);
                            diff = ColorSum(SimilarityDetection.abs(accum1 - accum2));
                        }
                       

                    }              
                }

                result1.SetPixel(i, j, diffColor);

            }
        }

        SimilarityDetection.SaveTexture(result1, "Assets/Resources/Images/Test/img1-temp1.jpg");


        double diffAccumulator = 0;

        for (int i = 0; i < columns; i++) 
        {
            for(int j = 0; j < rows; j++)
            {
                var diff = SimilarityDetection.abs(result1.GetPixel(i, j));

                picDiff.SetPixel(i, j, diff);
                diffAccumulator += Math.Sqrt(diff.r * diff.r + diff.g * diff.g + diff.b * diff.b);
            }
        }

        return diffAccumulator;
    }
   /// <summary>
   /// The other method of doing the difference detection, uses Convolution
   /// </summary>
   /// <param name="pic1"></param>
   /// <param name="pic2"></param>
   /// <param name="picDiff">Overlay image consisting of the differences between the two</param>
   /// <returns></returns>
    double CompareBlur(Texture2D pic1, Texture2D pic2, out Texture2D picDiff)   
    {

     
        if (pic1.height != pic2.height || pic1.width != pic2.width)
        {
            picDiff = null;
            return -1;
        }

        int[,] filter = new int[,] {
            {1, 1, 2, 2, 2, 2, 2,  1, 1, },
            {1, 2, 2, 3, 3, 3, 2,  2, 1, },
            {2, 2, 3, 4, 4, 4, 3, 2,  2, },
            {2, 3, 4, 5, 5, 5, 4, 3,  2, },

            {2, 3, 4, 5, 5, 5, 4, 3,  2, },

            {2, 3, 4, 5, 5, 5, 4, 3,  2, },
            {2, 2, 3, 4, 4, 4, 3, 2,  2, },
            {1, 2, 2, 3, 3, 3, 2,  2, 1, },
            {1, 1, 2, 2, 2, 2, 2,  1, 1, },
        };

        Debug.Log("First loop");

        //rows and columns of the filter

        // Must be nonempty
        int fRows = filter.GetLength(0);
        int fColumns = filter.GetLength(1);

        pic1 = BufferImage(pic1, fRows / 2, Color.white);
        pic2 = BufferImage(pic2, fColumns / 2, Color.white);

        int rows = pic1.height; int columns = pic1.width;
        
        int lengthWithWeights = 0;
        for (int i = 0; i < fRows; i++)
        {
            for(int j = 0; j < fColumns; j++)
            {
                lengthWithWeights += filter[i, j];
            }
        }
        Debug.Log("Load textures");

        picDiff = new Texture2D(pic1.height, pic1.width, TextureFormat.RGBAFloat, false);

        Debug.Log("Starting conv");

        Color[] pic1Pixels = pic1.GetPixels();
        Color[] pic2Pixels = pic2.GetPixels();
        int loop_rows=  rows - (fRows - 1);
        int loop_columns= columns - (fColumns - 1);
        Color accum;
        Debug.Log("Starting conv loop");
        for (int i = 0; i < loop_rows ; ++i)
        {
            for (int j = 0; j < loop_columns; ++j)
            {

                accum = new Color(0, 0, 0);
                // Apply filter
                for (int fR = 0; fR < fRows; ++fR)
                {
                    for (int fC = 0; fC < fColumns; ++fC)
                    {
                        accum += filter[fR, fC] * pic1Pixels[(i + fR) * columns + fC  + j];
                    }
                }


                pic1Pixels[i*columns + j] = accum / lengthWithWeights;

                accum = new Color(0, 0, 0);
                for (int fR = 0; fR < fRows; ++fR)
                {
                    for (int fC = 0; fC < fColumns; ++fC)
                    {
                        accum += filter[fR, fC] * pic2Pixels[(i + fR) * columns + fC + j];
                    }
                }

                pic2Pixels[i*columns + j] = accum / lengthWithWeights;
            }
        }
        Debug.Log("Ending conv");

        var v1 = new Texture2D(columns, rows, TextureFormat.RGBAFloat, false);
        v1.SetPixelData<Color>(pic1Pixels, 0);
        var v2 = new Texture2D(columns, rows, TextureFormat.RGBAFloat, false);
        v2.SetPixelData<Color>(pic2Pixels, 0);

        SimilarityDetection.SaveTexture(v1, "Assets/Resources/Images/Test/img1-temp1.jpg");
        SimilarityDetection.SaveTexture(v2, "Assets/Resources/Images/Test/img1-temp2.jpg");

        Debug.Log("Starting diff");

        double diffAccumulator = 0;
        int count1 = 0;
        int count2 = 0;
        Color[] picDiffPixels = picDiff.GetPixels();
        for (int i = 0; i < rows; ++i)
        {
            for (int j = 0; j < columns; ++j)
            {
                Color diff = SimilarityDetection.abs((pic1Pixels[i*columns + j] - pic2Pixels[i * columns + j]));
                double diffNumber = getDistance(diff);
                if (diffNumber < this.Threshold)
                {
                    count1++;
                    diff = new Color(0.0f, 0.0f, 0.0f);
                }
                else {
                    count2++;

                    Vector4 temp = diff;
                    var t1 = Mathf.Lerp((float)this.Threshold, 1.0f, (float)diffNumber);
                    diff = Vector4.Lerp(Vector4.zero, temp, t1);
                }
                picDiffPixels[i * columns + j] = diff;
                diffAccumulator += getDistance(diff);
            }
        }
        picDiff.SetPixelData<Color>(picDiffPixels, 0);
        SimilarityDetection.SaveTexture(picDiff, "Assets/Resources/Images/Test/img1-diff.jpg");

        Debug.Log("Ending diff");

        return diffAccumulator;
    }
    /// <summary>
    /// Creates an overlay image of red color, with varying transparency in regards to the error
    /// </summary>
    /// <param name="input"></param>
    /// <param name="error"></param>
    void CreateOverlay(Texture2D input, out Texture2D error)
    {
        int width = input.width;
        int height = input.height;
        error  = new Texture2D(width, height, TextureFormat.RGBAFloat, false);
        Color[] inputPixels = input.GetPixels();
        Color[] errorPixels = error.GetPixels();
        
        for (int i = 0; i < height; ++i)
        {
            for (int j = 0; j < width; ++j)
            {
                float remappedDistance = (float)SimilarityDetection.getDistance(inputPixels[i*width + j]);
                if(remappedDistance > 0.01)
                {
                    remappedDistance = (remappedDistance - (float)this.Threshold) / (1.0f - (float)this.Threshold);
                }
                inputPixels[i * width + j] = new Vector4(1.0f, 0.0f, 0.0f, remappedDistance);
            }
        }
        error.SetPixelData<Color>(inputPixels, 0);
    }
    /// <summary>
    /// Calculates the sum of the inner color values
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    float ColorSum(Color c) {
        return (c.r + c.g + c.b);
    }
    /// <summary>
    /// Buffer image to enable any filters
    /// </summary>
    /// <param name="img"></param>
    /// <param name="pixel">Pixels added on each side of the picture</param>
    /// <param name="color">Color the added color</param>
    /// <returns></returns>
    Texture2D BufferImage(Texture2D img, int pixel, Color color) {
        Debug.Log("Starting buffer");

        Texture2D bufImg = new Texture2D(img.height+pixel*2, img.width+pixel*2, TextureFormat.RGBAFloat, false);
        Color[] imgPixels = img.GetPixels();
        Color[] bufImgPixels = bufImg.GetPixels();

        for (int i = 0; i < bufImg.height; i++) {
            for (int j = 0; j < bufImg.width; j++) {
                if (i < pixel || j < pixel)
                {
                    bufImgPixels[i * bufImg.width + j] = color;
                }
                else if (i >= img.height + pixel || j >= img.width + pixel)
                {
                    bufImgPixels[i * bufImg.width + j] = color;
                }
                else {
                    bufImgPixels[i * bufImg.width + j] = imgPixels[(i - pixel) * img.width + j-pixel];
                }
            }
        }
        bufImg.SetPixelData<Color>(bufImgPixels, 0);
        SimilarityDetection.SaveTexture(bufImg, "Assets/Resources/Images/Test/buffered" + test + ".jpg");
        test = 1;
        Debug.Log("Ending buffer");

        return bufImg;
    }

}
