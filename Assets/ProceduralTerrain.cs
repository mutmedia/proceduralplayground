using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ProceduralTerrain : MonoBehaviour
{

    public bool AutoUpdate;
    public int lod;
    public int TextureResolution;
    public int MeshSideVertices;
    public float Size;
    [Header("Heightmap settings")]
    public long HeightSeed;
    public long MoistSeed;
    public ComputeShader noiseShader;
    [Range(1.0f, 16.0f)]
    public int Octaves = 1;
    [Range(0.0f, 1.0f)]
    public float OctavePersistence = 0.5f;
    [Range(0.0f, 100.0f)]
    public float DeformationScale = 0.5f;
    [Range(0.0f, 100.0f)]
    public float BaseScale = 1.0f;
    public AnimationCurve HeightCurve;
    public Gradient testHeight;
    public Gradient testMoist;
    public Vector2 BaseOffset;
    public Vector2 Offset {
        get
        {
            return BaseOffset / BaseScale;
        }
        }
    public float ZoomScale;
    public float MoveSpeed = 1.0f;

    public float HeightMapScale {
        get
        {
            return ZoomScale / BaseScale;
        }
        }


    private ComputeBuffer ComputeNoiseBuffer;
    private float[] ComputeNoiseData;

    private void ComputeHeightMap(float scale, Vector2 offset)
    {
        int kernelHandle = noiseShader.FindKernel("CSMain");

        noiseShader.SetFloat("scale", scale);
        noiseShader.SetFloat("offsetx", offset.x);
        noiseShader.SetFloat("offsety", offset.y);
        noiseShader.SetBuffer(kernelHandle, "noiseArray", ComputeNoiseBuffer);
        noiseShader.Dispatch(kernelHandle, TextureResolution / 8, TextureResolution / 8, 1);
    }

    public float GetMiddlePointHeight()
    {
        return MapToWorldHeight(GetHeightAtPoint(0.5f, 0.5f));
    }

    private float HeightScale
    {
        get
        {
            return DeformationScale / ZoomScale;
        }
    }

    public float MinHeight
    {
        get
        {
            return HeightScale * HeightCurve.Evaluate(0);
        }
    }

    public float MaxHeight
    {
        get
        {
            return HeightScale * HeightCurve.Evaluate(1);
        }
    }

    public float[] baseStartHeights
    {
        get
        {
            return Biomes.ConvertAll((biome) => biome.StartHeight).ToArray();
        }
    }

    public Color[] baseColors
    {
        get
        {
            return Biomes.ConvertAll((biome) => biome.Color).ToArray();
        }
    }





    private SimplexNoiseGenerator heightNoiseGen;
    private SimplexNoiseGenerator moistureNoiseGen;
    public void OnValidate()
    {
        HeightSeed = Math.Max(0, HeightSeed);
        MoistSeed = Math.Max(0, MoistSeed);
        Octaves = Mathf.Clamp(Octaves, 1, 16);
        OctavePersistence = Mathf.Clamp(OctavePersistence, 0.0f, 1.0f);
        MeshSideVertices = Mathf.Max(2, MeshSideVertices);
        TextureResolution = MeshSideVertices * lod;
        ZoomScale = Mathf.Max(0.01f, ZoomScale);
        Size = Mathf.Max(0.01f, Size);
        //transform.localScale = Vector3.one * Scale * 100;
        Biomes.Sort((b1, b2) => b1.StartHeight < b2.StartHeight ? -1 : 1);

        heightNoiseGen = new SimplexNoiseGenerator(HeightSeed);
        moistureNoiseGen = new SimplexNoiseGenerator(MoistSeed);
    }

    private float[,] CreateSimplexNoiseMap(int resolution, int octaves, float persistence, float scale, SimplexNoiseGenerator noiseGen)
    {
        return CreateSimplexNoiseMap(resolution, octaves, persistence, scale, noiseGen, (x) => x);
    }
    private float[,] CreateSimplexNoiseMap(int resolution, int octaves, float persistence, float scale, SimplexNoiseGenerator noiseGen, Func<float, float> tween)
    {
        var values = new float[resolution, resolution];
        var minValue = 1.0f;
        var maxValue = -1.0f;
        for (int iv = 0; iv < resolution; iv++)
            for (int iu = 0; iu < resolution; iu++)
            {
                double u = (((double)iu / resolution) * 2.0f - 1.0f) * scale + 0.5f * Offset.x;
                double v = (((double)iv / resolution) * 2.0f - 1.0f) * scale + 0.5f * Offset.y;
                float value = (float)noiseGen.EvaluateFractal(u, v, octaves, persistence);
                //Hack, the thing returns mostly between -0.7 and -0.7 for 8 octaves
                value /= 0.87f;
                value /= (1.0f - 0.4f * persistence);
                value = (value + 1.0f) / 2.0f;
                values[iu, iv] = tween(value);
                minValue = value < minValue ? value : minValue;
                maxValue = value > maxValue ? value : maxValue;
            }
        /*
        Debug.Log(minValue);
        Debug.Log(maxValue);
        */
        return values;
    }

    /*
    public float GetHeight(float u, float v)
    {
        double du = (((double)u / Resolution) * 2.0f - 1.0f) * ZoomScale + 0.5f * Offset.x;
        double dv = (((double)v / Resolution) * 2.0f - 1.0f) * ZoomScale + 0.5f * Offset.y;
        float value = (float)heightNoiseGen.EvaluateFractal(du, dv, Octaves, OctavePersistence);
        //Hack, the thing returns mostly between -0.7 and -0.7 for 8 octaves
        value /= 0.87f;
        value /= (1.0f - 0.4f * OctavePersistence);
        value = (value + 1.0f) / 2.0f;
        return HeightCurve.Evaluate(value);
    }

    public Biome GetBiome(float height)
    {
        foreach (var b in Biomes)
        {
            if (height > b.StartHeight)
            {
                return b;
            }
        }
        return Biomes[Biomes.Count - 1];
    }
    */

    private Mesh CreateMesh(float size, int lod, int resolution)
    {
        var mesh = new Mesh();
        {
            int len = resolution;
            var verts = new Vector3[len * len];
            var uvs = new Vector2[len * len];
            int i = 0;
            for (int iv = 0; iv < resolution; iv++)
                for (int iu = 0; iu < resolution; iu++)
                {
                    float u = (float)iu / (resolution - 1);
                    float v = (float)iv / (resolution - 1);
                    float x = ((u * 2.0f) - 1) * size;
                    float y = ((v * 2.0f) - 1) * size;
                    uvs[i] = new Vector2(u, v);
                    verts[i++] = new Vector3(x, 0.0f, y);
                }

            mesh.vertices = verts;
            mesh.uv = uvs;
        }

        {
            var tris = new int[((resolution - 1) * (resolution - 1)) * 6];
            Func<int, int[], int> setQuad =
                                                   (it, qv) =>
                                                   {
                                                       tris[it] = qv[0];
                                                       tris[it + 1] = tris[it + 4] = qv[2];
                                                       tris[it + 2] = tris[it + 3] = qv[1];
                                                       tris[it + 5] = qv[3];
                                                       return it + 6;
                                                   };

            int i = 0;
            int step = resolution;
            for (int iv = 0; iv < resolution - 1; iv++)
                for (int iu = 0; iu < resolution - 1; iu++)
                {
                    var q00 = iu + iv * step;
                    i = setQuad(i, new int[] { q00, q00 + 1, q00 + step, q00 + step + 1 });
                }
            mesh.triangles = tris;
        }

        return mesh;
    }

    /*
    public void DeformMesh(int resolution, Mesh mesh, float[,] heightmap, float maxDeformation)
    {
        List<Vector3> verts = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        mesh.GetVertices(verts);
        mesh.GetUVs(0, uvs);
        for (int v = 0; v < resolution; v++)
            for (int u = 0; u < resolution; u++)
            {
                var vert = verts[u + v * resolution];
                vert = Vector3.Scale(vert, Vector3.one - Vector3.up);
                vert += Vector3.up * heightmap[u, v] * maxDeformation;
                verts[u + v * resolution] = vert;
            }
        mesh.SetVertices(verts);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
    */

    public void DeformMesh(int meshResolution, int lod, Mesh mesh, float[] heightmap, float maxDeformation)
    {
        List<Vector3> verts = new List<Vector3>(meshResolution*meshResolution);
        List<Vector2> uvs = new List<Vector2>(meshResolution*meshResolution);
        mesh.GetVertices(verts);
        mesh.GetUVs(0, uvs);
        for (int v = 0; v < meshResolution; v++)
            for (int u = 0; u < meshResolution; u++)
            {
                var vert = verts[u + v * meshResolution];
                vert = new Vector3(vert.x, HeightCurve.Evaluate(heightmap[u * lod + v * lod * meshResolution * lod]) * maxDeformation, vert.z);
                verts[u + v * meshResolution] = vert;
            }
        mesh.SetVertices(verts);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    public List<Biome> Biomes;
    public Texture2D GenerateTexture(float[,] heightMap, List<Biome> biomes)
    {
        Texture2D texture = new Texture2D(heightMap.GetLength(0), heightMap.GetLength(1));
        var colors = new Color[texture.width * texture.height];
        for (int v = 0; v < texture.height; v++)
            for (int u = 0; u < texture.width; u++)
            {
                var h = heightMap[u, v];
                var biome = biomes.Find((b) => b.StartHeight < h && h < b.EndHeight);
                if (biome != null)
                    colors[u + v * texture.width] = biome.Color;

            }
        texture.SetPixels(colors);
        texture.Apply();
        return texture;
    }

    public float GetHeightAtPoint(float x, float y)
    {
        var heightmap = ComputeNoiseData;
        if (heightmap == null) return 0;
        var heightmapResolution = (int)Mathf.Sqrt(heightmap.Length);
        if (heightmapResolution == 0) return 0;
        x *= (heightmapResolution - 1);
        y *= (heightmapResolution - 1);
        var x1 = (int)(x);
        var x2 = x1 == heightmapResolution - 1 ? x1 : x1 + 1;
        var y1 = (int)(y);
        var y2 = y1 == heightmapResolution - 1 ? y1 : y1 + 1;
        x = x - x1;
        y = y - y1;

        var h = heightmap[x1 + heightmapResolution * y1] * (1 - x) * (1 - y) + heightmap[x2 + heightmapResolution * y1] * x * (1 - y) +
            heightmap[x1 + heightmapResolution * y2] * (1 - x) * y + heightmap[x2 + heightmapResolution * y2] * x * y;

        return HeightCurve.Evaluate(h);
    }

    public Biome GetBiome(float height)
    {
        return Biomes.FindLast((b) => height > b.StartHeight);
        //return Biomes[Biomes.Count - 1];
    }

    public float MapToWorldHeight(float height)
    {
        return height * HeightScale * transform.localScale.y;
    }

    public void CreateTerrain()
    {
        Biomes.Sort((b1, b2) => b1.StartHeight < b2.StartHeight ? -1 : 1);
        ComputeNoiseData = new float[TextureResolution * TextureResolution];
        noiseShader.SetInt("resolution", TextureResolution);
        var mesh = CreateMesh(Size, lod, MeshSideVertices);
        UpdateTerrain(mesh);
        GetComponent<MeshFilter>().sharedMesh = mesh;
        var material = GetComponent<MeshRenderer>().sharedMaterial;
        material.SetFloat("minHeight", MinHeight);
        material.SetFloat("maxHeight", MaxHeight);
        material.SetInt("baseColorCount", baseColors.Length);
        material.SetColorArray("baseColors", baseColors);
        material.SetFloatArray("baseStartHeights", baseStartHeights);
        /*
        Debug.Log("Heights");
        for(int i = 0; i < baseColors.Length; i++)
        {
            Debug.Log("height: " + baseStartHeights[i] + " color: " + baseColors[i]);
        }
        */

    }

    //private float[,] heightmap;
    public void UpdateTerrain(Mesh mesh)
    {
        //heightmap = CreateSimplexNoiseMap(Resolution, Octaves, OctavePersistence, ZoomScale, heightNoiseGen, (x) => HeightCurve.Evaluate(x));
        ComputeNoiseBuffer = new ComputeBuffer(ComputeNoiseData.Length, sizeof(float));
        ComputeHeightMap(HeightMapScale, Offset);
        ComputeNoiseBuffer.GetData(ComputeNoiseData);
        ComputeNoiseBuffer.Dispose();
        DeformMesh(MeshSideVertices, lod, mesh, ComputeNoiseData, HeightScale);
        //var moistMap = CreateSimplexNoiseMap(Resolution, Octaves, OctavePersistence, ZoomScale, moistureNoiseGen);
        //var texture = GenerateTexture(heightmap, Biomes);
        var material = GetComponent<MeshRenderer>().sharedMaterial;
        //material.mainTexture = texture;
        material.SetFloat("minHeight", MinHeight);
        material.SetFloat("maxHeight", MaxHeight);
    }

    private void Awake()
    {
        ZoomScale = 0.01f;

        heightNoiseGen = new SimplexNoiseGenerator(HeightSeed);
        moistureNoiseGen = new SimplexNoiseGenerator(MoistSeed);
        CreateTerrain();
    }

    // Update is called once per frame
    void Update()
    {
        var mesh = GetComponent<MeshFilter>().sharedMesh;
        UpdateTerrain(mesh);
        transform.position = new Vector3(0, - GetMiddlePointHeight(), 0);
    }
}
