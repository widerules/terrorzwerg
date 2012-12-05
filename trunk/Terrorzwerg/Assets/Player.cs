using UnityEngine;
using System.Collections;

#if UNITY_STANDALONE_WIN
using XInputDotNetPure;
#endif

public class Player : MonoBehaviour {
	public enum eTeam
	{
		Blue,
		Red
	}
	
	public Vector3 StartPosition;
	public Vector3 Position;
	bool vLightOn;
	public Light UnityLight;
	
	public bool LightOn {
		get {
			return vLightOn;
		}
		set {
			vLightOn = value;
			UnityLight.enabled = value;
		}
	}
	public bool IsInEnemyTerritory = false;
	public bool HasFlag = false;
	public float MaximumRunSpeed = 1;
	public float MaximumRunSpeedWithTreasure = 0.7f;
	public float LightTimeSeconds = 5;
	public float LightReloadTimeSeconds = 2;
	
	public float Health = 100;
	
	public eTeam Team = eTeam.Blue;
	int TeamNumber;
	
	
	public AudioClip SoundDie;
	public AudioClip SoundCapture;
	public AudioClip SoundDropCoin;
	
	Collider FlagCollider;
	Vector3 FlagStartPos;
	
	public Coin BaseCoin;
	float CoinDropRateDelayInSeconds = 2;
	
	// Use this for initialization
	void Start () {
		Position = StartPosition;
		
		if(Team == eTeam.Blue)
		{
			UnityLight.color = new Color(0.5f,0.5f,0.8f,1.0f);
			TeamNumber = 0;
		}
		else
		{
			UnityLight.color = new Color(0.8f,0.5f,0.5f,1.0f);
			TeamNumber = 1;
		}
	}
	
	// Update is called once per frame
	void Update () {
		
		var tmpHorizontal = Input.GetAxis("Horizontal_" + TeamNumber);
		var tmpVertical = Input.GetAxis("Vertical_" + TeamNumber);
		
		Vector3 tmpOldPos = Position;
		Vector3 tmpMovement = new Vector3(tmpHorizontal, 0, tmpVertical);
		Vector3 tmpDirection = tmpMovement.normalized;
		
		float tmpRunSpeed = 1.0f;
		if(HasFlag)
		{
			tmpRunSpeed = MaximumRunSpeedWithTreasure;
		}
		else
		{
			tmpRunSpeed = MaximumRunSpeed;	
		}
		
		float tmpSpeed = Mathf.Sqrt(tmpHorizontal*tmpHorizontal + tmpVertical*tmpVertical) * tmpRunSpeed * Time.deltaTime;
		
		tmpMovement = tmpDirection * tmpSpeed;
		
		Position += tmpMovement;

		
		// Check field bounds.
		if(Position.x < -25){
			Position.x = -25;
			StartCoroutine(CollisionResponse());
		}
		if(Position.x > 25){
			Position.x = 25;
			StartCoroutine(CollisionResponse());
		}
		if(Position.z < -10){
			Position.z = -10;
			StartCoroutine(CollisionResponse());
		}
		if(Position.z > 10){
			Position.z = 10;
			StartCoroutine(CollisionResponse());
		}
		
		// Check obstacle collision.
		RaycastHit tmpHit;
		int layerMask = 1 << 8 | 1 << 10;
		if(Physics.Raycast(tmpOldPos, Position-tmpOldPos, out tmpHit, (Position-tmpOldPos).magnitude, layerMask))
		{
			Position = tmpOldPos;
			StartCoroutine(CollisionResponse());
		}
		
		//Check flag collision.
		layerMask = 1 << 9;
		if(!HasFlag && Physics.Raycast(tmpOldPos, Position-tmpOldPos, out tmpHit, (Position-tmpOldPos).magnitude, layerMask))
		{
			
	        if(tmpHit.collider.gameObject.CompareTag("Flag_" + (1-TeamNumber)))
			{
				FlagCollider = tmpHit.collider;
				FlagStartPos = FlagCollider.transform.position;
				HasFlag = true;
				
				// Play capture sound
				AudioSource.PlayClipAtPoint(SoundCapture,Position);
				
							
				// Drop coins.
				StartCoroutine(DropCoins());
			}
		}
		
		// Set flag position.
		if(HasFlag && FlagCollider != null){	
			FlagCollider.transform.position = new Vector3(Position.x,FlagCollider.transform.position.y, Position.z);
		}
		
		// Set player model position.
		transform.position = Position;
		
		// Check enemy territory.
		if(Team == eTeam.Blue && Position.x > -5 || Team == eTeam.Red && Position.x < 5){
			IsInEnemyTerritory = true;
		}
		else {
			IsInEnemyTerritory = false;			
		}
		
		
		
		if(!LightOn && Input.GetAxis("Light_" + TeamNumber) > 0.5f && !HasFlag)
		{
			StartCoroutine(SwitchOnLight());
		}
		
		
		
	}
	
	public void DoDamage(float iAmount)
	{
		Health -= iAmount;
		
		if(Health <= 0)
		{
			StartCoroutine(Die());	
		}
	}
	
	IEnumerator Die()
	{
		AudioSource.PlayClipAtPoint(SoundDie, Position);
		Position.y = -1000;
		LightOn = false;
		
		if(HasFlag && FlagCollider != null)
		{
			FlagCollider.transform.position = FlagStartPos;
			HasFlag = false;
			FlagCollider = null;
		}
		
		yield return new WaitForSeconds(2);
		Position = StartPosition;
		Health = 100;
		
		enabled = true;
	}
	
	IEnumerator DropCoins()
	{
		while(HasFlag && FlagCollider != null)
		{
			yield return new WaitForSeconds(CoinDropRateDelayInSeconds);
			AudioSource.PlayClipAtPoint(SoundDropCoin, Position);
			
			Instantiate(BaseCoin, Position, Quaternion.identity);
		}
	}

	
	IEnumerator CollisionResponse()
	{
		#if UNITY_STANDALONE_WIN
		XInputDotNetPure.PlayerIndex tmpIndex = PlayerIndex.One;
		if(TeamNumber == 1)
			tmpIndex = PlayerIndex.Two;
		
		GamePad.SetVibration(tmpIndex,0.3f,0.3f);
		yield return new WaitForEndOfFrame();
		GamePad.SetVibration(tmpIndex,0.6f,0.6f);
		yield return new WaitForEndOfFrame();
		GamePad.SetVibration(tmpIndex,0.7f,0.7f);
		yield return new WaitForEndOfFrame();
		GamePad.SetVibration(tmpIndex,1f,1f);
		yield return new WaitForEndOfFrame();
		GamePad.SetVibration(tmpIndex,0.3f,0.3f);
		yield return new WaitForEndOfFrame();
		GamePad.SetVibration(tmpIndex,0,0); // Set to 0 to stop vibration!!!
		#endif		
		yield return false;
	}
	
	IEnumerator SwitchOnLight()
	{
		LightOn = true;
		yield return new WaitForSeconds(LightTimeSeconds);
		LightOn = false;
		yield return new WaitForSeconds(LightReloadTimeSeconds);
	}
}
