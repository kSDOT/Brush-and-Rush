using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;

namespace MarchingCubesProject
{


    public class MarchingImplementation : MonoBehaviour
    {
        GameObject MainCamera;
        /// <summary>
        /// The transform of sculpture we will copy
        /// </summary>
        public Transform ReferenceObjectTransform;
        /// <summary>
        /// Used if you want to save a sculpture to file (assets path is prepended)
        /// </summary>
        public string OutputFileName;
        /// <summary>
        /// Loads the reference scultpure from the file (assets path is prepended)
        /// </summary>
        public string InputFileName;
        /// <summary>
        /// Material used for sculptures
        /// </summary>
        public Material SculptureMaterial;
        /// <summary>
        /// Material used for the error overlay
        /// </summary>
        public Material ErrorMaterial;

        /// <summary>
        /// Used to construct the next voxel
        /// </summary>
        int meshIndex;

        /// <summary>
        /// Dimensions of the voxel grid
        /// </summary>
        public int width = 5;
        public int height = 5;
        public int depth = 5;

        /// <summary>
        /// Maps from voxelsize to real meters
        /// 1.0f -> 1 voxel unit = 1 meter 
        /// 10.f -> 1 voxel unit = 0.1 meter
        /// </summary>
        public float scaleDivision = 10.0f;

        /// <summary>
        /// Voxel grid (3d) with isovalues
        /// </summary>
        float[,,] voxels;
        /// <summary>
        /// Marching Tetrahedron
        /// </summary>
        MarchingTetrahedron marching;
        /// <summary>
        /// Constructs a cube
        /// </summary>
        [ContextMenu("TEST")]
        void CreateNew()
        {

            voxels = new float[width, height, depth];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        if (x == 0 || y == 0 || z == 0 || x == width-1 || y == height-1|| z == depth-1)
                            voxels[x, y, z] = -1f;
                        else
                            voxels[x, y, z] = 1f;
                    }
                }
            }

            List<Vector3> verts = new List<Vector3>();
            List<int> indices = new List<int>();

            //The mesh produced is not optimal. There is one vert for each index.
            //Would need to weld vertices for better quality mesh.
            marching.Generate(voxels, verts, indices);
            var position = new Vector3(0, 0, 0);
            CreateMesh(verts, indices, position, false);

        }

        [ContextMenu("Save to file")]
        void SaveToFile() {
            FileStream filestream = new FileStream(Application.dataPath + "\\" + this.OutputFileName, FileMode.Create);
            var streamwriter = new StreamWriter(filestream);
            streamwriter.AutoFlush = true;
            Console.SetOut(streamwriter);
            Console.SetError(streamwriter);


            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    for (int dep = 0; dep < this.depth; dep++) {

                        if (dep != this.depth - 1 || col != width - 1) { Console.Write(String.Format("{0} ", voxels[row, col, dep])); }
                        else { 
                            Console.Write(String.Format("{0}", voxels[row, col, dep])); 
                        }
                    }
                }
                if (row != height - 1) { Console.WriteLine(); }
            }
            streamwriter.Flush();
            streamwriter.Close();
            Console.SetOut(null);
            Console.SetError(null);
        }
        void LoadReferenceFromFile(string FileName)
        {
            String input = File.ReadAllText(Application.dataPath + "\\" + FileName);
            int i = 0, j = 0, k = 0;
            var voxels = new float[width, height, depth];

            // Load values from file into voxelarray
            foreach (var row in input.Split('\n'))
            {
                j = 0;
                foreach (var col in row.Trim().Split(' '))
                {
                    voxels[i, j, k] = int.Parse(col.Trim());
                    k++;
                    if (k == depth)
                    {
                        k = 0;
                        j++;
                    }
                }
                i++;
            }


            List<Vector3> verts = new List<Vector3>();
            List<int> indices = new List<int>();

            marching.Generate(voxels, verts, indices);


            var position = new Vector3(0, 0, 0);

          //  CreateMesh(vertsArray, indices.ToArray(), position, true);

        }
        /// <summary>
        /// Updates the sculpture with the modified isovalues
        /// </summary>
        void UpdateValues()
        {
            List<Vector3> verts = new List<Vector3>();
            List<int> indices = new List<int>();
            marching.Generate(voxels, verts, indices);

            var position = new Vector3(0, 0, 0);
            CreateMesh(verts, indices, position, false);
        }

        double Compare(float[,,] other, GameObject parent)
        {
            float errorAccumulator = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        this.voxels[x, y, z] = -other[x, y, z] * this.voxels[x, y, z];
                        if (this.voxels[x, y, z] == -1) { 
                            errorAccumulator++;
                        }
                    }
                }
            }

            List<Vector3> verts = new List<Vector3>();
            List<int> indices = new List<int>();
            marching.Generate(voxels, verts, indices);

            Vector3[] vertsArray = verts.ToArray();
            int length = vertsArray.Length;

           // CreateMesh(vertsArray, indices.ToArray(), parent.transform.position, false);

            // make sure its in [0, 100] range, using 1 decimal digits
            return Mathf.Clamp(Mathf.Round(errorAccumulator/(width * height * depth)* 10.0f) * 0.1f, 0, 100);
        }

        private void CreateMesh(List<Vector3> verts, List<int> indices, Vector3 position, bool ReferenceMesh=false)
        {
            Mesh mesh = new Mesh();
            mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(verts);
            mesh.SetTriangles(indices, 0);
            mesh.RecalculateNormals();

            //mesh.RecalculateBounds();
            GameObject go;
            //if (!ReferenceMesh)
            //{
            //    if (this.meshIndex != 0)
            //    {
            //        Destroy(GameObject.Find("Mesh"+(meshIndex-1)));
            //    }

            //    go = new GameObject("Mesh"+this.meshIndex++);
            //    go.tag = "Cube";
            //    go.transform.localPosition = position;
            //    go.transform.parent = transform;

            //}
            //else
            //{
            //    go = new GameObject("Reference");
            //    go.transform.parent = this.ReferenceObjectTransform;

            //}
            if (this.meshIndex != 0)
            {
                Destroy(GameObject.Find("Mesh" + (meshIndex - 1)));
            }

            go = new GameObject("Mesh" + this.meshIndex++);
            go.tag = "Cube";
            go.transform.localPosition = position;
            go.transform.parent = transform;
            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            go.GetComponent<Renderer>().material = SculptureMaterial;
            go.GetComponent<MeshFilter>().mesh = mesh;
            go.AddComponent<MeshCollider>();

        }
       
        private void Start()
        {
            this.meshIndex = 0;
            this.MainCamera = GameObject.Find("Main Camera");

            this.marching = new MarchingTetrahedron(this.scaleDivision);
            //Surface is the value that represents the surface of mesh
            //For example the perlin noise has a range of -1 to 1 so the mid point is where we want the surface to cut through.
            //The target value does not have to be the mid point it can be any value with in the range.
            marching.Surface = 0.0f;

            //this.LoadReferenceFromFile(this.OutputFileName);
        }
        private void Update()
        {
            // todo: replace with controller
            #region CameraControll
            if (Input.GetKeyDown("w")) { 
                this.MainCamera.transform.position = 
                    new Vector3(this.MainCamera.transform.position.x, this.MainCamera.transform.position.y + 0.1f,
                    this.MainCamera.transform.position.z
                    );
            }
            else if (Input.GetKeyDown("d"))
            {
                this.MainCamera.transform.position =
                    new Vector3(this.MainCamera.transform.position.x + 0.1f, this.MainCamera.transform.position.y,
                    this.MainCamera.transform.position.z);
            }
            #endregion
            RaycastHit hit;
            if (Input.GetMouseButtonDown(0)
                && Physics.Raycast(this.MainCamera.transform.position, this.MainCamera.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity)
                && hit.collider.gameObject.CompareTag("Cube"))
            {
                MeshCollider meshCollider = hit.collider as MeshCollider;

                if (meshCollider == null || meshCollider.sharedMesh == null)
                    return;

                Debug.DrawRay(this.MainCamera.transform.position, this.MainCamera.transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
                var hitRelative = AbsDistance(hit.point, this.transform.position) * scaleDivision;

                Debug.Log("Original: " + hit.point);
                Debug.Log("start: " + DateTimeOffset.Now.ToUnixTimeMilliseconds());

                modify(hitRelative);
                UpdateValues();
                Debug.Log("finished: " + DateTimeOffset.Now.ToUnixTimeMilliseconds());

            }
            else
            {
                Debug.DrawRay(this.MainCamera.transform.position, this.MainCamera.transform.TransformDirection(Vector3.forward) * 1000, Color.white);
                Debug.Log("Did not Hit");
            }
          
        }
        /// <summary>
        /// Modifies isovalues in a 3x3x3 cube around the hitpoint, setting them all to -1
        /// </summary>
        /// <param name="hitRelative"></param>
        private void modify(Vector3 hitRelative)
        {
            int x = Mathf.RoundToInt(hitRelative.x);
            int y = Mathf.RoundToInt(hitRelative.y);
            int z = Mathf.RoundToInt(hitRelative.z);
            Debug.Log("hit: " + new Vector3(x, y, z));

            for (int i = -1; i <= 1; ++i)
            {
                for (int j = -1; j <= 1; ++j)
                {
                    for (int k = -1; k <= 1; ++k)
                    {
                        int x_2 = x + i; 
                        int y_2 = y + j; 
                        int z_2 = z + k;
                        if(x_2 > 0 && x_2 < width && y_2 > 0 && y_2 < height && z_2 > 0 && z_2 < depth)
                        {
                            voxels[x_2, y_2, z_2] = -1;
                        }    
                    }

                }
            }
        }
        /// <summary>
        /// Substracts two vectors and gives back the absolute result
        /// </summary>
        /// <param name="vec1"></param>
        /// <param name="vec2"></param>
        /// <returns></returns>
        private Vector3 AbsDistance(Vector3 vec1, Vector3 vec2)
        {
            return new Vector3(Mathf.Abs(vec1.x - vec2.x), Mathf.Abs(vec1.y - vec2.y), Mathf.Abs(vec1.z - vec2.z));
        }

    }

}