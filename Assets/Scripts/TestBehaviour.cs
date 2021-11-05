using Generation;
using Map;
using Random;
using UnityEngine;
using UnityEngine.Assertions;

public class TestBehaviour : MonoBehaviour {
    [SerializeField] private TextAsset testShapeText;
    [SerializeField] private TextAsset testNestedParentText;
    [SerializeField] private TextAsset testNestedChildText;
    [SerializeField] private TextAsset tileDictionaryText;
    
    [SerializeField] private TextAsset endText;
    [SerializeField] private TextAsset turnText;

    private void Start() {
        var rng = new Rng(Rng.RandomSeed);
        Debug.Log($"Seed: {rng.Seed}, float: {rng.NextFloat(0, 100)}");
        var tempDict = new TileDictionary {{"a", new TileData {metaData = null, placeableOn = false}}};
        Debug.Log(tempDict.ToJson());

        var tileDictionary = TileDictionary.FromJson(tileDictionaryText.text);
        Debug.Log(tileDictionary.ToJson());
        Debug.Log(string.Join(", ", tileDictionary["-"].metaData.tags));

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

        var turn = ConnectableShape.FromTileShape(TileShape.FromCsv(turnText.text, tileDictionary));
        var end = ConnectableShape.FromTileShape(TileShape.FromCsv(endText.text, tileDictionary));
        
        Assert.IsFalse(end.TryConnect(end, Direction.West, out _));
        Assert.IsTrue(end.TryConnect(end.Rotate(2) as ConnectableShape, Direction.West, out var offset));
        Debug.Log($"Offset: {offset}");
        Assert.IsTrue((end.Rotate(1) as ConnectableShape).TryConnect(end.Rotate(3) as ConnectableShape, Direction.North, out offset));
        Debug.Log($"Offset: {offset}");
        Assert.IsTrue((end.Rotate(2) as ConnectableShape).TryConnect(end.Rotate(4) as ConnectableShape, Direction.East, out offset));
        Debug.Log($"Offset: {offset}");
        Assert.IsTrue((end.Rotate(3) as ConnectableShape).TryConnect(end.Rotate(5) as ConnectableShape, Direction.South, out offset));
        Debug.Log($"Offset: {offset}");

        var parent = TileShape.FromCsv(".,.", tileDictionary);
        Assert.IsTrue(parent.TryNestShape(end, new Vector2Int(0, -1)));
        Debug.Log(parent.ToCsv());
    }
}
