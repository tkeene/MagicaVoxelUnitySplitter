using System.Collections.Generic;
using UnityEngine;

namespace KoboldsKeep
{
    namespace MagicaVoxelSplitter
    {
        public class MagicaVoxelRuntimeMeshSplitter : MonoBehaviour
        {
            public MagicaVoxelRuntimeMeshSplitterData meshData;
            public bool logColors = true;

            void Start()
            {
                ProcessMesh();
            }

            private struct STriangle
            {
                public int a, b, c;
                public Vector2 uv; // We can assume that all three points on a triangle from Magicavoxel point at the same texture coordinate.
            }

            private void ProcessMesh()
            {
                MeshFilter originalMeshFilter = GetComponentInChildren<MeshFilter>();
                Mesh rawMesh = originalMeshFilter.mesh;
                int[] rawTris = rawMesh.triangles; // Triplets of indices into the vertex array.
                Vector3[] rawVerts = rawMesh.vertices;
                Vector2[] rawUvs = rawMesh.uv; // Color coordinates on the material's palette.

                // Convert the mesh into a triangles list.
                List<STriangle> allTriangles = new List<STriangle>();
                for (int i = 0; i < rawTris.Length; i += 3)
                {
                    STriangle currentTriangle = new STriangle();
                    currentTriangle.a = rawTris[i];
                    currentTriangle.b = rawTris[i + 1];
                    currentTriangle.c = rawTris[i + 2];
                    currentTriangle.uv = rawUvs[currentTriangle.a];
                    allTriangles.Add(currentTriangle);
                }
                // Dictionary key is the texture UVs of the triangle on the MagicaVoxel color palette.
                Dictionary<Vector2, List<STriangle>> meshDataDictionary = new Dictionary<Vector2, List<STriangle>>();
                // Split the triangles out into meshes based on their texture UVs.
                foreach (STriangle triangle in allTriangles)
                {
                    if (!meshDataDictionary.ContainsKey(triangle.uv))
                    {
                        meshDataDictionary[triangle.uv] = new List<STriangle>();
                    }
                    meshDataDictionary[triangle.uv].Add(triangle);
                }
                foreach (Vector2 key in meshDataDictionary.Keys)
                {
                    List<STriangle> values = meshDataDictionary[key];
                    MeshFilter newMeshObject = GameObject.Instantiate(originalMeshFilter);
                    newMeshObject.mesh = new Mesh();
                    newMeshObject.mesh.vertices = rawVerts;
                    List<int> tris = new List<int>();
                    foreach (STriangle triangle in values)
                    {
                        tris.Add(triangle.a);
                        tris.Add(triangle.b);
                        tris.Add(triangle.c);
                    }
                    newMeshObject.mesh.SetTriangles(tris, submesh: 0);
                    newMeshObject.mesh.normals = rawMesh.normals;
                    newMeshObject.mesh.uv = rawMesh.uv;
                    newMeshObject.mesh.RecalculateBounds();
                    // Set up colliders
                    MeshCollider newCollider = newMeshObject.GetComponent<MeshCollider>();
                    newCollider.sharedMesh = newMeshObject.mesh;
                    // Get data based on the color of this voxel
                    MeshRenderer newMeshRenderer = newMeshObject.GetComponent<MeshRenderer>();
                    Color voxelColor = meshData.GetColorFromMaterial(newMeshRenderer.material, key);
                    if (logColors)
                    {
                        // Log the color and if you double-click on the message in the Console, you'll select it in the Hierarchy.
                        Debug.Log(voxelColor, newMeshObject.gameObject);
                    }
                    MagicaVoxelRuntimeMeshSplitterData.STerrainData colorData = meshData.GetTerrainDataFromVoxelColor(voxelColor);
                    newCollider.material = colorData.physicsMaterial;
                    newMeshRenderer.material = colorData.renderMaterial;
                }
                // Disable the original so that it doesn't overlap with the split meshes.
                originalMeshFilter.gameObject.SetActive(false);
            }
        }
    }
}
