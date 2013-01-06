using UnityEngine;
using System.Collections;
using com.google.zxing.qrcode;
using com.google.zxing.common;

public class Game : MonoBehaviour {

    public enum eGameState
    {
        Menu,
        InGame,
        GameOver
    }
    private eGameState vGameState;
    public eGameState GameState
    {
        get { return vGameState; }
        private set { vGameState = value; }
    }
    
	Player.eTeam WinningTeam;
	
	string ipadress;
    public string IPAddress
    {
        get { return ipadress; }
        private set { ipadress = value; }
    }

    public int Port = 5554;
    public int MinimumPlayers = 1;

    string port;

	bool noFlag=true;
	string stealingTeam = "";
	
	public Player basePlayer;
    System.Collections.Generic.Dictionary<NetworkPlayer, Player> Players = new System.Collections.Generic.Dictionary<NetworkPlayer,Player>();

    public float MaxPlayerConnectionTime = 10;
    public float MaxGameOverTime = 10;

    float PlayerConnectionTime = 10;
    float GameOverTime = 10;
    Texture2D TextureLogPlayer0;
    Texture2D TextureLogPlayer1;


	public bool SomeoneHasLightOn  {
		get;
		private set;
	}
	
	// Use this for initialization
	void Start () {
        GameState = eGameState.Menu;

        InitializeServer();
        InitializeMenu();
	}

    private void InitializeMenu()
    {
        TextureLogPlayer0 = CreateQR(IPAddress + ":" + Port + ";0", 256);
        TextureLogPlayer1 = CreateQR(IPAddress + ":" + Port + ";1", 256);
    }

    Texture2D CreateQR(string iQRString, int iSize)
    {
        Texture2D tmpTex = new Texture2D(iSize, iSize);

        QRCodeWriter tmpWriter = new QRCodeWriter();
        ByteMatrix tmpMatrix = tmpWriter.encode(iQRString, com.google.zxing.BarcodeFormat.QR_CODE, iSize, iSize);

        Color32[] tmpColor = new Color32[iSize * iSize];

        for (int i = 0; i < iSize; i++)
        {
            for (int j = 0; j < iSize; j++)
            {
                int tmpPos = j * iSize + i;
                byte tmpCol = tmpMatrix.Array[i][j] == 0 ? (byte)0 : (byte)255;
                tmpColor[tmpPos].r = tmpColor[tmpPos].g = tmpColor[tmpPos].b = tmpCol;
                tmpColor[tmpPos].a = 255;
            }
        }

        tmpTex.SetPixels32(tmpColor);
        tmpTex.Apply();

        return tmpTex;
    }

    void InitializeServer()
    {
        // TODO change to NAT Server.
        Network.InitializeServer(16, Port);
        ipadress = Network.player.ipAddress;	    
    }

	// Update is called once per frame
	void Update () {
        switch (vGameState)
        {
            case eGameState.Menu:
                UpdateMenu();
                break;
            case eGameState.InGame:
                UpdateGame();
                break;
            case eGameState.GameOver:
                UpdateGameOver();
                break;
            default:
                break;
        }
	}


    void UpdateMenu()
    {
        if (Players.Count >= MinimumPlayers)
        {
            PlayerConnectionTime -= Time.deltaTime;
        }
        if (PlayerConnectionTime <= 0)
        {
            PlayerConnectionTime = 0;
            StartNewGame();
        }
    }

    void UpdateGame()
    {
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
                EndGame(tmpPlayer.Team);
			}
        }
    }

    void UpdateGameOver()
    {
        GameOverTime -= Time.deltaTime;
        if (GameOverTime < 0)
        {
            PlayerConnectionTime = MaxPlayerConnectionTime;            
            DisconnectAllPlayers();
            GameState = eGameState.Menu;
        }
    }

    void StartNewGame()
    {
        // Reset level.
        var tmpCoins = FindObjectsOfType(typeof(Coin));
        foreach (Coin tmpCoin in tmpCoins)
        {
            Destroy(tmpCoin.gameObject);
        }
        var tmpFlag = GameObject.FindGameObjectsWithTag("Flag_0")[0];
        tmpFlag.transform.position = new Vector3(-20, 0, 0);
        tmpFlag = GameObject.FindGameObjectsWithTag("Flag_1")[0];
        tmpFlag.transform.position = new Vector3(20, 0, 0);

        GameState = eGameState.InGame;
        networkView.RPC("GameStarted", RPCMode.All);
    }

	void EndGame(Player.eTeam iWinningTeam){
        GameOverTime = MaxGameOverTime; 
        WinningTeam = iWinningTeam;
        GameState = eGameState.GameOver;
        networkView.RPC("GameOver", RPCMode.All, (int)iWinningTeam);
        RemoveAllPlayers();
	}
	
	void OnGUI () {
        switch (vGameState)
        {
            case eGameState.Menu:
                OnGUIMenu();
                break;
            case eGameState.InGame:
                OnGUIGame();
                break;
            case eGameState.GameOver:
                OnGUIGameOver();
                break;
            default:
                break;
        }
		
	}

    void OnGUIMenu()
    {
        if (Players.Count >= 2)
        {
            GUI.Label(new Rect(10, 10, 400, 20), "Time left to connect: " + (int)PlayerConnectionTime + " seconds...");
        }
        else
        {
            GUI.Label(new Rect(10, 10, 400, 20), "Please connect by hovering over the QR codes.");
        }

        GUI.Label(new Rect(10, 30, 400, 20), "Players connected: " + Players.Count);

        GUI.BeginGroup(new Rect(Screen.width / 2 - 400, 100, 270, 290), "Team 1");
        GUI.Label(new Rect(7, 0, 256, 25), "Team 1");
        GUI.DrawTexture(new Rect(7, 27, 256, 256), TextureLogPlayer0);
        GUI.EndGroup();

        GUI.BeginGroup(new Rect(Screen.width / 2 + 144, 100, 270, 290), "Team 2");
        GUI.Label(new Rect(7, 0, 256, 25), "Team 2"); 
        GUI.DrawTexture(new Rect(7, 27, 256, 256), TextureLogPlayer1);
        GUI.EndGroup();
    }

    void OnGUIGame()
    {
        string tmpInfoText;

        if (noFlag)
        {
            tmpInfoText = "Go get the treasure!!! (" + ipadress + ")";
        }
        else
        {
            tmpInfoText = "Team" + stealingTeam + " the treasure!";
        }
        //tmpInfoText = ipadress;

        GUI.Label(new Rect(Screen.width / 2 - 100, 10, 200, 30), tmpInfoText);
    }

    void OnGUIGameOver()
    {
        GUI.Label(new Rect(10, 10, 400, 20), "The game is over.");
        GUI.Label(new Rect(10, 30, 400, 20), "Team " + WinningTeam.ToString() + " has won!!!");
        GUI.Label(new Rect(10, 50, 400, 20), "New game starts in: " + (int)GameOverTime + " seconds...");
    }

    void OnDestroy()
    {
        
    }
    #region NetworkStuff
    public void SendHealth(int Health, NetworkPlayer player){	
		networkView.RPC("SetHealth", player, Health);
	}	
	
	[RPC]
	void SetHealth(int Health){
		
	}
	
	[RPC]
	void MovePlayer(float x, float y, float light, NetworkMessageInfo nmi){
        if (Players.ContainsKey(nmi.sender))
        {
            var tmpPlayer = Players[nmi.sender];
            tmpPlayer.xAxis = x;
            tmpPlayer.yAxis = y;
            tmpPlayer.LightButton = light;
            networkView.RPC("SetPlayerPosition", nmi.sender, tmpPlayer.Position, tmpPlayer.vibrate);
            tmpPlayer.vibrate = false;
        }
	}
	
	[RPC]
	void SetPlayerPosition(Vector3 iPosition, bool vibrate){
		
	}

    [RPC]
    void SetPlayerTeam(int iTeam, NetworkMessageInfo tmpNetworkInfo)
    {
        Player.eTeam tmpTeam = (Player.eTeam)iTeam;
        var tmpPlayers = FindObjectsOfType(typeof(Player));

        Vector2 tmpRandPos = Random.insideUnitCircle * 2;
        Player tmpPlayer = null;
        if(Players.TryGetValue(tmpNetworkInfo.sender, out tmpPlayer))
        {
            if(tmpTeam == Player.eTeam.Blue)
            {
                tmpPlayer.SetPositionAndTeam(new Vector3(-22, 1, 0) + new Vector3(tmpRandPos.x, 0, tmpRandPos.y), tmpTeam);
            }
            else
            {
                tmpPlayer.SetPositionAndTeam(new Vector3(22, 1, 0) + new Vector3(tmpRandPos.x, 0, tmpRandPos.y), tmpTeam);
            }
        }
    }

    [RPC]
    void GameStarted()
    {

    }

    [RPC]
    void GameOver(int iWinningTeam)
    {

    }

	void OnPlayerConnected(NetworkPlayer iPlayer){
        Player tmpPlayer = (Player)Instantiate(basePlayer, new Vector3(0, -10000, 0), Quaternion.identity);
        tmpPlayer.nPlayer = iPlayer;

        Players.Add(iPlayer, tmpPlayer);

        // Reset connection timer.
        PlayerConnectionTime = MaxPlayerConnectionTime;
    }

    void OnPlayerDisconnected(NetworkPlayer iPlayer)
    {
        // Remove player from list and destroy him.
        if (Players.ContainsKey(iPlayer))
        {
            var tmpPlayer = Players[iPlayer];
            DestroyObject(tmpPlayer.gameObject);
            Players.Remove(iPlayer);
        }

        // Quit game if no player left.
        if (GameState == eGameState.InGame && Players.Count < 1)
        {
            EndGame(Player.eTeam.Blue);
        }
    }

    void DisconnectAllPlayers()
    {
        foreach (var tmpPlayer in Players)
        {
            Network.CloseConnection(tmpPlayer.Key, true);
        }

        Players.Clear();
    }

    void RemoveAllPlayers()
    {
        foreach (var tmpPlayer in Players)
        {
            DestroyObject(tmpPlayer.Value.gameObject);
        }
    }
    #endregion
}
