/// credits to https://github.com/Scrawk/Marching-Cubes for providing the tetrahedron implementation
using System.Collections.Generic;

using UnityEngine;

namespace MarchingCubesProject
{
    public class MarchingTetrahedron : MonoBehaviour
    {

        private Vector3[] EdgeVertex { get; set; }

        private Vector3[] CubePosition { get; set; }

        private Vector3[] TetrahedronPosition { get; set; }

        private float[] TetrahedronValue { get; set; }

        private float Scale;

        /// <summary>
        /// TetrahedronEdgeConnection lists the index of the endpoint vertices for each of the 6 edges of the tetrahedron.
        /// tetrahedronEdgeConnection[6][2]
        /// </summary>
        private static readonly int[,] TetrahedronEdgeConnection = new int[,]
        {
            {0,1},  {1,2},  {2,0},  {0,3},  {1,3},  {2,3}
        };

        /// <summary>
        /// TetrahedronEdgeConnection lists the index of verticies from a cube 
        /// that made up each of the six tetrahedrons within the cube.
        /// tetrahedronsInACube[6][4]
        /// </summary>
        private static readonly int[,] TetrahedronsInACube = new int[,]
        {
            {0,5,1,6},
            {0,1,2,6},
            {0,2,3,6},
            {0,3,7,6},
            {0,7,4,6},
            {0,4,5,6}
        };

        /// <summary>
        /// For any edge, if one vertex is inside of the surface and the other is outside of 
        /// the surface then the edge intersects the surface
        /// For each of the 4 vertices of the tetrahedron can be two possible states, 
        /// either inside or outside of the surface
        /// For any tetrahedron the are 2^4=16 possible sets of vertex states.
        /// This table lists the edges intersected by the surface for all 16 possible vertex states.
        /// There are 6 edges.  For each entry in the table, if edge #n is intersected, then bit #n is set to 1.
        /// tetrahedronEdgeFlags[16]
        /// </summary>
        private static readonly int[] TetrahedronEdgeFlags = new int[]
        {
            0x00, 0x0d, 0x13, 0x1e, 0x26, 0x2b, 0x35, 0x38, 0x38, 0x35, 0x2b, 0x26, 0x1e, 0x13, 0x0d, 0x00
        };

        /// <summary>
        /// For each of the possible vertex states listed in tetrahedronEdgeFlags there
        /// is a specific triangulation of the edge intersection points.  
        /// TetrahedronTriangles lists all of them in the form of 0-2 edge triples 
        /// with the list terminated by the invalid value -1.
        /// tetrahedronTriangles[16][7]
        /// </summary>
        private static readonly int[,] TetrahedronTriangles = new int[,]
        {
            {-1, -1, -1, -1, -1, -1, -1},
            { 0,  3,  2, -1, -1, -1, -1},
            { 0,  1,  4, -1, -1, -1, -1},
            { 1,  4,  2,  2,  4,  3, -1},

            { 1,  2,  5, -1, -1, -1, -1},
            { 0,  3,  5,  0,  5,  1, -1},
            { 0,  2,  5,  0,  5,  4, -1},
            { 5,  4,  3, -1, -1, -1, -1},

            { 3,  4,  5, -1, -1, -1, -1},
            { 4,  5,  0,  5,  2,  0, -1},
            { 1,  5,  0,  5,  3,  0, -1},
            { 5,  2,  1, -1, -1, -1, -1},

            { 3,  4,  2,  2,  4,  1, -1},
            { 4,  1,  0, -1, -1, -1, -1},
            { 2,  3,  0, -1, -1, -1, -1},
            {-1, -1, -1, -1, -1, -1, -1}
        };

        /// <summary>
        /// The surface value in the voxels. Normally set to 0. 
        /// </summary>
        public float Surface { get; set; }


        /// <summary>
        /// Winding order of triangles use 2,1,0 or 0,1,2
        /// </summary>
        protected int[] WindingOrder;


        public MarchingTetrahedron(float scale = 1.0f, float surface = 0.0f)
        {
            Scale = scale;
            Surface = surface;
            Cube = new float[8];
            WindingOrder = new int[] { 0, 1, 2 };
            EdgeVertex = new Vector3[6];
            CubePosition = new Vector3[8];
            TetrahedronPosition = new Vector3[4];
            TetrahedronValue = new float[4];
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="voxels"></param>
        /// <param name="verts"></param>
        /// <param name="indices"></param>
        public void Generate(float[,,] voxels, IList<Vector3> verts, IList<int> indices)
        {

            int width = voxels.GetLength(0);
            int height = voxels.GetLength(1);
            int depth = voxels.GetLength(2);

            int x, y, z, i;
            int ix, iy, iz;
            for (x = 0; x < width - 1; x++)
            {
                for (y = 0; y < height - 1; y++)
                {
                    for (z = 0; z < depth - 1; z++)
                    {
                        //Get the values in the 8 neighbours which make up a cube
                        for (i = 0; i < 8; i++)
                        {
                            ix = x + VertexOffset[i, 0];
                            iy = y + VertexOffset[i, 1];
                            iz = z + VertexOffset[i, 2];

                            Cube[i] = voxels[ix, iy, iz];
                        }

                        //Perform algorithm
                        March(x, y, z, Cube, verts, indices);
                    }
                }
            }

        }

        void March(float x, float y, float z, float[] cube, IList<Vector3> vertList, IList<int> indexList)
        {
            int i, j, vertexInACube;

            //Make a local copy of the cube's corner positions
            for (i = 0; i < 8; i++)
            {
                CubePosition[i].x = x + VertexOffset[i, 0];
                CubePosition[i].y = y + VertexOffset[i, 1];
                CubePosition[i].z = z + VertexOffset[i, 2];
            }

            for (i = 0; i < 6; i++)
            {
                for (j = 0; j < 4; j++)
                {
                    vertexInACube = TetrahedronsInACube[i, j];
                    TetrahedronPosition[j] = CubePosition[vertexInACube];
                    TetrahedronValue[j] = cube[vertexInACube];
                }

                MarchTetrahedron(vertList, indexList);
            }
        }

        /// <summary>
        /// MarchTetrahedron performs the Marching Tetrahedrons algorithm on a single tetrahedron
        /// </summary>
        private void MarchTetrahedron(IList<Vector3> vertList, IList<int> indexList)
        {
            int i, j, vert, vert0, vert1, idx;
            int flagIndex = 0, edgeFlags;
            float offset, invOffset;

            //Find which vertices are inside of the surface and which are outside
            for (i = 0; i < 4; i++) if (TetrahedronValue[i] <= Surface) flagIndex |= 1 << i;

            //Find which edges are intersected by the surface
            edgeFlags = TetrahedronEdgeFlags[flagIndex];

            //If the tetrahedron is entirely inside or outside of the surface, then there will be no intersections
            if (edgeFlags == 0) return;

            //Find the point of intersection of the surface with each edge
            for (i = 0; i < 6; i++)
            {
                //if there is an intersection on this edge
                if ((edgeFlags & (1 << i)) != 0)
                {
                    vert0 = TetrahedronEdgeConnection[i, 0];
                    vert1 = TetrahedronEdgeConnection[i, 1];
                    offset = GetOffset(TetrahedronValue[vert0], TetrahedronValue[vert1]);
                    invOffset = 1.0f - offset;

                    EdgeVertex[i].x = invOffset * TetrahedronPosition[vert0].x + offset * TetrahedronPosition[vert1].x;
                    EdgeVertex[i].y = invOffset * TetrahedronPosition[vert0].y + offset * TetrahedronPosition[vert1].y;
                    EdgeVertex[i].z = invOffset * TetrahedronPosition[vert0].z + offset * TetrahedronPosition[vert1].z;
                }
            }

            //Save the triangles that were found. There can be up to 2 per tetrahedron
            for (i = 0; i < 2; i++)
            {
                if (TetrahedronTriangles[flagIndex, 3 * i] < 0) break;

                idx = vertList.Count;

                for (j = 0; j < 3; j++)
                {
                    vert = TetrahedronTriangles[flagIndex, 3 * i + j];
                    indexList.Add(idx + WindingOrder[j]);
                    vertList.Add(EdgeVertex[vert]/Scale);
                }
            }
        }

        /// <summary>
        /// GetOffset finds the approximate point of intersection of the surface
        /// between two points with the values v1 and v2
        /// </summary>
        protected virtual float GetOffset(float v1, float v2)
        {
            float delta = v2 - v1;
            return (delta == 0.0f) ? Surface : (Surface - v1) / delta;
        }


        /// <summary>
        /// VertexOffset lists the positions, relative to vertex0, 
        /// of each of the 8 vertices of a cube.
        /// vertexOffset[8][3]
        /// </summary>
        protected static readonly int[,] VertexOffset = new int[,]
        {
            {0, 0, 0},{1, 0, 0},{1, 1, 0},{0, 1, 0},
            {0, 0, 1},{1, 0, 1},{1, 1, 1},{0, 1, 1}
        };
        private float[] Cube { get; set; }


    }

}
