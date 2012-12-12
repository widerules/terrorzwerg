using UnityEngine;
using System.Collections;

public class Game_noMinimap : MonoBehaviour
{
    NetworkViewID NetID;
    public int Health = 100;

    string ConnectionIP = "77.80.49.197"; //"131.130.238.217";
    string InfoString = "";

    bool InGame = false;

    public GameObject Player;

    int StrikeIndex = -1;
    Vector2 StrikeStart;
    bool Striking = false;
    float StrikingMatchAxis = 0;

    public AudioClip SoundStrikeMatch;

    // Use this for initialization
    void Start()
    {
		ConnectionIP=GameData.instance.ipAdress;
		
		ConnectToServer(ConnectionIP);
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
                        AudioSource.PlayClipAtPoint(SoundStrikeMatch, camera.transform.position, 1);
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
        if (!InGame)
        {
            GUI.BeginGroup(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 50, 400, 100));

            GUI.Label(new Rect(5, 80, 400, 20), InfoString);

            GUI.EndGroup();
        }
    }

    void ConnectToServer(string iIP)
    {
        Network.Connect(iIP, 6666);

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
		Application.LoadLevel("Game_Menu");
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
        if (iCollided)
        {
            StartCoroutine(CollisionResponse());
        }
    }
	
	[RPC]
	void SetPlayerTeam(int team){
		
	}
	
	[RPC]
	void GameStarted(){
		InfoString ="Game Starts";
		InGame=true;
		StartCoroutine(UpdateNetwork());
	}
	
	[RPC]
	void GameOver(int WinningTeam){
		GameData.instance.WinningTeam=WinningTeam;
	}
	
    IEnumerator CollisionResponse()
    {
        Handheld.Vibrate();
        yield return new WaitForSeconds(1.0f);
        yield return false;
    }

}
