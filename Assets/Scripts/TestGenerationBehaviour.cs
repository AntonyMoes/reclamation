using System.Linq;
using Generation;
using Map;
using Random;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestGenerationBehaviour : MonoBehaviour {
    [SerializeField] private TextAsset dictionaryText;
    [SerializeField] private TextAsset mapText;
    [SerializeField] private TextAsset[] poolText;
    [SerializeField] private TextMeshProUGUI seedDisplay;
    [SerializeField] private Button generate;
    [SerializeField] private TextMeshProUGUI displayPrefab;
    [SerializeField] private RectTransform displayRoot;

    private void Generate() {
        ClearDisplays();
        
        var tileDictionary = TileDictionary.FromJson(dictionaryText.text);
        var map = TileShape.FromCsv(mapText.text, tileDictionary);
        var pool = poolText
            .Select(t => ConnectableShape.FromTileShape(TileShape.FromCsv(t.text, tileDictionary)))
            .ToList();

        var seed = Rng.RandomSeed;
        seedDisplay.text = $"Seed: {seed}";


        var mapCopy = map.Copy();
        var rng = new Rng(seed);
        ConnectedGenerator.GenerateDumb(rng, map, pool);
        CreateDisplay().text = map.ToCsv();
        var holder = HolderShape.FromShapes(map.NestedShapes, out var holderOffset);
        CreateDisplay().text = holder.ToCsv();
        mapCopy.TryNestShape(holder, holderOffset);
        CreateDisplay().text = mapCopy.ToCsv();
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
}
