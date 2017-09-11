using System.Collections.Generic;
using UnityEngine;

namespace KoboldsKeep
{
    namespace MagicaVoxelSplitter
    {
        public class MagicaVoxelRuntimeMeshSplitterData : MonoBehaviour
        {
            [System.Serializable]
            public struct STerrainData
            {
                public Color colorToLookFor;
                public Material renderMaterial;
                public PhysicMaterial physicsMaterial;
            };
            public STerrainData[] terrainImportData;

            // Cache created RenderTextures so we don't have to blit the texture every time we check for a color.
            private static Dictionary<Material, Texture2D> cachedRenderTextures = new Dictionary<Material, Texture2D>();

            public Color GetColorFromMaterial(Material material, Vector2 uv)
            {
                Color ret;
                // Copy the material's Texture2D to a new Texture2D because it imports into Unity as non-readalbe by default.
                Texture2D textureToSample = null;
                if (!cachedRenderTextures.TryGetValue(material, out textureToSample))
                {
                    Texture2D materialTexture = material.mainTexture as Texture2D;
                    if (materialTexture != null)
                    {
                        // Have to use a RenderTexture 
                        RenderTexture blitTexture = new RenderTexture(materialTexture.width, materialTexture.height, depth: 0, format: RenderTextureFormat.Default, readWrite: RenderTextureReadWrite.Linear);
                        Graphics.Blit(materialTexture, blitTexture);
                        textureToSample = new Texture2D(blitTexture.width, blitTexture.height, materialTexture.format, mipmap: false, linear: true);
                        textureToSample.ReadPixels(new Rect(0, 0, blitTexture.width, blitTexture.height), destX: 0, destY: 0);
                        textureToSample.Apply();
                        cachedRenderTextures.Add(material, textureToSample);
                    }
                }
                if (textureToSample != null)
                {
                    ret = textureToSample.GetPixelBilinear(uv.x, uv.y);
                }
                else
                {
                    Debug.LogError("Could not get RenderTexture for " + material.name);
                    ret = Color.black;
                }

                return ret;
            }

            public STerrainData GetTerrainDataFromVoxelColor(Color color)
            {
                STerrainData ret = terrainImportData[0];
                float bestDifference = float.PositiveInfinity;
                foreach (STerrainData data in terrainImportData)
                {
                    float newDifference = Mathf.Abs(color.r - data.colorToLookFor.r)
                        + Mathf.Abs(color.g - data.colorToLookFor.g)
                        + Mathf.Abs(color.b - data.colorToLookFor.b);
                    if (newDifference < bestDifference)
                    {
                        bestDifference = newDifference;
                        ret = data;
                    }
                }
                return ret;
            }

        }
    }
}