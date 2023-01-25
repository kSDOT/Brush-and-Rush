
using System;
using System.Collections;
using UnityEngine;

namespace MarchingCubesProject
{ 
    /// <summary>
    /// A concrete class for getting 3d meshes
    /// </summary>
    public class Object3D
    {

        public Vector3 Offset { get; set; }

        public Object3D()
        {
            Offset = Vector3.zero;
        }
        /// <summary>
        /// Samples a 3D fractal.
        /// </summary>
        /// <param name="x">A value on the x axis.</param>
        /// <param name="y">A value on the y axis.</param>
        /// <param name="z">A value on the z axis.</param>
        /// <returns>A noise value between -Amp and Amp.</returns>
        public virtual float Sample3D(float x, float y, float z)
            {
                x = x + Offset.x;
                y = y + Offset.y;
                z = z + Offset.z;

            if (x == 0 || y == 0 || z == 0 || x == 1 || y == 1 || z == 1)
                return -1;
            else
                return 1f;
        }

        }

}