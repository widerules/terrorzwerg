using UnityEngine;
using System.Collections;

public class Game : MonoBehaviour {
	
	bool IsGameRunning;
	Player.eTeam WinningTeam;
	
	string ipadress;
	string port;
	
	bool playerConn=false;
	bool noFlag=true;
	string stealingTeam = "";
	
	int LastPlayerTeam = 1;
	
	System.Collections.Generic.List<NetworkPlayer> nPlayers=new System.Collections.Generic.List<NetworkPlayer>();
	public Player basePlayer;
	
	public bool SomeoneHasLightOn  {
		get;
		private set;
	}
	
	// Use this for initialization
	void Start () {
		IsGameRunning = true;
		Network.InitializeServer(16,6666);
		ipadress = Network.player.ipAddress;
	
	}
	
	// Update is called once per frame
	void Update () {
		var tmpPlayers = FindObjectsOfType(typeof(Player));
		SomeoneHasLightOn = false;
		noFlag=true;
		stealingTeam="";
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
			if(tmpPlayer.HasFlag && noFlag){
				noFlag=false;
				stealingTeam = " "+tmpPlayer.Team.ToString()+" has";
			}
			else if(tmpPlayer.HasFlag){
				stealingTeam = "s 1 and 2 have";
			}
			if(tmpPlayer.HasFlag && !tmpPlayer.IsInEnemyTerritory)
			{
				GameOver(tmpPlayer.Team);
			}
		}
	}
	
	void GameOver(Player.eTeam iWinningTeam){
		WinningTeam = iWinningTeam;
		IsGameRunning = false;
	}
	
	void OnGUI () {
		string tmpInfoText;

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
		//tmpInfoText = ipadress;
		
		GUI.Label(new Rect(Screen.width/2 - 100,10,200,30), tmpInfoText);
		
	}
	
	public void SendHealth(int Health,NetworkPlayer player){
		
		networkView.RPC("SetHealth",player,Health);
	}	
	
	[RPC]
	void SetHealth(int Health){
		
	}
	
	[RPC]
	void MovePlayer(float x, float y,float light,NetworkMessageInfo nmi){
		var tmpPlayers = FindObjectsOfType(typeof(Player));
		foreach(Player tmpPlayer in tmpPlayers){
			if(tmpPlayer.nPlayer==nmi.sender){
				tmpPlayer.xAxis = x;
				tmpPlayer.yAxis = y;
				tmpPlayer.LightButton=light;
				networkView.RPC("SetPlayerPosition",nmi.sender,tmpPlayer.Position,tmpPlayer.vibrate);
				tmpPlayer.vibrate=false;
			}
		}
	}
	
	[RPC]
	void SetPlayerPosition(Vector3 iPosition,bool vibrate){
		
	}

	
	void OnPlayerConnected(NetworkPlayer player){
		playerConn=true;
		nPlayers.Add(player);
		Player tmpPl  = (Player)Instantiate(basePlayer, new Vector3(0,0,0), Quaternion.identity);
		tmpPl.nPlayer = player;
		
		Vector2 tmpRandPos = Random.insideUnitCircle*2;
		if(LastPlayerTeam == 0)
		{
			tmpPl.Team = Player.eTeam.Red;
			tmpPl.StartPosition = new Vector3(22,0,0) + new Vector3(tmpRandPos.x,0,tmpRandPos.y);
		}
		else
		{
			tmpPl.Team = Player.eTeam.Blue;
			tmpPl.StartPosition = new Vector3(-22,0,0) + new Vector3(tmpRandPos.x,0,tmpRandPos.y);
		}
		
		LastPlayerTeam = 1-LastPlayerTeam;
	}
}
