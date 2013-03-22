using UnityEngine;
using System.Collections;

public class Rainzone : MonoBehaviour {

    private float vSize = 3;

    public float Size
    {
        get { return vSize; }
        set { 
            vSize = value;
            print("...confirming rain zone size change to " + value);
            // Change size of the particle system and the collider.
            AnimationState tmpAnim = transform.GetComponentInChildren<ParticleSystem>().animation["RainZoneSize"];
            tmpAnim.time = Size;
            transform.GetComponentInChildren<ParticleSystem>().animation.Sample();
            transform.GetComponentInChildren<Projector>().orthographicSize = Size;
            transform.GetComponentInChildren<Projector>().orthoGraphicSize = Size;
        }
    }


	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
