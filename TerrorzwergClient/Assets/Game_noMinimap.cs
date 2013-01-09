using UnityEngine;
using System.Collections;

public class Game_noMinimap : MonoBehaviour
{
    NetworkViewID NetID;
    public int Health = 100;

    string ConnectionIP = "0.0.0.0"; //"131.130.238.217";
    int ConnectionPort = 6666;
    string InfoString = "";

    bool InGame = false;
	bool currColl = false;
	
    public GameObject Player;

    int StrikeIndex = -1;
    Vector2 StrikeStart;
    bool Striking = false;
    float StrikingMatchAxis = 0;

    public AudioClip SoundStrikeMatch;
    public AudioClip SoundHurt;
    public AudioClip[] SoundDie;

    public Texture StrikingTexture;
    public Texture TexPrepare_Red;
    public Texture TexPrepare_Blue;
    public Texture TexWon;
    public Texture TexLost;

    // Use this for initialization
    void Start()
    {
        GameData.instance.winningTeam = -1;
        ConnectionIP = GameData.instance.ipAdress;
        ConnectionPort = GameData.instance.port;

        ConnectToServer(ConnectionIP, ConnectionPort);
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
        Rect tmpFull = new Rect(0, 0, Screen.width, Screen.height);
        if (!InGame)
        {

            //GUI.BeginGroup(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 50, 400, 100));

            //GUI.Label(new Rect(5, 80, 400, 20), InfoString);

            //GUI.EndGroup();
            if (GameData.instance.playerId == 0)
            {
                GUI.DrawTexture(tmpFull, TexPrepare_Blue);
            }
            else
            {
                GUI.DrawTexture(tmpFull, TexPrepare_Red);
            }
        }
        else
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), StrikingTexture);
        }
        if (GameData.instance.winningTeam == GameData.instance.playerId)
        {
            GUI.DrawTexture(tmpFull, TexWon);
        }
        else if(GameData.instance.winningTeam != -1)
        {
            GUI.DrawTexture(tmpFull, TexLost);
        }
    }

    void ConnectToServer(string iIP, int iPort)
    {
        Network.Connect(iIP, iPort);

    }

    void OnFailedToConnect(NetworkConnectionError error)
    {
        InfoString = "Connection Failed: " + error;
    }

    void OnConnectedToServer()
    {
		InfoString = "Connected to "+GameData.instance.ipAdress +" - you are in Team "+GameData.instance.playerId;

		networkView.RPC("SetPlayerTeam",RPCMode.Server,GameData.instance.playerId);

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
    void PlayStrikingSound()
    {
        AudioSource.PlayClipAtPoint(SoundStrikeMatch, camera.transform.position, 1);
    }

    [RPC]
    void PlayHurtSound()
    {
        AudioSource.PlayClipAtPoint(SoundHurt, camera.transform.position, 1);
    }

    [RPC]
    void PlayDeathSound()
    {
        int tmpRand = Random.Range(0, SoundDie.Length);

        AudioSource.PlayClipAtPoint(SoundDie[tmpRand], camera.transform.position, 1);
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
	
    IEnumerator CollisionResponse()
    {
        Handheld.Vibrate();
        currColl=true;
        yield return new WaitForSeconds(1.0f);
		currColl=false;
        yield return false;
    }

}
