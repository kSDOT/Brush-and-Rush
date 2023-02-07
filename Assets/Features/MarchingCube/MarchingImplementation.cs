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

    [RequireComponent(typeof(LineRenderer))]
    public class MarchingImplementation : MonoBehaviour
    {
        LineRenderer laserLine;        

        /// <summary>
        /// The transform parent of sculpture we will copy
        /// </summary>
        public Transform ReferenceObjectTransform;
        /// <summary>
        /// The transform parent of our duplicate
        /// </summary>
        public Transform DuplicateObjectTransform;
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
        /// Material used for sculptures when overlaying
        /// </summary>
        public Material SculptureOverlayMaterial;
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
        private Mesh[] meshes;
        private Mesh meshReference;
        private GameObject[] childObjects;
        float inputTimer;
        private int chunkNumber;
        LayerMask mask;
        public bool createNew = false;
        /// <summary>
        /// Constructs a cube
        /// </summary>
        MarchingCubes Marching;
        enum JobStatus { Free, CreateNew, UpdateValues, Reference };
        JobStatus Status;
        [ContextMenu("TEST")]
        IEnumerator CreateNew()
        {

            this.Marching.verts.Clear();
            this.Marching.indices.Clear();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        if (x == 0 || y == 0 || z == 0 || x == width - 1 || y == height - 1 || z == depth - 1)
                            this.Marching.voxels[x * height * depth + y * depth + z] = -1f;
                        else
                            this.Marching.voxels[x * height * depth + y * depth + z] = 1f;
                    }
                }
            }
            var job = this.Marching.Schedule();
            while (!job.IsCompleted) yield return null;
            job.Complete();
            StartCoroutine(CreateMesh(this.Marching.verts, this.Marching.indices));
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


            String input = File.ReadAllText(Application.streamingAssetsPath + "\\" + FileName);
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
            while (!job.IsCompleted)
            {
                yield return null;
            }
            job.Complete();
            this.CreateMeshReference(this.Marching.verts, this.Marching.indices);
            NativeArray<float>.Copy(this.Marching.voxels, this.referenceVoxels);

            this.Status = JobStatus.CreateNew;

            StartCoroutine(CreateNew());
            yield return null;
        }
        /// <summary>
        /// Updates the sculpture with the modified isovalues
        /// </summary>
        IEnumerator UpdateValues()
        {

            this.Marching.verts.Clear();
            this.Marching.indices.Clear();
            var job = this.Marching.Schedule();
            while (!job.IsCompleted)
            {
                yield return null;
            }
            job.Complete();

            StartCoroutine(this.CreateMesh(this.Marching.verts, this.Marching.indices));
            this.Status = JobStatus.Free;
        }
        [ContextMenu("Compare")]
         void Compare()
        {
            GameObject parent = new GameObject("Compare");
            StartCoroutine(this.Compare(parent, returnValue=>Debug.Log(returnValue)));
        }
        public IEnumerator Compare(GameObject parent, System.Action<float> callback)
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
            foreach(var child in this.childObjects)
            {
                child.transform.SetParent(parent.transform);
                child.transform.localPosition = new Vector3(0, 0, 0);
                child.GetComponent<MeshRenderer>().material = this.SculptureOverlayMaterial;
            }

            Marching.Schedule().Complete();

            CreateMeshResult(Marching.verts, Marching.indices, parent.transform);

            // make sure its in [0, 100] range, using 1 decimal digits
            callback.Invoke(Mathf.Clamp(Mathf.Round(errorAccumulator/(width * height * depth)* 10.0f) * 0.1f, 0, 100));
        }
        /// <summary>
        /// Creates the mesh given vertices
        /// </summary>
        /// <param name="verts"></param>
        /// <param name="indices"></param>
        IEnumerator CreateMesh(NativeList<Vector3> verts, NativeList<int> indices)
        {
            int timePerFrame = 0;
            DateTimeOffset t1 = DateTime.Now;
            var vertsArray = verts.ToArray();
            var indicesArray = indices.ToArray();
            int chunkSize = vertsArray.Length / this.chunkNumber;
            while(chunkSize%3!= 0)
            {
                chunkSize++;
            }
            int i = 0;
            for (; i < this.meshes.Length - 1; i++) {
                this.meshes[i].SetVertices(vertsArray);
                this.meshes[i].SetTriangles(indicesArray[(i * chunkSize)..((i + 1) * chunkSize)], 0);
                this.meshes[i].RecalculateNormals();

                Destroy(this.childObjects[i]);
                this.childObjects[i] = new GameObject("");
                this.childObjects[i].layer = 13;
                this.childObjects[i].transform.parent = this.DuplicateObjectTransform;
                this.childObjects[i].transform.localPosition = new Vector3(0, 0, 0);

                this.childObjects[i].AddComponent<MeshFilter>().mesh = this.meshes[i];
                this.childObjects[i].AddComponent<MeshRenderer>();
                this.childObjects[i].GetComponent<Renderer>().material = SculptureMaterial;
                this.childObjects[i].AddComponent<MeshCollider>();
                timePerFrame += (DateTime.Now - t1).Milliseconds;
                if (timePerFrame >= 20)
                {
                    yield return 0;
                }
            }
            // Remainder
            this.meshes[i].SetVertices(vertsArray);
            this.meshes[i].SetTriangles(indicesArray[(i * chunkSize)..], 0);
            this.meshes[i].RecalculateNormals();

            Destroy(this.childObjects[i]);
            this.childObjects[i] = new GameObject("");
            this.childObjects[i].layer = 13;
            this.childObjects[i].transform.parent = this.DuplicateObjectTransform;
            this.childObjects[i].transform.localPosition = new Vector3(0, 0, 0);

            this.childObjects[i].AddComponent<MeshFilter>().mesh = this.meshes[i];
            this.childObjects[i].AddComponent<MeshRenderer>();
            this.childObjects[i].GetComponent<Renderer>().material = SculptureMaterial;
            this.childObjects[i].AddComponent<MeshCollider>();
            yield return null;
        }
        /// <summary>
        /// Creates the reference mesh after loading file (almost same as createmesh)
        /// </summary>
        /// <param name="verts"></param>
        /// <param name="indices"></param>
        private void CreateMeshReference(NativeList<Vector3> verts, NativeList<int> indices)
        {
            this.meshReference.SetVertices(verts.ToArray());
            this.meshReference.SetTriangles(indices.ToArray(), 0);
            this.meshReference.RecalculateNormals();

            GameObject go;
            go = new GameObject("Reference");
            go.transform.parent = this.ReferenceObjectTransform;
            go.transform.localPosition = new Vector3(0, 0, 0);

            go.AddComponent<MeshFilter>().mesh = meshReference;
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
            Mesh meshResult = new Mesh(); 
            meshResult.SetVertices(verts.ToArray());
            meshResult.SetTriangles(indices.ToArray(), 0);
            meshResult.RecalculateNormals();

            GameObject go;
            go = new GameObject("Difference");
            go.transform.parent = parentTransform;
            go.transform.localPosition = new Vector3(0, 0, 0);

            go.AddComponent<MeshFilter>().mesh = meshResult;
            go.AddComponent<MeshRenderer>();
            go.GetComponent<Renderer>().material = ErrorMaterial;
            go.AddComponent<MeshCollider>();

        }

        private void Start()
        {
            this.laserLine = this.GetComponent<LineRenderer>();
            this.inputTimer = 0;
            this.chunkNumber = 10;
            this.mask = LayerMask.GetMask("Sculpture");

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
                verts = new NativeList<Vector3>(1000, Allocator.Persistent),
                indices = new NativeList<int>(1000, Allocator.Persistent),
                voxels = new(width * height * depth, Allocator.Persistent),
            };
            this.meshes = new Mesh[chunkNumber];
            for (int i = 0; i < this.meshes.Length; ++i)
            {
                this.meshes[i] = new Mesh();
                this.meshes[i].MarkDynamic();
                this.meshes[i].indexFormat = IndexFormat.UInt32;
            }
            this.meshReference = new Mesh();
            this.meshReference.MarkDynamic();
            this.meshReference.indexFormat = IndexFormat.UInt32;

            this.childObjects = new GameObject[chunkNumber];
            for (int i = 0; i < this.childObjects.Length; ++i)
            {
                this.childObjects[i] = new GameObject("");
                this.childObjects[i].transform.parent = this.DuplicateObjectTransform;
                this.childObjects[i].transform.position = Vector3.zero;
                this.childObjects[i].transform.localRotation = Quaternion.identity;
            }
            if (!this.createNew)
            {
                this.Status = JobStatus.Reference;
                this.StartCoroutine(this.LoadReferenceFromFile(this.InputFileName));
            }
            else
            {
                this.Status = JobStatus.CreateNew;

                StartCoroutine(CreateNew());
            }

        }
        bool isDown = false;
        public void TriggerDown()
        {
            isDown = true;
        }  
        
        public void TriggerUp()
        {

            isDown = false;
        }
        private void Update()
        {
            this.laserLine.SetPosition(0, this.transform.position + new Vector3(0.0f, 0.0f, 0.05f));
            inputTimer += Time.deltaTime;
            RaycastHit hit;
            if (this.isDown
                && Physics.Raycast(this.transform.position, this.transform.forward, out hit, Mathf.Infinity, mask)
                //&& hit.collider.gameObject.CompareTag("Cube")
                )
            {
                this.laserLine.SetPosition(1, hit.point);
                MeshCollider meshCollider = hit.collider as MeshCollider;

                if (meshCollider == null || meshCollider.sharedMesh == null)
                    return;

                var hitRelative = AbsDistance(hit.point, this.DuplicateObjectTransform.position) * scaleDivision;

                
                if(this.Status == JobStatus.Free && this.inputTimer > 0.1f)
                {

                    this.inputTimer = 0.0f;

                    this.Status = JobStatus.UpdateValues;
                    modify(hitRelative);
                } 


            }
            else
            {
                this.laserLine.SetPosition(1, this.transform.position + this.transform.forward * 20);
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