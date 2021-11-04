﻿using System.Linq;
using Generation;
using Map;
using Random;
using TMPro;
using UnityEngine;

public class TestGenerationBehaviour : MonoBehaviour {
    [SerializeField] private TextAsset dictionaryText;
    [SerializeField] private TextAsset mapText;
    [SerializeField] private TextAsset[] poolText;
    [SerializeField] private TextMeshProUGUI display;
    [SerializeField] private TextMeshProUGUI seedDisplay;

    private void Start() {
        var tileDictionary = TileDictionary.FromJson(dictionaryText.text);
        var map = TileShape.FromCsv(mapText.text, tileDictionary);
        var pool = poolText
            .Select(t => ConnectableShape.FromTileShape(TileShape.FromCsv(t.text, tileDictionary)))
            .ToList();

        var seed = Rng.RandomSeed;
        seedDisplay.text = $"Seed: {seed}";

        var rng = new Rng(seed);
        ConnectedGenerator.GenerateDumb(rng, map, pool);
        display.text = map.ToCsv();
    }
}