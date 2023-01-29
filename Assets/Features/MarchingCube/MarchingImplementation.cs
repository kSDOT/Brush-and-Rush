using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using ManagedCuda;
using System.Linq;

namespace MarchingCubesProject
{


    public class MarchingImplementation : MonoBehaviour
    {
        GameObject MainCamera;
        public Transform ReferenceObjectTransform;
        public string OutputFileName;
        public string InputFileName;
        public Material material;

        int meshIndex;

        /// The size of voxel array.
        public int width = 5;
        public int height = 5;
        public int depth = 5;

        public float scaleDivision = 10.0f;

        VoxelArray voxels;
        MarchingTetrahedron marching;
        [ContextMenu("TEST")]
        void CreateNew()
        {
            voxels = new VoxelArray(width, height, depth);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        float u = x / (width - 1.0f);
                        float v = y / (height - 1.0f);
                        float w = z / (depth - 1.0f);

                        voxels[x, y, z] = this.Sample3D(u, v, w);
                    }
                }
            }

            List<Vector3> verts = new List<Vector3>();
            List<int> indices = new List<int>();

            //The mesh produced is not optimal. There is one vert for each index.
            //Would need to weld vertices for better quality mesh.
            marching.Generate(voxels.Voxels, verts, indices);

            d_Voxels = voxels.Voxels.Cast<float>().ToArray();

            Vector3[] vertsArray = verts.ToArray();
            int length = vertsArray.Length;

            CudaDeviceVariable<Vector3> d_vertecies = vertsArray;
            CudaDeviceVariable<Vector3> d_normals = new CudaDeviceVariable<Vector3>(length);

            CudaNormal.BlockDimensions = 512;
            CudaNormal.GridDimensions = (length + 511) / 512;

            CudaNormal.Run(length, width, height, depth, d_vertecies.DevicePointer, d_Voxels.DevicePointer, d_normals.DevicePointer);
            ctx.Synchronize();
            Vector3[] normals = d_normals;
            d_normals.Dispose();
            d_vertecies.Dispose();

            var position = new Vector3(0, 0, 0);

            CreateMesh(vertsArray, normals, indices.ToArray(), position);

        }

        [ContextMenu("Save to file")]
        void SaveToFile() {
            FileStream filestream = new FileStream(Application.dataPath + "\\" + this.OutputFileName, FileMode.Create);
            var streamwriter = new StreamWriter(filestream);
            streamwriter.AutoFlush = true;
            Console.SetOut(streamwriter);
            Console.SetError(streamwriter);

            System.Text.StringBuilder output = new System.Text.StringBuilder("");

            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    for (int dep = 0; dep < this.depth; dep++) {

                        if (dep != this.depth - 1 && col != width - 1) { Console.Write(String.Format("{0} ", voxels[row, col, dep])); }
                        else { Console.Write(String.Format("{0}", voxels[row, col, dep])); }
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
            var voxels = new VoxelArray(width, height, depth);

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

            marching.Generate(voxels.Voxels, verts, indices);

            d_Voxels = voxels.Voxels.Cast<float>().ToArray();

            Vector3[] vertsArray = verts.ToArray();
            int length = vertsArray.Length;

            CudaDeviceVariable<Vector3> d_vertecies = vertsArray;
            CudaDeviceVariable<Vector3> d_normals = new CudaDeviceVariable<Vector3>(length);

            CudaNormal.BlockDimensions = 512;
            CudaNormal.GridDimensions = (length + 511) / 512;

            CudaNormal.Run(length, width, height, depth, d_vertecies.DevicePointer, d_Voxels.DevicePointer, d_normals.DevicePointer);
            ctx.Synchronize();
            Vector3[] normals = d_normals;
            d_normals.Dispose();
            d_vertecies.Dispose();

            var position = new Vector3(0, 0, 0);

            CreateMesh(vertsArray, normals, indices.ToArray(), position, true);

        }
        void UpdateValues()
        {
            List<Vector3> verts = new List<Vector3>();
            List<int> indices = new List<int>();
            marching.Generate(voxels.Voxels, verts, indices);

            d_Voxels = voxels.Voxels.Cast<float>().ToArray();
            Vector3[] vertsArray = verts.ToArray();
            int length = vertsArray.Length;

            CudaDeviceVariable<Vector3> d_vertecies = vertsArray;
            CudaDeviceVariable<Vector3> d_normals = new CudaDeviceVariable<Vector3>(length);

            CudaNormal.Run(length, width, height, depth, d_vertecies.DevicePointer, d_Voxels.DevicePointer, d_normals.DevicePointer);
            var position = new Vector3(0, 0, 0);
            ctx.Synchronize();

            Vector3[] normals = d_normals;

            d_normals.Dispose();
            d_vertecies.Dispose();

            CreateMesh(vertsArray, normals, indices.ToArray(), position);
        }
        public virtual float Sample3D(float x, float y, float z)
        {
            if (x == 0 || y == 0 || z == 0 || x == 1 || y == 1 || z == 1)
                return -1;
            else
                return 1f;
        }
        void Compare(VoxelArray other)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        this.voxels[x, y, z] = -other[x, y, z] * this.voxels[x, y, z];
                    }
                }
            }
        }

        private void CreateMesh(Vector3[] verts, Vector3[] normals, int[] indices, Vector3 position, bool ReferenceMesh=false)
        {
            Mesh mesh = new Mesh();
            mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(verts);
            mesh.SetTriangles(indices, 0);

            if (normals.Length > 0)
                mesh.SetNormals(normals);
            else
                mesh.RecalculateNormals();

            mesh.RecalculateBounds();
            GameObject go;
            if (!ReferenceMesh)
            {
                if (this.meshIndex != 0)
                {
                    Destroy(GameObject.Find("Mesh"+(meshIndex-1)));
                }

                go = new GameObject("Mesh"+this.meshIndex++);
                go.tag = "Cube";
                go.transform.localPosition = position;
                go.transform.parent = transform;

            }
            else
            {
                go = new GameObject("Reference");
                go.transform.parent = this.ReferenceObjectTransform;
                
            }
            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            go.GetComponent<Renderer>().material = material;
            go.GetComponent<MeshFilter>().mesh = mesh;
            go.AddComponent<MeshCollider>();

        }
       
        CudaKernel CudaCarve;
        CudaKernel CudaNormal;
        
        CudaContext ctx;
        CudaDeviceVariable<Vector3> d_normals;
        CudaDeviceVariable<float> d_Voxels;

        private void Start()
        {
            this.meshIndex = 0;
            this.MainCamera = GameObject.Find("Main Camera");

            this.marching = new MarchingTetrahedron(this.scaleDivision);
            //Surface is the value that represents the surface of mesh
            //For example the perlin noise has a range of -1 to 1 so the mid point is where we want the surface to cut through.
            //The target value does not have to be the mid point it can be any value with in the range.
            marching.Surface = 0.0f;

            #region cuda

            this.ctx = new CudaContext();
            this.CudaCarve = ctx.LoadKernel(Application.dataPath + "\\Features\\SimilarityDetection\\Cuda\\Marching.ptx", "carve");
            CudaCarve.BlockDimensions = new ManagedCuda.VectorTypes.dim3(3, 3, 3);
            CudaCarve.GridDimensions = 1;

            this.CudaNormal = ctx.LoadKernel(Application.dataPath + "\\Features\\SimilarityDetection\\Cuda\\Marching.ptx", "GetNormal");

            #endregion
            this.LoadReferenceFromFile(this.OutputFileName);
        }
        private void OnDestroy()
        {
            if (this.ctx != null) { this.ctx.Dispose(); }
        }
        private void Update()
        {
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
                var hitRelative =  (hit.point - this.transform.position)*scaleDivision;

                Debug.Log("Original: " + hit.point);

                #region differ
                //var a  = new VoxelArray(width, height, depth);
                //for(int x = 0; x < width; x++)
                //{
                //    for (int y = 0; y < height; y++)
                //    {
                //        for (int z = 0; z < depth; z++)
                //        {

                //            a[x, y, z] = voxels[x, y, z];
                //        }
                //    }
                //}
                //Compare(a);
                //voxels[1, 2, 2] = -1;
                //voxels[Mathf.Abs((int)(hitRelative.x/1)), Mathf.Abs((int)(hitRelative.y / 1)), Mathf.Abs((int)(hitRelative.z / 1))] = 1;
                //Debug.Log(a.Voxels);
                #endregion
                modify(hitRelative);

                UpdateValues();
            }
            else
            {
                Debug.DrawRay(this.MainCamera.transform.position, this.MainCamera.transform.TransformDirection(Vector3.forward) * 1000, Color.white);
                Debug.Log("Did not Hit");
            }
          
        }

        private void modify(Vector3 hitRelative)
        {
            int x = Mathf.RoundToInt(hitRelative.x);
            int y = Mathf.RoundToInt(hitRelative.y);
            int z = Mathf.RoundToInt(hitRelative.z);

            CudaCarve.Run(width, height, depth, x, y, z, this.d_Voxels.DevicePointer);

            ctx.Synchronize();
            // block copy doesnt work, so have to do element-wise copy :/
            float[] v = this.d_Voxels;
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    for (int k = 0; k < depth; k++)
                    {
                        this.voxels.Voxels[i, j, k] = v[i * (height * depth) + j * depth + k];
                    }
                      
                }
            }
        }

    }

}