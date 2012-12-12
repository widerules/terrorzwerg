using UnityEngine;

public class GameData : MonoSingleton< GameData >
{
    public string ipAdress = "";
	public int playerId = 0;
	public int winningTeam=0;
    public override void Init(){ ipAdress="77.79.99.99"; playerId=0;winningTeam=-1; }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
