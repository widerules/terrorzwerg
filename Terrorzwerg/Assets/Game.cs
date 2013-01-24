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
    string NetworkGUID;
    public int Port = 5554;
    public int MinimumPlayers = 2;

    string port;

	bool noFlag=true;
	string stealingTeam = "";
	
	public Player basePlayer;
    System.Collections.Generic.Dictionary<NetworkPlayer, Player> Players = new System.Collections.Generic.Dictionary<NetworkPlayer,Player>();

    public float MaxPlayerConnectionTime = 10;
    public float MaxGameOverTime = 10;

    int PlayersReadyCount = 0;
    float PlayerConnectionTime = 10;
    float GameOverTime = 10;
    Texture2D TextureLogPlayer0;
    Texture2D TextureLogPlayer1;
    public Texture2D MenuBackground;
    public Texture2D MenuTitle;
    public Texture2D MenuBlueWon;
    public Texture2D MenuRedWon;

    public Texture2D[] MenuNumerals;

    public Color QRCodeForecolor;
    public Color QRCodeBackcolor;
    public int QRCodeSize = 180;
    public int QRDistance = 100;
    public int QRHeight = 400;
	public GameObject[] Obstacles;

    public AudioClip Fanfare;
    float MusicVolume = 1.0f;
    public float IngameMusicVolume = 0.3f;

	public bool SomeoneHasLightOn  {
		get;
		private set;
	}

    public bool EnableShadowCollision = true;

    public float RainZoneSize = 3;

    public GUISkin Skin;

	// Use this for initialization
	void Start () {
        GameState = eGameState.Menu;

        InitializeServer();
        InitializeMenu();
	}

    private void InitializeMenu()
    {
		//http://www.unet.univie.ac.at/~a0701760/terrorzwerg/TerrorzwergClient.apk?Zwegdata=127.0.0.1:666;0
        TextureLogPlayer0 = CreateQR("http://www.unet.univie.ac.at/~a0701760/terrorzwerg/TerrorzwergClient.apk?Zwegdata=" + IPAddress + ":" + Port + "," + NetworkGUID + ";0", QRCodeSize);
        TextureLogPlayer1 = CreateQR("http://www.unet.univie.ac.at/~a0701760/terrorzwerg/TerrorzwergClient.apk?Zwegdata=" + IPAddress + ":" + Port + "," + NetworkGUID + ";1", QRCodeSize);
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
                if (tmpMatrix.Array[i][j] == 0)
                {
                    tmpColor[tmpPos] = QRCodeBackcolor;
                }
                else
                {
                    tmpColor[tmpPos] = QRCodeForecolor;
                }
                //byte tmpCol = tmpMatrix.Array[i][j] == 0 ? (byte)0 : (byte)255;
                //tmpColor[tmpPos].r = tmpColor[tmpPos].g = tmpColor[tmpPos].b = tmpCol;
                //tmpColor[tmpPos].a = 255;
            }
        }

        tmpTex.SetPixels32(tmpColor);
        tmpTex.Apply();

        return tmpTex;
    }

    void InitializeServer()
    {
        int tmpTries = 3;
        int tmpTriesNat = 3;
        // ToDo: change to NAT Server.
        NetworkConnectionError tmpError;
        Debug.Log("Using NAT.");
        while ((tmpError = Network.InitializeServer(16, Port, true)) != NetworkConnectionError.NoError && tmpTriesNat > 0)
        {
            Debug.Log("Server: " + tmpError.ToString());
            Port += Random.Range(1, 12);
            tmpTriesNat--;
        }

        Debug.Log("Using public address.");
        while ((tmpError = Network.InitializeServer(16, Port, false)) != NetworkConnectionError.NoError && tmpTries > 0)
        {
            Debug.Log("Server: " + tmpError.ToString());
            Port += Random.Range(1, 12);
            tmpTries--;
        }

        MasterServer.RegisterHost("TerrorZwerg", Network.player.ipAddress + ":" + Network.player.port, "Test Game");
       
        ipadress = Network.player.ipAddress;
        Port = Network.player.port;
        NetworkGUID = Network.player.guid;
        Debug.Log("Server started at: " + ipadress + ":" + Port + " NAT GUID: " + NetworkGUID);
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
        if (Players.Count == PlayersReadyCount && PlayersReadyCount > 0)
        {
            PlayerConnectionTime -= Time.deltaTime;
        }
        if (PlayerConnectionTime <= 0)
        {
            PlayerConnectionTime = 0;
            StartNewGame();
        }
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
        if (MusicVolume < 1.0f)
            MusicVolume += Time.deltaTime;
        else
            MusicVolume = 1.0f;

        audio.volume = MusicVolume;
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
                        if (EnableShadowCollision)
                        {
                            Vector3 tmpRayVec = (tmpEnemyPlayer.Position - tmpPlayer.Position) + new Vector3(0, 0.5f, 0);
                            float tmpRayLength = tmpRayVec.magnitude;
                            Ray tmpColRay = new Ray(tmpPlayer.Position, tmpRayVec);
                            // Collide everything but players. It's too buggy with self collision etc.
                            int layerMask = 1 << 10;
                            layerMask = ~layerMask;
                            if (!Physics.Raycast(tmpColRay, tmpRayLength, layerMask))
                            {
                                //Debug.DrawLine(tmpPlayer.Position + new Vector3(0, 0.5f, 0), tmpEnemyPlayer.Position + new Vector3(0, 0.5f, 0), Color.magenta);
                                tmpEnemyPlayer.DoDamage((1.0f - (tmpDistance / tmpPlayer.UnityLight.range)) * 80 * Time.deltaTime);
                            }
                        }
                        else
                        {
                            tmpEnemyPlayer.DoDamage((1.0f - (tmpDistance / tmpPlayer.UnityLight.range)) * 80 * Time.deltaTime);
                        }
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
        if (Input.GetKey(KeyCode.Escape))
        {
            EndGame(0);
        }

        if (MusicVolume > IngameMusicVolume)
            MusicVolume -= Time.deltaTime;
        else
            MusicVolume = IngameMusicVolume;

        audio.volume = MusicVolume;
    }

    void UpdateGameOver()
    {
        GameOverTime -= Time.deltaTime;
        if (GameOverTime < 0)
        {
            PlayerConnectionTime = MaxPlayerConnectionTime;            
            //DisconnectAllPlayers();
            networkView.RPC("GameRestarted", RPCMode.All);
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

		RandomizeObstacles();
        var tmpFlag = GameObject.FindGameObjectsWithTag("Flag_0")[0];
        tmpFlag.transform.position = new Vector3(-20, 0, 0);
        tmpFlag.transform.localRotation = Quaternion.identity;
        tmpFlag = GameObject.FindGameObjectsWithTag("Flag_1")[0];
        tmpFlag.transform.position = new Vector3(20, 0, 0);
        tmpFlag.transform.localRotation = Quaternion.identity;

        GameState = eGameState.InGame;
        networkView.RPC("GameStarted", RPCMode.All);

        var tmpRainzones = FindObjectsOfType(typeof(Rainzone));
        foreach (Rainzone tmpZone in tmpRainzones)
        {
            tmpZone.Size = RainZoneSize;
        }
    }
	
	void RandomizeObstacles(){
		bool a=true;
		bool b=true;
		bool c=true;
		bool d=true;
		foreach(GameObject obstacle in Obstacles)
		{
			if(obstacle.name.Contains("0")){
				if(Random.value>0.5){
					obstacle.SetActiveRecursively(true);
					
				}
			}	
			else if(obstacle.name.Contains("1") && a)
			{
				if(Random.value<0.5){
					obstacle.SetActiveRecursively(true);
					a=false;
				}
			}
			else if(obstacle.name.Contains("2") && b){
				if(Random.value<0.5){
					obstacle.SetActiveRecursively(true);
					b=false;
				}
			}
			else if(obstacle.name.Contains("3") && c){
				if(Random.value<0.5){
					obstacle.SetActiveRecursively(true);
					c=false;
				}
			}
			else if(obstacle.name.Contains("4") && d){
				if(Random.value<0.5){
					obstacle.SetActiveRecursively(true);
					d=false;
				}
			}
		}
	}

	void EndGame(Player.eTeam iWinningTeam){
        AudioSource.PlayClipAtPoint(Fanfare, camera.transform.position, 1);
        GameOverTime = MaxGameOverTime; 
        WinningTeam = iWinningTeam;
        GameState = eGameState.GameOver;
        PlayersReadyCount = 0;
        networkView.RPC("GameOver", RPCMode.All, (int)iWinningTeam);
        //RemoveAllPlayers();
	}

    #region GUIStuff
    void OnGUI () {
        GUI.skin = Skin;
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
        //GUI.DrawTexture(new Rect(0,0,Screen.width, Screen.height), MenuBackground);
        GUI.DrawTextureWithTexCoords(new Rect(0, 0, Screen.width, Screen.height), MenuBackground, new Rect(0, 0, Screen.width / 500.0f, Screen.height/500.0f));

        if (Players.Count == PlayersReadyCount && PlayersReadyCount > 0)
        {
            int tmpTexId = Mathf.Clamp((int)PlayerConnectionTime, 0, (MenuNumerals.Length-1));
            GUI.DrawTexture(new Rect(Screen.width / 2 - 128, 130, 256, 256), MenuNumerals[tmpTexId]);
        }
        //GUI.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        //GUI.Label(new Rect(0, 0, Screen.width, 200), "Terror Zwerg");
        //GUI.color = Color.white;

        GUI.DrawTexture(new Rect(Screen.width / 2 - 400, -100, 800, 400), MenuTitle, ScaleMode.ScaleAndCrop, true);
        GUI.DrawTexture(new Rect(Screen.width / 2 - 400, -100, 800, 400), MenuTitle, ScaleMode.ScaleAndCrop, true);
        GUI.DrawTexture(new Rect(Screen.width / 2 - 400, -100, 800, 400), MenuTitle, ScaleMode.ScaleAndCrop, true);

        GUI.BeginGroup(new Rect(Screen.width / 2 - QRCodeSize - QRDistance, QRHeight, 270, 290));
        GUI.DrawTexture(new Rect(0, 0, QRCodeSize, QRCodeSize), TextureLogPlayer0, ScaleMode.ScaleAndCrop, true);
        GUI.EndGroup();

        GUI.BeginGroup(new Rect(Screen.width / 2 + QRDistance, QRHeight, 270, 290));
        GUI.DrawTexture(new Rect(0, 0, QRCodeSize, QRCodeSize), TextureLogPlayer1, ScaleMode.ScaleAndCrop, true);
        GUI.EndGroup();
    }

    void OnGUIGame()
    {
        string tmpInfoText;

        //if (noFlag)
        //{
        //    tmpInfoText = "Go get the treasure!!! (" + ipadress + ")";
        //}
        //else
        //{
        //    tmpInfoText = "Team" + stealingTeam + " the treasure!";
        //}
        ////tmpInfoText = ipadress;

        //GUI.Label(new Rect(Screen.width / 2 - 100, 10, 200, 30), tmpInfoText);
    }

    void OnGUIGameOver()
    {
        if (WinningTeam == Player.eTeam.Blue)
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), MenuBlueWon);
        }
        else
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), MenuRedWon);
        }
        //GUI.Label(new Rect(10, 10, 400, 20), "The game is over.");
        //GUI.Label(new Rect(10, 30, 400, 20), "Team " + WinningTeam.ToString() + " has won!!!");

        int tmpTexId = Mathf.Clamp((int)GameOverTime, 0, (MenuNumerals.Length - 1));
        GUI.DrawTexture(new Rect(Screen.width / 2 - 128, 130, 256, 256), MenuNumerals[tmpTexId]);
    }
    #endregion

    void OnDestroy()
    {
        //DisconnectAllPlayers();
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
        if (Players.TryGetValue(tmpNetworkInfo.sender, out tmpPlayer))
        {
            if(tmpTeam == Player.eTeam.Blue)
            {
                int tmpRand = Random.Range(0, 1);
                if(tmpRand == 0)
                    tmpPlayer.SetPositionAndTeam(new Vector3(-22, 1, 5) + new Vector3(tmpRandPos.x, 0, tmpRandPos.y), tmpTeam);
                else
                    tmpPlayer.SetPositionAndTeam(new Vector3(-22, 1, -5) + new Vector3(tmpRandPos.x, 0, tmpRandPos.y), tmpTeam);
            }
            else
            {
                int tmpRand = Random.Range(0, 1);
                if (tmpRand == 0)
                    tmpPlayer.SetPositionAndTeam(new Vector3(22, 1, 5) + new Vector3(tmpRandPos.x, 0, tmpRandPos.y), tmpTeam);
                else
                    tmpPlayer.SetPositionAndTeam(new Vector3(22, 1, -5) + new Vector3(tmpRandPos.x, 0, tmpRandPos.y), tmpTeam);

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

    [RPC]
    void GameRestarted()
    {

    }

	void OnPlayerConnected(NetworkPlayer iPlayer){
        Player tmpPlayer = (Player)Instantiate(basePlayer, new Vector3(0, -10000, 0), Quaternion.identity);
        tmpPlayer.nPlayer = iPlayer;

        Players.Add(iPlayer, tmpPlayer);

        Debug.Log("Player connected: " + iPlayer.ipAddress + ":" + iPlayer.port.ToString());
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

    public void Player_PlaySound(NetworkPlayer iPlayer, string iSound)
    {
        networkView.RPC("PlaySound", iPlayer, iSound);
    }

    [RPC]
    void PlaySound(string iSound)
    {
    }

    /// <summary>
    /// Client calls this when he is ready for the game.
    /// </summary>
    [RPC]
    void Ready()
    {
        PlayersReadyCount++;
    }
    #endregion
}
