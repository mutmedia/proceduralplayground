using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Biome", menuName = "Biome", order = 1)]
[Serializable]
public class Biome : ScriptableObject
{
    [Range(0, 1)]
    public float StartHeight;
    [Range(0, 1)]
    public float EndHeight;
    public Color Color;
    public string Name;
    public int Index;
    public List<Food> FoodOptions;
}
