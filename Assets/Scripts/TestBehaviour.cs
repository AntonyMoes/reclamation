using Map;
using UnityEngine;

public class TestBehaviour : MonoBehaviour {
    [SerializeField] private TextAsset testShapeText;
    
    private void Start() {
        var rng = new Random.Random(UnityEngine.Random.Range(0, int.MaxValue));
        Debug.Log($"Seed: {rng.Seed}, float: {rng.NextFloat(0, 100)}");

        var testShape = TileShape.FromCsv(testShapeText.text);
        Debug.Log(testShape.Size);

        Debug.Log(testShape.ToCsv());
        Debug.Log(testShape.Rotate(1).ToCsv());
        Debug.Log(testShape.Rotate(2).ToCsv());
        Debug.Log(testShape.Rotate(3).ToCsv());
        Debug.Log(testShape.Rotate(4).ToCsv());

        Debug.Log(testShape[new Vector2Int(0, 0)]);
        Debug.Log(testShape[new Vector2Int(0, 2)]);
        Debug.Log(testShape[new Vector2Int(2, 1)]);
    }
}
