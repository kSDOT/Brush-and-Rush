using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using System.Collections;

namespace MarchingCubesProject
{


    public class MarchingImplementation : MonoBehaviour
    {
        GameObject MainCamera;
        /// <summary>
        /// The transform parent of sculpture we will copy
        /// </summary>
        public Transform ReferenceObjectTransform;
        /// <summary>
        /// Our copied object
        /// </summary>
        private GameObject CopiedObject;
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
        NativeArray<float> referenceVoxels;
        /// <summary>
        /// Constructs a cube
        /// </summary>
        MarchingCubes Marching;
        enum JobStatus { Free, CreateNew, UpdateValues, Reference };
        JobStatus Status;
        [ContextMenu("TEST")]
        IEnumerator CreateNew()
        {
            Debug.Log("enter: " + DateTimeOffset.UnixEpoch.Millisecond);

            this.Marching.verts.Clear();
            this.Marching.indices.Clear();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        if (x == 0 || y == 0 || z == 0 || x == width-1 || y == height-1|| z == depth-1)
                            this.Marching.voxels[x * height * depth + y * depth + z] = -1f;
                        else
                            this.Marching.voxels[x * height * depth + y * depth + z] = 1f;
                    }
                }
            }

            var job = this.Marching.Schedule();
            while (!job.IsCompleted) yield return null;
            job.Complete();
            CreateMesh(this.Marching.verts, this.Marching.indices);
            Debug.Log("finish: "+ DateTimeOffset.UnixEpoch.Millisecond);
            this.Status = JobStatus.Free;

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

                        if (dep != this.depth - 1 || col != width - 1) { Console.Write(String.Format("{0} ", this.Marching.voxels[row * height*depth + col * depth + dep])); }
                        else { 
                            Console.Write(String.Format("{0}", this.Marching.voxels[row * height * depth + col * depth + dep])); 
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
        IEnumerator LoadReferenceFromFile(string FileName = "")
        {
            this.referenceVoxels = new(width * height * depth, Allocator.Persistent);


            String input = File.ReadAllText(Application.dataPath + "\\" + FileName);
            int i = 0, j = 0, k = 0;
            this.Marching.verts.Clear();
            this.Marching.indices.Clear();
            // Load values from file into voxelarray
            foreach (var row in input.Split('\n'))
            {
                j = 0;
                foreach (var col in row.Trim().Split(' '))
                {
                    this.Marching.voxels[i * depth * height + j * depth + k] = int.Parse(col.Trim());
                    k++;
                    if (k == depth)
                    {
                        k = 0;
                        j++;
                    }
                }
                i++;
            }

            var job = this.Marching.Schedule();
            while (!job.IsCompleted) { 
                yield return null; 
            }
            job.Complete();
            this.CreateMeshReference(this.Marching.verts, this.Marching.indices);
            NativeArray<float>.Copy(this.Marching.voxels, this.referenceVoxels);

            this.Status = JobStatus.CreateNew;

            StartCoroutine(CreateNew());
        }

        /// <summary>
        /// Updates the sculpture with the modified isovalues
        /// </summary>
        IEnumerator UpdateValues()
        {
            Debug.Log("enter update: " + DateTimeOffset.Now.ToUnixTimeMilliseconds());

            this.Marching.verts.Clear();
            this.Marching.indices.Clear();
            var job = this.Marching.Schedule();
            while (!job.IsCompleted)
            {
                yield return null;
            }
            job.Complete();
            Debug.Log("exit generate: " + DateTimeOffset.Now.ToUnixTimeMilliseconds());

            this.CreateMesh(this.Marching.verts, this.Marching.indices);
            this.Status = JobStatus.Free;
            Debug.Log("exit update: " + DateTimeOffset.Now.ToUnixTimeMilliseconds());
        }
        [ContextMenu("Compare")]
        void Compare()
        {
            GameObject parent = new GameObject("Compare");
            StartCoroutine(this.Compare(parent));
        }
        IEnumerator Compare(GameObject parent )
        {
            while(this.Status != JobStatus.Free) { yield return null; }

            this.Marching.verts.Clear();
            this.Marching.indices.Clear();
            float errorAccumulator = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        this.Marching.voxels[x * height * depth + y * depth + z] = -this.referenceVoxels[x * height * depth + y * depth + z] 
                                                                          * this.Marching.voxels[x * height * depth + y * depth + z];
                        if (this.Marching.voxels[x * height * depth + y * depth + z] == 1) { 
                            errorAccumulator++;
                        }
                    }
                }
            }
            var opaque_material = this.CopiedObject.GetComponent<MeshRenderer>().material.color;
            opaque_material= new Color(opaque_material.r, opaque_material.g, opaque_material.b, 0.3f);
            this.CopiedObject.GetComponent<MeshRenderer>().material.color = opaque_material;
            this.CopiedObject.transform.SetParent(parent.transform);
            this.CopiedObject.transform.localPosition = new Vector3(0, 0, 0);

            Marching.Schedule().Complete();

            CreateMeshResult(Marching.verts, Marching.indices, parent.transform);

            // make sure its in [0, 100] range, using 1 decimal digits
            yield return Mathf.Clamp(Mathf.Round(errorAccumulator/(width * height * depth)* 10.0f) * 0.1f, 0, 100);
        }
        /// <summary>
        /// Creates the mesh given vertices
        /// </summary>
        /// <param name="verts"></param>
        /// <param name="indices"></param>
        private void CreateMesh(NativeList<Vector3> verts, NativeList<int> indices)
        {
            Debug.Log("enter create mesh: " + DateTimeOffset.Now.ToUnixTimeMilliseconds());
            Mesh mesh = new Mesh();
            mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(verts.ToArray());
            mesh.SetTriangles(indices.ToArray(), 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            //mesh.RecalculateTangents();

          
            Destroy(this.CopiedObject);
            this.CopiedObject = new GameObject("Mesh");
            this.CopiedObject.tag = "Cube";
            this.CopiedObject.transform.parent = transform;
            this.CopiedObject.transform.localPosition = new Vector3(0, 0, 0);

            this.CopiedObject.AddComponent<MeshFilter>().mesh = mesh;
            this.CopiedObject.AddComponent<MeshRenderer>();
            this.CopiedObject.GetComponent<Renderer>().material = SculptureMaterial;
            this.CopiedObject.AddComponent<MeshCollider>();
            Debug.Log("exit create mesh: " + DateTimeOffset.Now.ToUnixTimeMilliseconds());

        }
        /// <summary>
        /// Creates the reference mesh after loading file (almost same as createmesh)
        /// </summary>
        /// <param name="verts"></param>
        /// <param name="indices"></param>
        private void CreateMeshReference(NativeList<Vector3> verts, NativeList<int> indices)
        {
            Mesh mesh = new Mesh();
            mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(verts.ToArray());
            mesh.SetTriangles(indices.ToArray(), 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            GameObject go;
            go = new GameObject("Reference");
            go.transform.parent = this.ReferenceObjectTransform;
            go.transform.localPosition = new Vector3(0, 0, 0);

            go.AddComponent<MeshFilter>().mesh = mesh;
            go.AddComponent<MeshRenderer>();
            go.GetComponent<Renderer>().material = SculptureMaterial;
            go.AddComponent<MeshCollider>();

        }
        /// <summary>
        /// Creates the difference mesh, overlayed on top of duplicate mesh (almost same as createmesh)
        /// </summary>
        /// <param name="verts"></param>
        /// <param name="indices"></param>
        private void CreateMeshResult(NativeList<Vector3> verts, NativeList<int> indices, Transform parentTransform)
        {
            Mesh mesh = new Mesh();
            mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(verts.ToArray());
            mesh.SetTriangles(indices.ToArray(), 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            GameObject go;
            go = new GameObject("Difference");
            go.transform.parent = this.CopiedObject.transform;
            go.transform.localPosition = new Vector3(0, 0, 0);

            go.AddComponent<MeshFilter>().mesh = mesh;
            go.AddComponent<MeshRenderer>();
            go.GetComponent<Renderer>().material = ErrorMaterial;
            go.AddComponent<MeshCollider>();

        }

        private void Start()
        {
            this.MainCamera = GameObject.Find("Main Camera");

            this.Marching = new MarchingCubes
            {

                width = this.width,
                height = this.height,
                depth = this.depth,
                Scale = this.scaleDivision,
                EdgeVertex = new(12, Allocator.Persistent),

                Cube = new(8, Allocator.Persistent),
                WindingOrder = new(new[] { 0, 1, 2 }, Allocator.Persistent),
                Surface = 0.0f,
                verts = new NativeList<Vector3>(100000, Allocator.Persistent),
                indices = new NativeList<int>(100000, Allocator.Persistent),
                voxels = new(width * height * depth, Allocator.Persistent),
            };
            this.Status = JobStatus.Reference;
            this.StartCoroutine(this.LoadReferenceFromFile(this.InputFileName));
            this.CopiedObject = new GameObject();

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
            #region UpdateJob
            //switch (this.Status)
            //{
            //    case JobStatus.Free:         break;
            //    case JobStatus.CreateNew:    this.StartCoroutine(this.CreateNew()); break;
            //    case JobStatus.UpdateValues: this.StartCoroutine(this.UpdateValues()); break;
            //    case JobStatus.Reference:    this.StartCoroutine(this.LoadReferenceFromFile()); break;
            //}
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
                
                if(this.Status == JobStatus.Free)
                {
                    this.Status = JobStatus.UpdateValues;
                    modify(hitRelative);
                } 

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
                            this.Marching.voxels[x_2* height * depth + y_2 * depth + z_2] = -1;
                        }    
                    }

                }
            }

            StartCoroutine(UpdateValues());
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
        private void OnDestroy()
        {
            this.Marching.WindingOrder.Dispose();
            this.Marching.verts.Dispose();
            this.Marching.Cube.Dispose();
            this.Marching.EdgeVertex.Dispose();
            this.Marching.indices.Dispose();
            this.Marching.voxels.Dispose();
            this.referenceVoxels.Dispose();
        }

    }

}