using System;
using System.Linq;
using Generation;
using Map;
using Random;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class TestGenerationBehaviour : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI seedDisplay;
    [SerializeField] private Button generate;
    [SerializeField] private TextMeshProUGUI displayPrefab;
    [SerializeField] private RectTransform displayRoot;

    [Header("Base")]
    [SerializeField] private TextAsset dictionaryText;
    [SerializeField] private TextAsset mapText;

    [Header("Simple")]
    [SerializeField] private bool generateBase;
    [SerializeField] private TextAsset[] poolText;

    [Header("Advanced")]
    [SerializeField] private bool generateAdvanced;
    [SerializeField] private WeightedItem<TextAsset>[] roadPoolText;
    [SerializeField] private WeightedItem<TextAsset>[] riverPoolText;
    [SerializeField] private TextAsset bridgeEdgeText;
    [SerializeField] private TextAsset bridgeSegmentText;

    private void Generate() {
        ClearDisplays();

        // check 107807186 for strange gen near river, maybe riverCheck is fucked
        // 219842578 - yep it is
        // 516341611
        // 795334635
        // 1378295752
        // 1636234718
        // 1978910350
        var seed = Rng.RandomSeed;
        seedDisplay.text = $"Seed: {seed}";

        var tileDictionary = TileDictionary.FromJson(dictionaryText.text);
        var map = mapText.ToTileShape(tileDictionary);
        
        if (generateBase) {
            var pool = poolText
                .Select(t => t.ToConnectableShape(tileDictionary))
                .ToList();

            var mapForRoad = map.Copy();
            ConnectedGenerator.GenerateDumb(new Rng(seed), mapForRoad, pool.Select(s => (s, 1f)).ToList());
            var holder = HolderShape.FromShapes(mapForRoad.NestedShapes, out _);
            CreateDisplay().text = holder.ToCsv();
            CreateDisplay().text = mapForRoad.ToCsv();
        }

        if (generateAdvanced) {
            var mapForAdvanced = map.Copy();
            var roadPool = roadPoolText
                .Select(item => (item.item.ToConnectableShape(tileDictionary), item.weight))
                .ToList();
            var riverPool = riverPoolText
                .Select(item => (item.item.ToConnectableShape(tileDictionary), item.weight))
                .ToList();
            var bridgeEdge = bridgeEdgeText.ToConnectableShape(tileDictionary);
            var bridgeSegment = bridgeSegmentText.ToConnectableShape(tileDictionary);
            var riverSymbol = tileDictionary.FirstOrDefault(p => p.Value.metaData.tags.Contains(Tags.River)).Key;

            ConnectedGenerator.GenerateDumbRoadAndRiver(new Rng(seed), mapForAdvanced, roadPool, riverPool, bridgeEdge,
                bridgeSegment, riverSymbol);
            CreateDisplay().text = mapForAdvanced.ToCsv();
            foreach (var (shape, _) in mapForAdvanced.NestedShapes) {
                CreateDisplay().text = shape.ToCsv();
            }
        }
    }

    private void Start() {
        generate.onClick.AddListener(Generate);
        generate.onClick.Invoke();
    }

    private void ClearDisplays() {
        foreach (Transform child in displayRoot)
            Destroy(child.gameObject);
    }

    private TextMeshProUGUI CreateDisplay() {
        return Instantiate(displayPrefab, displayRoot);
    }

    [Serializable]
    public struct WeightedItem<T> {
        public T item;
        public float weight;
    }
}
