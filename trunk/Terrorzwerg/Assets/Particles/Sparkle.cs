using UnityEngine;
using System.Collections;

public class Sparkle : MonoBehaviour {

	// Use this for initialization
	void Start () 
	{
		particleSystem.Play();
		particleSystem.enableEmission = false;
	}
	
	// Update is called once per frame
	void Update () 
	{	
		bool tmpPlayerWithLightInRange = false;
		var tmpPlayers = FindObjectsOfType(typeof(Player));
		
		foreach (Player tmpPlayer in tmpPlayers)
		{
			if(tmpPlayer.LightOn && (tmpPlayer.Position - transform.position).magnitude < tmpPlayer.UnityLight.range*1.5f)
			{
				tmpPlayerWithLightInRange = true;
				break;
			}
		}
		
		if(tmpPlayerWithLightInRange)
		{
			particleSystem.enableEmission = true;
		}
		else
		{
			particleSystem.enableEmission = false;
		}
	}
}
