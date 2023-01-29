using UnityEngine;
public class VoxelArray : MonoBehaviour
{

    /// <summary>
    /// Create a new voxel array.
    /// </summary>
    /// <param name="width">The size of the voxels on the x axis.</param>
    /// <param name="height">The size of the voxels on the y axis.</param>
    /// <param name="depth">The size of the voxels on the z axis.</param>
    public VoxelArray(int width, int height, int depth)
    {
        Voxels = new float[width, height, depth];
    }

    /// <summary>
    /// The size of the voxels on the x axis.
    /// </summary>
    public int Width => Voxels.GetLength(0);

    /// <summary>
    /// The size of the voxels on the y axis.
    /// </summary>
    public int Height => Voxels.GetLength(1);

    /// <summary>
    /// The size of the voxels on the z axis.
    /// </summary>
    public int Depth => Voxels.GetLength(2);

    /// <summary>
    /// Get/set the voxel data.
    /// </summary>
    /// <param name="x">The index on the x axis.</param>
    /// <param name="y">The index on the y axis.</param>
    /// <param name="z">The index on the z axis.</param>
    /// <returns>The voxels data.</returns>
    public float this[int x, int y, int z]
    {
        get { return Voxels[x, y, z]; }
        set { Voxels[x, y, z] = value; }
    }

    /// <summary>
    /// THe voxel data.
    /// </summary>
    public float[,,] Voxels { get; set; }

   
}
