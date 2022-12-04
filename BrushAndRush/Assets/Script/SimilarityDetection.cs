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
    //TODO:
    // get picture input. 
    //Convert picture to greyscale or compare individual color values
    //add buffer? right now cuts off outer rows and columns
    //actually test this code kekW
    int Compare(int[] pic1, int[] pic2, int rows, int columns) {
        if (pic1.Length != rows * columns || pic2.Length != rows * columns) {
            return -1;
        }
        int[] filter = { 0, 1, 1, 1, 0,
                         1, 2, 2, 2, 1,
                         1, 2, 3, 2, 1,
                         1, 2, 2, 2, 1,
                         0, 1, 1, 1, 0};
        //rows and columns of the filter
        int fRows = 5;
        int fColumns = 5;

        int[] result1 = new int[(rows - (fRows - 1)) * (columns - (fColumns - 1))];
        int[] result2 = new int[(rows - (fRows - 1)) * (columns - (fColumns - 1))];

        for (int i = 0; i < rows - ((fRows - 1)); i++) {
            for (int j = 0; j < columns - ((fColumns - 1)); j++) {
                result1[i + j * rows] = 0;  //not sure if initialisation is needed, probably not
                result2[i + j * rows] = 0;
                for (int fR = 0; fR < fRows; fR++) {
                    for (int fC = 0; fC < fColumns; fC++) {
                        result1[i + j * rows] += filter[fR + fC * fRows] * pic1[i + fR + j * rows + fC * fRows];
                        result2[i + j * rows] += filter[fR + fC * fRows] * pic2[i + fR + j * rows + fC * fRows];
                    }
                }

                
            }
        
        }
        int picDiff = 0;
        for (int i = 0; i < result1.Length; i++) {
           picDiff+= Mathf.Abs(result1[i] - result2[i]);
        }
        return picDiff;
    }

}
