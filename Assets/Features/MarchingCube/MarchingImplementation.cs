using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;
using ManagedCuda;
using System.Linq;

namespace MarchingCubesProject
{


    public class MarchingImplementation : MonoBehaviour
    {
        GameObject MainCamera;

        public Material material;


        private List<GameObject> meshes = new List<GameObject>();


        int meshIndex;

        //The size of voxel array.
        int width = 10;
        int height = 10;
        int depth = 10;

        public float scaleDivision = 10.0f;

        VoxelArray voxels;
        List<Vector3> verts;
        List<Vector3> normals;
        List<int> indices;
        MarchingTetrahedron marching;
        [ContextMenu("TEST")]
        void CreateNew()
        {


            //Surface is the value that represents the surface of mesh
            //For example the perlin noise has a range of -1 to 1 so the mid point is where we want the surface to cut through.
            //The target value does not have to be the mid point it can be any value with in the range.
            marching.Surface = 0.0f;

            Debug.Log("Befeore entering sample loop");
            voxels = new VoxelArray(width, height, depth);
            d_Voxels = voxels.Voxels.Cast<float>().ToArray();
            //Fill voxels with values. Im using perlin noise but any method to create voxels will work.
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
            Debug.Log("After entering sample loop");

            Debug.Log("Befeore generate");

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<int> indices = new List<int>();

            //The mesh produced is not optimal. There is one vert for each index.
            //Would need to weld vertices for better quality mesh.
            marching.Generate(voxels.Voxels, verts, indices);

            Debug.Log("After generate");

            Debug.Log("Befeore normals");

            for (int i = 0; i < verts.Count; i++)
            {
                //Presumes the vertex is in local space where
                //the min value is 0 and max is width/height/depth.
                Vector3 p = verts[i];

                float u = p.x / (width - 1.0f);
                float v = p.y / (height - 1.0f);
                float w = p.z / (depth - 1.0f);

                Vector3 n = voxels.GetNormal(u, v, w);

                normals.Add(n);
            }
            Debug.Log("after normals");

            Debug.Log("Befeore createmesh");

            var position = new Vector3(0, 0, 0);

            CreateMesh(verts, normals, indices, position);
            Debug.Log("After createmesh");


        }
        void UpdateValues()
        {
            List<Vector3> verts = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<int> indices = new List<int>();

            //The mesh produced is not optimal. There is one vert for each index.
            //Would need to weld vertices for better quality mesh.
            marching.Generate(voxels.Voxels, verts, indices);

            //marching.Generate(voxels.Voxels, verts, indices);
            for (int i = 0; i < verts.Count; i++)
            {
                //Presumes the vertex is in local space where
                //the min value is 0 and max is width/height/depth.
                Vector3 p = verts[i];

                float u = p.x / (width - 1.0f);
                float v = p.y / (height - 1.0f);
                float w = p.z / (depth - 1.0f);

                Vector3 n = voxels.GetNormal(u, v, w);

                normals.Add(n);
            }


            var position = new Vector3(0, 0, 0);

            CreateMesh(verts, normals, indices, position);
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

        private void CreateMesh(List<Vector3> verts, List<Vector3> normals, List<int> indices, Vector3 position)
        {
            Mesh mesh = new Mesh();
            mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(verts);
            mesh.SetTriangles(indices, 0);

            if (normals.Count > 0)
                mesh.SetNormals(normals);
            else
                mesh.RecalculateNormals();

            mesh.RecalculateBounds();

            if (this.meshIndex != 0)
            {
                Destroy(GameObject.Find("Mesh"+(meshIndex-1)));
            }
            GameObject go = new GameObject("Mesh"+this.meshIndex++);
            //go.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            go.tag = "Cube";
            go.transform.parent = transform;
            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            go.GetComponent<Renderer>().material = material;
            go.GetComponent<MeshFilter>().mesh = mesh;
            go.transform.localPosition = position;
            go.AddComponent<MeshCollider>();

            meshes.Add(go);
        }
       
        CudaKernel CudaCarve;
        CudaKernel CudaDiff;
        CudaKernel CudaOverlay;
        
        CudaContext ctx;
        CudaDeviceVariable<int> d_filter;
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
            //this.CudaDiff = ctx.LoadKernel(Application.dataPath + "\\Features\\SimilarityDetection\\Cuda\\convolution.ptx", "diff");
            //this.CudaOverlay = ctx.LoadKernel(Application.dataPath + "\\Features\\SimilarityDetection\\Cuda\\convolution.ptx", "overlay");

            #endregion
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
            Debug.Log("Hitrelative: " + hitRelative);

            int x = Mathf.RoundToInt(hitRelative.x);
            int y = Mathf.RoundToInt(hitRelative.y);
            int z = Mathf.RoundToInt(hitRelative.z);


            CudaCarve.BlockDimensions = new ManagedCuda.VectorTypes.dim3(3 ,3 ,3);
            CudaCarve.GridDimensions = 1;

            CudaCarve.Run(width, height, depth, x, y, z, this.d_Voxels.DevicePointer);
            Buffer.BlockCopy(d_Voxels, 0, voxels.Voxels, 0, width * height * depth);
            ctx.Synchronize();
            //for (int i = -1; i <= 1; i++)
            //{
            //    for (int j = -1; j <= 1; j++)
            //    {
            //        for (int k = -1; k <= 1; k++)
            //        {

            //        }

            //    }
            //}
            //voxels[x, y, z] = -1;
            //if(x < width - 1)
            //{
            //    voxels[x+1, y, z] = -1;
            //}
            //if (x > 0)
            //{
            //    voxels[x - 1, y, z] = -1;

            //}
            //if (y < height - 1)
            //{
            //    voxels[x, y+1, z] = -1;

            //}
            //if (y > 0)
            //{
            //    voxels[x, y-1, z] = -1;

            //}
            //if (z < depth - 1)
            //{
            //    voxels[x, y, z+1] = -1;

            //}
            //if (z > 0)
            //{
            //    voxels[x, y, z-1] = -1;
            //}

        }

        private Vector3 AbsDistance(Vector3 vec1, Vector3 vec2)
        {
            return new Vector3(Mathf.Abs(vec1.x) - Mathf.Abs(vec2.x), Mathf.Abs(vec1.y) - Mathf.Abs(vec2.y), Mathf.Abs(vec1.z) - Mathf.Abs(vec2.z));
        }


    }

}