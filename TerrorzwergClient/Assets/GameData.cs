using UnityEngine;

public class GameData : MonoSingleton< GameData >
{
    public string ipAdress = "";
    public string networkGUID = "";
    public int port = 6666;
	public int playerId = 0;
	public int winningTeam=0;
    public bool connectionFailed = false;
    public override void Init() { networkGUID = "";  ipAdress = "0.0.0.0"; port = 6666; playerId = -1; winningTeam = -1; connectionFailed = false; }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
