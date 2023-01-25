using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;


namespace MarchingCubesProject
{


    public class MarchingImplementation : MonoBehaviour
    {
        GameObject MainCamera;

        public Material material;


        private List<GameObject> meshes = new List<GameObject>();

        Object3D obj = new Object3D();


        //The size of voxel array.
        int width = 10;
        int height = 10;
        int depth = 10;

        VoxelArray voxels;
        List<Vector3> verts;
        List<Vector3> normals;
        List<int> indices;
        Marching marching = new MarchingTetrahedron();
        [ContextMenu("TEST")]
        void Test()
        {


            //Surface is the value that represents the surface of mesh
            //For example the perlin noise has a range of -1 to 1 so the mid point is where we want the surface to cut through.
            //The target value does not have to be the mid point it can be any value with in the range.
            marching.Surface = 0.0f;


            voxels = new VoxelArray(width, height, depth);

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

                        voxels[x, y, z] = obj.Sample3D(u, v, w);
                    }
                }
            }

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<int> indices = new List<int>();

            //The mesh produced is not optimal. There is one vert for each index.
            //Would need to weld vertices for better quality mesh.
            marching.Generate(voxels.Voxels, verts, indices);

       
            
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

            CreateMesh32(verts, normals, indices, position);

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

            CreateMesh32(verts, normals, indices, position);
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

        private void CreateMesh32(List<Vector3> verts, List<Vector3> normals, List<int> indices, Vector3 position)
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

            GameObject go = new GameObject("Mesh");
            go.transform.parent = transform;
            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            go.GetComponent<Renderer>().material = material;
            go.GetComponent<MeshFilter>().mesh = mesh;
            go.transform.localPosition = position;
            go.AddComponent<MeshCollider>();

            meshes.Add(go);
        }
        private void Start()
        {
            this.MainCamera = GameObject.Find("Main Camera");
            this.obj = new Object3D();

            this.marching = new MarchingTetrahedron();
            //Surface is the value that represents the surface of mesh
            //For example the perlin noise has a range of -1 to 1 so the mid point is where we want the surface to cut through.
            //The target value does not have to be the mid point it can be any value with in the range.
            marching.Surface = 0.0f;

            voxels = new VoxelArray(width, height, depth);
            verts = new List<Vector3>();
            normals = new List<Vector3>();
            indices = new List<int>();

        }
        private void Update()
        {
            RaycastHit hit;
            if (Input.GetMouseButtonDown(0) && Physics.Raycast(this.MainCamera.transform.position, this.MainCamera.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
            {
                MeshCollider meshCollider = hit.collider as MeshCollider;

                if (meshCollider == null || meshCollider.sharedMesh == null)
                    return;

                Debug.DrawRay(this.MainCamera.transform.position, this.MainCamera.transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
                var hitRelative = (AbsDistance(this.transform.position,  hit.point));
                Debug.Log("Original: " + hit.point);

                Debug.Log("Hit: "+ new Vector3(Mathf.Abs((int)(hitRelative.x / 1)), Mathf.Abs((int)(hitRelative.y / 1)), Mathf.Abs((int)(hitRelative.z / 1))));
                var a  = new VoxelArray(width, height, depth);
                for(int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        for (int z = 0; z < depth; z++)
                        {

                            a[x, y, z] = voxels[x, y, z];
                        }
                    }
                }
                voxels[1, 1, 1] = -1;

                //voxels[1, 2, 2] = -1;
                //voxels[Mathf.Abs((int)(hitRelative.x/1)), Mathf.Abs((int)(hitRelative.y / 1)), Mathf.Abs((int)(hitRelative.z / 1))] = 1;
                Compare(a);
                Debug.Log(a.Voxels);
                 UpdateValues();
            }
            else
            {
                Debug.DrawRay(this.MainCamera.transform.position, this.MainCamera.transform.TransformDirection(Vector3.forward) * 1000, Color.white);
                Debug.Log("Did not Hit");
            }
        }

        private Vector3 AbsDistance(Vector3 vec1, Vector3 vec2)
        {
            return new Vector3(Mathf.Abs(vec1.x) - Mathf.Abs(vec2.x), Mathf.Abs(vec1.y) - Mathf.Abs(vec2.y), Mathf.Abs(vec1.z) - Mathf.Abs(vec2.z));
        }


    }

}