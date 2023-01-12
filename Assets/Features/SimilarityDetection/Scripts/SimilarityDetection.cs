using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class SimilarityDetection : MonoBehaviour
{
    public double Threshold = 40;
    Texture2D? errorOverlay;
    public static Color abs(Color c1)
    {
        c1.r = Mathf.Abs(c1.r);
        c1.g = Mathf.Abs(c1.g);
        c1.b = Mathf.Abs(c1.b);
        c1.a = Mathf.Abs(c1.a);

        return c1;
    }
    public static double getDistance(Color c1)
    {
       return Math.Sqrt(c1.r * c1.r + c1.g * c1.g + c1.b * c1.b);
    }
    // Start is called before the first frame update
    void Start()
    {
    }
    public double Evaluate(string img1, string img2)
    {

        (Texture2D referenceTexture, Texture2D inputTexture) = this.LoadTextures(img1, img2);
        Texture2D output;

        //var t = CompareProx(referenceTexture, inputTexture, out output);
        var score = this.CompareBlur(referenceTexture, inputTexture, out output);

     
        this.CreateOverlay(output, out errorOverlay);

        SaveTexture(output, "Assets/Resources/Images/Test/img1-2.png");
        SaveTexture(errorOverlay, "Assets/Resources/Images/Test/img1-overlay.png");

        return 100 / Math.Sqrt(score);

    }
    // Update is called once per frame
    void Update()
    {
        
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


    private Texture2D ReadableDuplicate(Texture2D source)
    {
        var pix = source.GetRawTextureData();
        var readableText = new Texture2D(source.width, source.height, source.format, false);
        readableText.LoadRawTextureData(pix);
        readableText.Apply();
        return readableText;
    }
    private (Texture2D, Texture2D) LoadTextures()
    {
        return LoadTextures("{0}/Features/SimilarityDetection/Resources/Images/screenshots/cur_duplicate.png",
            "{0}/Features/SimilarityDetection/Resources/Images/screenshots/cur_original.png");
       // return LoadTextures("Images/Test/img1", "Images/Test/img1-1");
        //LoadTextures("Images/Test/colorful1", "Images/Test/colorful2", "Assets/Resources/Images/Test/colorful3.jpg");
    }

    private static void SaveTexture(Texture2D t, string s)
    {
        byte[] temp = t.EncodeToPNG();
        File.WriteAllBytes(s, temp);
    }


    public (Texture2D, Texture2D) LoadTextures(string path1, string path2) {
        Debug.Log(path1.ToString());
        Debug.Log(path2.ToString());
        var texture1 = ReadableDuplicate(Resources.Load<Texture2D>(path1));
        Debug.Log(texture1);
        var texture2 = ReadableDuplicate(Resources.Load<Texture2D>(path2));
        Debug.Log(texture2);
        return (texture1, texture2);
    }

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
    double CompareBlur(Texture2D pic1, Texture2D pic2, out Texture2D picDiff)   
    {

        int rows = pic1.height; int columns = pic1.width;
        ;
        if (pic1.height != pic2.height || pic1.width != pic2.width)
        {
            picDiff = null;
            return -1;
        }

        /*    int[,] filter = new int[,] {
                { 0, 1, 1, 1, 0, },

                { 1, 2, 2, 2, 1, },


                { 1, 2, 3, 2, 1, },

                { 1, 2, 2, 2, 1, },

                { 0, 1, 1, 1, 0, },
            };

            */
        //int[,] filter = new int[,] { { 1 } };
        /*   int[,] filter = new int[,] {
            {  1, 1, 1,  },
             { 1, 1, 1,  },
              {  1, 1, 1  },
           }; */
            int[,] filter = new int[,] {
                {1, 1, 1, 2, 2, 2, 2, 2,  1, 1, 1, },
                {1, 1, 2, 2, 3, 3, 3, 2,  2, 1, 1, },
                {1, 2, 2, 2, 3, 3, 3, 2,  2, 2, 1, },
                {2, 2, 2, 3, 4, 4, 4, 3, 2,  2, 2, },
                {2, 3, 3, 4, 5, 5, 5, 4, 3,  3, 2, },

                {2, 3, 3, 4, 5, 5, 5, 4, 3,  3, 2, },

                {2, 3, 3, 4, 5, 5, 5, 4, 3,  3, 2, },
                {2, 2, 2, 3, 4, 4, 4, 3, 2,  2, 2, },
                {1, 2, 2, 2, 3, 3, 3, 2,  2, 2, 1, },
                {1, 1, 2, 2, 3, 3, 3, 2,  2, 1, 1, },
                {1, 1, 1, 2, 2, 2, 2, 2,  1, 1, 1, },


            };
        
        //rows and columns of the filter

        // Must be nonempty
        int fRows = filter.GetLength(0);
        int fColumns = filter.GetLength(1);
        int fLength = filter.Length;

        int lengthWithWeights = 0;
        for (int i = 0; i < fRows; i++)
        {
            for(int j = 0; j < fColumns; j++)
            {
                lengthWithWeights += filter[i, j];
            }
        }

        // Reduced dimensionality, should be the best imo, but we might need padding
        Texture2D result1 = new Texture2D(rows - (fRows - 1), columns - (fColumns - 1));
        Texture2D result2 = new Texture2D(rows - (fRows - 1), columns - (fColumns - 1));
        picDiff = new Texture2D((rows - (fRows - 1)), (columns - (fColumns - 1)));


        for (int i = 0; i < rows - (fRows - 1); i++)
        {
            for (int j = 0; j < columns - (fColumns - 1); j++)
            {
                var accum1 = new Color(0.0f, 0.0f, 0.0f);

                var accum2 = new Color(0.0f, 0.0f, 0.0f);

                // Apply filter
                for (int fR = 0; fR < fRows; fR++)
                {
                    for (int fC = 0; fC < fColumns; fC++)
                    {
                        //!!! Why is this applying the filter to both images� It should compare one pixel of the original to the filtered area of the copy!!!
                        accum1 += filter[fR, fC] * pic1.GetPixel(i + fR, j + fC);
                        accum2 += filter[fR, fC] * pic2.GetPixel(i + fR, j + fC);

                    }
                }

                result1.SetPixel(i, j, accum1 / lengthWithWeights);
                result2.SetPixel(i, j, accum2 / lengthWithWeights);
            }
        }

        SimilarityDetection.SaveTexture(result1, "Assets/Resources/Images/Test/img1-temp1.jpg");
        SimilarityDetection.SaveTexture(result2, "Assets/Resources/Images/Test/img1-temp2.jpg");

        double diffAccumulator = 0;
        int count1 = 0;
        int count2 = 0;
        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                Color diff = SimilarityDetection.abs((result1.GetPixel(i, j) - result2.GetPixel(i, j)));
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
                picDiff.SetPixel(i, j, diff);
                diffAccumulator += getDistance(diff);
            }
        }
        Debug.Log("Count1: " + count1);
        Debug.Log("Count2: " + count2);
        return diffAccumulator;
    }

    void CreateOverlay(Texture2D input, out Texture2D error)
    {
        error  = new Texture2D(input.width, input.height);
        for (int i = 0; i < input.height; i++)
        {
            for (int j = 0; j < input.width; j++)
            {

                float remappedDistance = (float)SimilarityDetection.getDistance(input.GetPixel(i, j));
                if(remappedDistance > 0.01)
                {
                    remappedDistance = (remappedDistance - (float)this.Threshold) / (1.0f - (float)this.Threshold);
                }
                error.SetPixel(i, j, new Vector4(1.0f, 0.0f, 0.0f, remappedDistance));
            }
        }
    }

    float ColorSum(Color c) {
        return (c.r + c.g + c.b);
    }

    //pixel: pixels added on each side of the picture; color the added color. white canvas should get buffered in white
    Texture2D BufferImage(Texture2D img, int pixel, Color color) {
        Texture2D bufImg = new Texture2D(img.height+pixel*2, img.width+pixel*2);

        for (int i = 0; i < bufImg.height; i++) {
            for (int j = 0; j < bufImg.width; j++) {
                if (i < pixel || j < pixel)
                {
                    bufImg.SetPixel(i, j, color);
                }
                else if (i >= img.height + pixel || j >= img.width + pixel)
                {
                    bufImg.SetPixel(i, j, color);
                }
                else {
                    bufImg.SetPixel(i, j, img.GetPixel(i - pixel, j - pixel));
                }
            }
        }

        return bufImg;
    }

}
