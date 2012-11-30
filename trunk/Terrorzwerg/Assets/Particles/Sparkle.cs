using UnityEngine;
using System.Collections;

public class Sparkle : MonoBehaviour {

	// Use this for initialization
	void Start () {
		particleSystem.Play();
		particleSystem.enableEmission = false;
	}
	
	// Update is called once per frame
	void Update () {	
		Game tmpGame = (Game)FindObjectOfType(typeof(Game));
		if(tmpGame == null)
			return;
		
		if(tmpGame.SomeoneHasLightOn)
		{
			particleSystem.enableEmission = true;
		}
		else
		{
			particleSystem.enableEmission = false;
		}
	}
}
