using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimilarityDetection : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //GEORD
    //TODO:
    // get picture input. 
    //Convert picture to greyscale or compare individual color values 
    //add buffer? right now cuts off outer rows and columns
    //actually test this code kekW

    //KLEJDI: Maybe dont convert to greyscale, we want to compare color average

    int Compare(Texture2D[] pic1, Texture2D[] pic2, out Texture2D[] picDiff) {
        int rows = pic1.width, int columns = pic1.height, int length = rows*columns;

        if (pic1.height != pic2.height || pic1.width != pic2.width) {
            return -1;
        }

        Color[,] filter = new Color[] { 
            0, 1, 1, 1, 0,
                         
            1, 2, 2, 2, 1,
                         
            1, 2, 3, 2, 1,
                         
            1, 2, 2, 2, 1,
                         
            0, 1, 1, 1, 0
        };

        //rows and columns of the filter
        // Must be rectangluar and nonempty
        int fRows = filter.Length;
        int fColumns = filter[0].Length;


        // Reduced dimensionality, should be the best imo, but we might need padding
        Texture2D result1 = new Texture2D((rows - (fRows - 1)),  (columns - (fColumns - 1)));
        Texture2D result2 = new Texture2D((rows - (fRows - 1)),  (columns - (fColumns - 1)));

        for (int i = 0; i < rows - (fRows - 1); i++) {
            for (int j = 0; j < columns - (fColumns - 1); j++) {

              //  result1[i + j * rows] = 0;  //not sure if initialisation is needed, probably not
              //  result2[i + j * rows] = 0;

                for (int fR = 0; fR < fRows; fR++) {
                    for (int fC = 0; fC < fColumns; fC++) {

                        result1.SetPixel(i, j, filter[fR, fC] * pic1.GetPixel(i + fR, fC));
                        result2.SetPixel(i, j, filter[fR, fC] * pic1.GetPixel(i + fR, fC));


                    }
                }

                
            }
        }

        int diffAccumulator = 0;

        for (int i = 0; i < height; i++) 
        {
            for(int j = 0; j < width; j++)
            {
                var diff = Mathf.Abs(result1[i] - result2[i]);
                picDiff.SetPixel(i, j, diff);
                diffAccumulator += diff;
            }
        }

        return diffAccumulator;
    }

}
