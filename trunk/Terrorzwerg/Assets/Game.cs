using UnityEngine;
using System.Collections;

public class Game : MonoBehaviour {
	
	bool IsGameRunning;
	int WinningTeam;
	
	public bool SomeoneHasLightOn  {
		get;
		private set;
	}
	
	// Use this for initialization
	void Start () {
		IsGameRunning = true;
	}
	
	// Update is called once per frame
	void Update () {
		var tmpPlayers = FindObjectsOfType(typeof(Player));
		SomeoneHasLightOn = false;
		foreach (Player tmpPlayer in tmpPlayers) {
			if(tmpPlayer.LightOn)
			{
				SomeoneHasLightOn = true;
				foreach(Player tmpEnemyPlayer in tmpPlayers)
				{
					float tmpDistance = Vector3.Distance(tmpPlayer.Position, tmpEnemyPlayer.Position);
					if(tmpPlayer != tmpEnemyPlayer && tmpEnemyPlayer.IsInEnemyTerritory && tmpDistance < tmpPlayer.UnityLight.range)
					{
						tmpEnemyPlayer.DoDamage((1.0f-(tmpDistance/tmpPlayer.UnityLight.range)) * 100 * Time.deltaTime);
					}
				}
				
			}
			if(tmpPlayer.HasFlag && !tmpPlayer.IsInEnemyTerritory)
			{
				GameOver(tmpPlayer.Team);
			}
		}
	}
	
	void GameOver(int iWinningTeam){
		WinningTeam = iWinningTeam;
		IsGameRunning = false;
	}
	
	void OnGUI () {
		string tmpInfoText;
		bool noFlag=true;
		string stealingTeam = "";
		
		var tmpPlayers = FindObjectsOfType(typeof(Player));
		foreach (Player tmpPlayer in tmpPlayers) {
			if(tmpPlayer.HasFlag && noFlag){
				noFlag=false;
				stealingTeam = " "+tmpPlayer.Team.ToString()+" has";
			}
			else if(tmpPlayer.HasFlag){
				stealingTeam = "s 1 and 2 have";
			}
		}

		if(IsGameRunning && noFlag)
		{
			tmpInfoText = "Go get the treasure!!!";
		}
		else if(IsGameRunning && !noFlag){
			tmpInfoText = "Team"+stealingTeam+" the treasure!";
		}
		else
		{
			tmpInfoText = "Team " + WinningTeam + " won!";
		}
		GUI.Label(new Rect(Screen.width/2 - 100,10,200,30), tmpInfoText);
	}
}