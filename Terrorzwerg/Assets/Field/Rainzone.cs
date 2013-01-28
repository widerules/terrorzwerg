using UnityEngine;
using System.Collections;

public class Rainzone : MonoBehaviour {

    private float vSize = 3;

    public float Size
    {
        get { return vSize; }
        set { 
            vSize = value;

            // Change size of the particle system and the collider.
            transform.GetComponentInChildren<ParticleSystem>().animation["RainZoneSize"].time = Size;
        }
    }


	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
