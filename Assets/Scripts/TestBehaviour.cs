using Map;
using UnityEngine;

public class TestBehaviour : MonoBehaviour {
    [SerializeField] private TextAsset testShapeText;
    [SerializeField] private TextAsset testNestedParentText;
    [SerializeField] private TextAsset testNestedChildText;
    
    private void Start() {
        var rng = new Random.Random(UnityEngine.Random.Range(0, int.MaxValue));
        Debug.Log($"Seed: {rng.Seed}, float: {rng.NextFloat(0, 100)}");

        var testShape = TileShape.FromCsv(testShapeText.text);
        Debug.Log(testShape.Size);

        Debug.Log(testShape[new Vector2Int(0, 0)]);
        Debug.Log(testShape[new Vector2Int(0, 2)]);
        Debug.Log(testShape[new Vector2Int(2, 1)]);

        var testNestedParent = TileShape.FromCsv(testNestedParentText.text);
        var testNestedChild = TileShape.FromCsv(testNestedChildText.text);

        var nestCornerResult = testNestedParent.TryNestShape(testNestedChild, new Vector2Int(0, 0));
        Debug.Log($"Try nest into corner: {nestCornerResult}");
        
        var nestProperResult = testNestedParent.TryNestShape(testNestedChild, new Vector2Int(1, 1));
        Debug.Log($"Try nest properly: {nestProperResult}");

        testNestedParent.TryNestShape(testNestedChild, new Vector2Int(3, 2));

        Debug.Log(testNestedParent.ToCsv());
        Debug.Log(testNestedParent.Rotate(1).ToCsv());
        Debug.Log(testNestedParent.Rotate(2).ToCsv());
        Debug.Log(testNestedParent.Rotate(3).ToCsv());
        Debug.Log(testNestedParent.Rotate(4).ToCsv());
    }
}
