using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralTerrainObjectPlacer : MonoBehaviour
{

    private List<Food> spawnedFood = new List<Food>();
    private ProceduralTerrain terrain;
    public float ScalingFactor = 0.04f / 0.25f;
    public float SpawnInterval;

    public void SpawnFood(float x, float y)
    {
        float height = terrain.GetHeightAtPoint(x, y);
        var options = terrain.GetBiome(height).FoodOptions;
        if (options.Count == 0) return;
        options.Sort((f1, f2) => UnityEngine.Random.value > 0.5 ? -1 : 1);
        var food = Instantiate(options[0], this.transform);
        x = (x - 0.5f) * 2.0f;
        y = (y - 0.5f) * 2.0f;
        food.Height = height;
        food.InitialZoomScale = terrain.HeightMapScale;
        food.InitialOffset = terrain.Offset;
        
        food.initialPosition = new Vector3(x, 0, y);
        FixFoodPosition(food);
        spawnedFood.Add(food);
    }

    private void FixFoodPosition(Food food)
    {
        food.transform.localScale = ScalingFactor * Vector3.one * food.CurrentSize / terrain.ZoomScale;
        var worldXZposition = food.initialPosition * food.InitialZoomScale * terrain.Size / terrain.HeightMapScale;
        var deltaOffset = (food.InitialOffset - terrain.Offset) / 2.0f;
        var terrainMiddlePoint = terrain.GetMiddlePointHeight();
        food.transform.position = new Vector3(
            worldXZposition.x + ((deltaOffset.x) * terrain.Size) / terrain.HeightMapScale,
            terrain.MapToWorldHeight(food.Height) - terrainMiddlePoint + food.transform.localScale.y / 2,
            worldXZposition.z + ((deltaOffset.y) * terrain.Size) / terrain.HeightMapScale);
    }

    public void OnValidate()
    {
        spawnedFood = new List<Food>();
        terrain = FindObjectOfType<ProceduralTerrain>();
    }

    private void Awake()
    {
        spawnedFood = new List<Food>();
        terrain = FindObjectOfType<ProceduralTerrain>();
    }

    private void Start()
    {
        StartCoroutine(SpawnFoodForever());
    }

    // Update is called once per frame
    public float k;
    void Update()
    {
        //transform.position = new Vector3(-terrain.Size * terrain.Offset.x / (2.0f * Mathf.Sqrt(terrain.ZoomScale)), 0, -terrain.Size * terrain.Offset.y / (2.0f * Mathf.Sqrt(terrain.ZoomScale)));
        //transform.localScale = (Vector3.one - Vector3.up) * (2.0f / terrain.ZoomScale) + Vector3.up * (2.0f / terrain.ZoomScale);
        var terrainMiddlePoint = terrain.GetMiddlePointHeight();
        spawnedFood.ForEach((food) =>
        {
            if (food == null || food.Eaten) return;
            //Debug.Log("Rescaling food");
            FixFoodPosition(food);
        });
        spawnedFood.RemoveAll((food) => food.Eaten);
    }

    IEnumerator SpawnFoodForever()
    {
        while (true)
        {
            var x = UnityEngine.Random.value;
            var y = UnityEngine.Random.value;
            SpawnFood(x, y);
            yield return new WaitForSeconds(SpawnInterval);
        }
    }
}
