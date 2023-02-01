using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class UndoRedo : MonoBehaviour
{
    //----------Variables----------
    //---Serialized Variables---
    [SerializeField]
    [MinValue(0)]
    private int numberOfUndos;
    [SerializeField]
    [MinValue(0)]
    private int numberOfRedos;

    //--------Methods----------

}
