using UnityEngine;

public class Tester : MonoBehaviour {

    [Resource( "" )]
    public Object[] Objects;
    
    [Resource( "Camera Prefab" )]
    public GameObject Prefab;
    [Resource( "Logo" )]
    public Texture2D Texture;
    [Resource( "SomeTextDoc" )]
    public TextAsset Text;

    void Start() {
        this.LoadResources();
    }
}
