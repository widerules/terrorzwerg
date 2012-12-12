using UnityEngine;

public class GameData : MonoSingleton< GameData >
{
    public string ipAdress = "";
	public int playerId = 0;
	public int winningTeam=0;
    public override void Init(){ ipAdress="0.0.0.0"; playerId=-1;winningTeam=-1; }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
