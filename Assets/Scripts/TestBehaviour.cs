using Map;
using UnityEngine;
using UnityEngine.Assertions;

public class TestBehaviour : MonoBehaviour {
    [SerializeField] private TextAsset testShapeText;
    [SerializeField] private TextAsset testNestedParentText;
    [SerializeField] private TextAsset testNestedChildText;
    [SerializeField] private TextAsset tileDictionaryText;
    
    private void Start() {
        var rng = new Random.Random(UnityEngine.Random.Range(0, int.MaxValue));
        Debug.Log($"Seed: {rng.Seed}, float: {rng.NextFloat(0, 100)}");
        var tempDict = new TileDictionary {{"a", new TileData {metaData = null, placeableOn = false}}};
        Debug.Log(tempDict.ToJson());

        var tileDictionary = TileDictionary.FromJson(tileDictionaryText.text);

        var testShape = TileShape.FromCsv(testShapeText.text, tileDictionary);

        Debug.Log(testShape.ToCsv());
        Debug.Log(testShape[new Vector2Int(0, 0)].ToText());
        Debug.Log(testShape[new Vector2Int(0, 2)].ToText());
        Debug.Log(testShape[new Vector2Int(2, 1)].ToText());

        var testNestedParent = TileShape.FromCsv(testNestedParentText.text, tileDictionary);
        var testNestedChild = TileShape.FromCsv(testNestedChildText.text, tileDictionary);

        var nestCornerResult = testNestedParent.TryNestShape(testNestedChild, new Vector2Int(0, 0));
        Debug.Log($"Try nest into corner: {nestCornerResult}");
        Assert.IsFalse(nestCornerResult);

        var nestProperResult = testNestedParent.TryNestShape(testNestedChild, new Vector2Int(1, 1));
        Debug.Log($"Try nest properly: {nestProperResult}");
        Assert.IsTrue(nestProperResult);

        testNestedParent.TryNestShape(testNestedChild, new Vector2Int(3, 2));

        Debug.Log(testNestedParent.ToCsv());
        Debug.Log(testNestedParent.Rotate(1).ToCsv());
        Debug.Log(testNestedParent.Rotate(2).ToCsv());
        Debug.Log(testNestedParent.Rotate(3).ToCsv());
        Debug.Log(testNestedParent.Rotate(4).ToCsv());

        // todo: proper tests maybe?
    }
}
