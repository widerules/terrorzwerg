using UnityEngine;
using System.Collections;

public class Game_noMinimap : MonoBehaviour
{
    NetworkViewID NetID;
    public int Health = 100;


    public GUISkin TerrorSkin;
    string ConnectionIP = "0.0.0.0"; //"131.130.238.217";
    int ConnectionPort = 6666;
    string InfoString = "";

    bool InGame = false;
	bool currColl = false;
	bool showReadyButton = false;
	
    public GameObject Player;

    int StrikeIndex = -1;
    Vector2 StrikeStart;
    bool Striking = false;
    float StrikingMatchAxis = 0;
    float ConnectionFailedTime = 5;

    public AudioClip SoundStrikeMatch;
    public AudioClip SoundHurt;
    public AudioClip SoundCoin;
    public AudioClip SoundExtinguish;
    public AudioClip[] SoundDie;
	public AudioClip[] SoundWalk;
	

    public Texture StrikingTexture;
    public Texture TexPrepare_Red;
    public Texture TexPrepare_Blue;
    public Texture TexWon;
    public Texture TexLost;
    public Texture TexConnectionFailed;

    public Font GUIFont;

    // Use this for initialization
    void Start()
    {

        GameData.instance.connectionFailed = false;
        GameData.instance.winningTeam = -1;

        MasterServer.RequestHostList("TerrorZwerg");
        ConnectToServer();
    	InfoString="Connecting ... ";
	}

    // Update is called once per frame
    void Update()
    {
        foreach (var tmpTouch in Input.touches)
        {
            if (!Striking && tmpTouch.phase == TouchPhase.Began)
            {
                StrikeIndex = tmpTouch.fingerId;
                StrikeStart = tmpTouch.position;
            }
            else if (tmpTouch.phase == TouchPhase.Ended)
            {
                if (tmpTouch.fingerId == StrikeIndex)
                {
                    float tmpLength = (tmpTouch.position - StrikeStart).magnitude;
                    if (tmpLength > Screen.width * 0.3f)
                    {
                        StrikingMatchAxis = 1.0f;
                    }
                    Striking = false;
                }
            }
        }

        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    void OnGUI()
    {
        GUI.skin = TerrorSkin;
        Rect tmpFull = new Rect(0, 0, Screen.width, Screen.height);
        if (!InGame)
        {

            //GUI.BeginGroup(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 50, 400, 100));

            //GUI.Label(new Rect(5, 80, 400, 20), InfoString);

            //GUI.EndGroup();
            if (GameData.instance.playerId == 0)
            {
				//ready button
                GUI.DrawTexture(tmpFull, TexPrepare_Blue);
				if ( showReadyButton && GUI.Button(new Rect(Screen.width/2-100,Screen.height/2-50,200,100),"Team Blue - READY!") ){
					networkView.RPC("Ready",RPCMode.Server);
					showReadyButton=false;
				}
       				
            }
            else
            {
				// ready button
                GUI.DrawTexture(tmpFull, TexPrepare_Red);
				if ( showReadyButton && GUI.Button(new Rect(Screen.width/2-100,Screen.height/2-50,200,100),"Team Red - READY!") ){
					networkView.RPC("Ready",RPCMode.Server);
					showReadyButton=false;
				}
            }
			// back button
			if( showReadyButton && GUI.Button(new Rect(Screen.width/2-100,Screen.height/2+100,200,100),"Back")){
                Network.CloseConnection(Network.connections[0], true);
				Application.LoadLevel("Client_Menu");
			}
        }
        else
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), StrikingTexture);
        }
		
        if (!showReadyButton && GameData.instance.winningTeam == GameData.instance.playerId)
        {
            GUI.DrawTexture(tmpFull, TexWon);
        }
        else if(!showReadyButton && GameData.instance.winningTeam != -1)
        {
            GUI.DrawTexture(tmpFull, TexLost);
        }
		
		
        if (GameData.instance.connectionFailed)
        {
            ConnectionFailedTime -= Time.deltaTime;
            GUI.DrawTexture(tmpFull, TexConnectionFailed);
            if (ConnectionFailedTime <= 0)
            {
                Application.LoadLevel("Client_Menu");
				//InGame=false;
           }
        }
    }

    int tmpConnectionTriesNAT = 2;
    int tmpConnectionTries = 2;

    void ConnectToServer()
    {
        if (tmpConnectionTries > 0)
        {
            Network.Connect(GameData.instance.ipAdress, GameData.instance.port);
            tmpConnectionTries--;
        }
        else if (tmpConnectionTriesNAT > 0)
        {
            HostData[] tmpData = MasterServer.PollHostList();
            foreach (var tmpGame in tmpData)
            {
                if (tmpGame.gameName == GameData.instance.ipAdress + ":" + GameData.instance.port)
                {
                    Network.Connect(tmpGame);
                }
            }
            tmpConnectionTriesNAT--;
        }
        else
        {
            GameData.instance.connectionFailed = true;
        }
    }

    void OnFailedToConnect(NetworkConnectionError error)
    {
        // Try again.
        ConnectToServer();
    }

    void OnConnectedToServer()
    {
		InfoString = "Connected to "+GameData.instance.ipAdress +" - you are in Team "+GameData.instance.playerId;

		networkView.RPC("SetPlayerTeam",RPCMode.Server,GameData.instance.playerId);
		
		showReadyButton = true;

    }
	void OnDisconnectedFromServer(NetworkDisconnection info) {
		Application.LoadLevel("Client_Menu");
	}

    IEnumerator UpdateNetwork()
    {
        while (InGame)
        {
            UpdatePlayerPosition();
            yield return new WaitForSeconds(1.0f / 15.0f);
        }
    }


    void UpdatePlayerPosition()
    {
        var tmpHorizontal = -Input.acceleration.y; //Input.GetAxis("Horizontal");
        var tmpVertical = Input.acceleration.x; //Input.GetAxis("Vertical");
        networkView.RPC("MovePlayer", RPCMode.Server, tmpHorizontal, tmpVertical, StrikingMatchAxis);

        StrikingMatchAxis = 0;
    }


    [RPC]
    void MovePlayer(float iXAxis, float iYAxis, float iLight)
    {

    }

    [RPC]
    void SetHealth(int iHealth)
    {
        //Health = iHealth;
    }

    [RPC]
    void SetPlayerPosition(Vector3 iPosition, bool iCollided)
    {
        if (iCollided && !currColl)
        {
            StartCoroutine(CollisionResponse());
        }
    }
	
	[RPC]
	void SetPlayerTeam(int team){
		
	}

    [RPC]
    void PlaySound(string iSound)
    {
		if(InGame){
	        switch (iSound)
	        {
	            case "Striking":
	                AudioSource.PlayClipAtPoint(SoundStrikeMatch, camera.transform.position, 1);
	                break;
	            case "Hurt":
	                AudioSource.PlayClipAtPoint(SoundHurt, camera.transform.position, 1);
	                break;
	            case "Death":
	     		    int tmpRandD = Random.Range(0,SoundDie.Length);
	                AudioSource.PlayClipAtPoint(SoundDie[tmpRandD], camera.transform.position, 1);
	                break;
	            case "Walk":
	     		    int tmpRandW = Random.Range(0,SoundWalk.Length);
			        AudioSource.PlayClipAtPoint(SoundWalk[tmpRandW],camera.transform.position, 0.3f);
	                break;
	            case "Extinguish":
	                AudioSource.PlayClipAtPoint(SoundExtinguish, camera.transform.position, 0.3f);
	                break;
	            case "Coin":
	                AudioSource.PlayClipAtPoint(SoundCoin, camera.transform.position, 0.3f);
	                break;
	            default:
	                break;
	        }
		}
    }

	[RPC]
	void GameStarted(){
		InfoString ="Game Starts";
		InGame=true;
		StartCoroutine(UpdateNetwork());
	}
	
	[RPC]
	void GameOver(int WinningTeam){
		GameData.instance.winningTeam=WinningTeam;
		InGame=false;
		if(WinningTeam==GameData.instance.playerId){
			InfoString = "Your Team Won!";
		}
		else{
			InfoString = "Your Team LOST!";
		}
	}
	[RPC]
	void Ready(){}
	
	[RPC]
	void GameRestarted(){
		InGame=false;
		showReadyButton=true;
	}
	
	
    IEnumerator CollisionResponse()
    {
        Handheld.Vibrate();
        currColl=true;
        yield return new WaitForSeconds(1.0f);
		currColl=false;
        yield return false;
    }

}
