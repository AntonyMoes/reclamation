using Map;
using UnityEngine;

public class TestBehaviour : MonoBehaviour {
    [SerializeField] private TextAsset testShapeText;
    
    private void Start() {
        var rng = new Random.Random(UnityEngine.Random.Range(0, int.MaxValue));
        Debug.Log($"Seed: {rng.Seed}, float: {rng.NextFloat(0, 100)}");

        var testShape = TileShape.FromCsv(testShapeText.text);
        Debug.Log(testShape.ToCsv());
    }
}
