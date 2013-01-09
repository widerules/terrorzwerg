using UnityEngine;
using System.Collections;

public class ScrollLava : MonoBehaviour {
    public float ScrollSpeed = 0.5f;
    float Scroll;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        Scroll += ScrollSpeed * Time.deltaTime;
        Scroll = Scroll % 1;
        renderer.material.mainTextureOffset = new Vector2(0, Scroll);
        renderer.material.SetTextureOffset("_BumpMap", new Vector2(0, Scroll));
	}
}
