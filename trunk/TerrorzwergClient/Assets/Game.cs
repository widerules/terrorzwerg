using UnityEngine;
using System.Collections;

public class Game : MonoBehaviour {
	NetworkViewID NetID;
	public int Health = 100;
	
	string ConnectionIP = "131.130.109.157"; //"77.80.49.197"; //"131.130.238.217";
	string InfoString = "";
	
	bool InGame = false;
	
	public GameObject Player;
	
	int StrikeIndex = -1;
	Vector2 StrikeStart;
	bool Striking = false;
	float StrikingMatchAxis = 0;
	
	public Texture2D HealthTex;
	
	public AudioClip SoundStrikeMatch;
	
	// Use this for initialization
	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		foreach (var tmpTouch in Input.touches) 
		{
			if(!Striking && tmpTouch.phase == TouchPhase.Began)
			{
				StrikeIndex = tmpTouch.fingerId;
				StrikeStart = tmpTouch.position;
			}
			else if (tmpTouch.phase == TouchPhase.Ended)
			{
				if( tmpTouch.fingerId == StrikeIndex)
				{
					float tmpLength = (tmpTouch.position-StrikeStart).magnitude;
					if(tmpLength > Screen.width*0.8f)
					{
						StrikingMatchAxis = 1.0f;
						AudioSource.PlayClipAtPoint(SoundStrikeMatch,camera.transform.position,1);
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
		if(InGame)
		{
			int HealthSize = (int)(Screen.height * (float)(Health)/100.0f);
			Color tmp = GUI.color;
			GUI.color = Color.green;
			GUI.DrawTexture(new Rect(Screen.width-40,Screen.height  - HealthSize, 40, HealthSize), HealthTex);
			//GUI.Box(new Rect(Screen.width-20,Screen.height  - HealthSize, 20, HealthSize),"");
			GUI.color = tmp;
		}
		else
		{
			GUI.BeginGroup(new Rect(Screen.width/2-100,Screen.height/2-50,200,100));
			
			ConnectionIP = GUI.TextField(new Rect(5,10,190,20),ConnectionIP);
			
			if(GUI.Button (new Rect(5,30,190,40),"Connect to Server"))
			{
				ConnectToServer(ConnectionIP);	
			}
			
			GUI.Label(new Rect(5,80,190,20), InfoString);
			
			GUI.EndGroup();
		}
	}

	void ConnectToServer (string iIP)
	{
		Network.Connect(iIP, 6666);

	}
	
	void OnFailedToConnect(NetworkConnectionError error) 
	{
        InfoString = "Connection Failed: " + error;
    }
	
	 void OnConnectedToServer() 
	{
        InfoString = "Connection Succeeded!";
		InGame = true;
		StartCoroutine(UpdateNetwork());
    }
	
	IEnumerator UpdateNetwork()
	{
		while(InGame)
		{
			UpdatePlayerPosition();
			yield return new WaitForSeconds(1.0f/15.0f);	
		}
	}
	
	
	void UpdatePlayerPosition()
	{
		var tmpHorizontal = -Input.acceleration.y; //Input.GetAxis("Horizontal");
		var tmpVertical =  Input.acceleration.x; //Input.GetAxis("Vertical");
		networkView.RPC("MovePlayer",RPCMode.Server,tmpHorizontal,tmpVertical,StrikingMatchAxis);
		
		StrikingMatchAxis = 0;
	}
	
	
	[RPC]
	void SetHealth(int iHealth)
	{
		Health = iHealth;
	}
	
	[RPC]
	void MovePlayer(float iXAxis, float iYAxis, float iLight)
	{
		
	}

	[RPC]
	void SetPlayerPosition(Vector3 iPosition, bool iCollided)
	{
		Player.transform.position = iPosition;
		
		if(iCollided)
		{
			StartCoroutine(CollisionResponse());
		}
	}
	
	IEnumerator CollisionResponse()
	{
		Handheld.Vibrate();
		yield return new WaitForEndOfFrame();
		Handheld.Vibrate();
		yield return new WaitForEndOfFrame();
		Handheld.Vibrate();
		yield return new WaitForEndOfFrame();
		Handheld.Vibrate();
		yield return new WaitForEndOfFrame();
		Handheld.Vibrate();
		yield return new WaitForEndOfFrame();
		Handheld.Vibrate();

		yield return false;
	}

}
