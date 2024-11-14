using UnityEngine;

public class MaterialHolder : MonoBehaviour {
    [SerializeField]
    private Material storedMaterial;

    public Material GetMaterial() {
        return storedMaterial;
    }
}
