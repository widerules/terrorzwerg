using UnityEngine;
using System.Collections;

#if UNITY_STANDALONE_WIN
using XInputDotNetPure;
#endif

public class Player : MonoBehaviour {
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
	public float LightTimeSeconds = 5;
	
	public float Health = 100;
	
	public int Team = 0;
	
	public AudioClip SoundDie;
	
	Collider FlagCollider;
	Vector3 FlagStartPos;
	
	// Use this for initialization
	void Start () {
		Position = StartPosition;
	}
	
	// Update is called once per frame
	void Update () {
		
		var tmpHorizontal = Input.GetAxis("Horizontal_" + Team);
		var tmpVertical = Input.GetAxis("Vertical_" + Team);
		
		Vector3 tmpOldPos = Position;
		Vector3 tmpMovement = new Vector3(tmpHorizontal, 0, tmpVertical);
		Vector3 tmpDirection = tmpMovement.normalized;
		float tmpSpeed = Mathf.Sqrt(tmpHorizontal*tmpHorizontal + tmpVertical*tmpVertical) * MaximumRunSpeed * Time.deltaTime;
		
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
			
	        if(tmpHit.collider.gameObject.CompareTag("Flag_" + (1-Team)))
			{
				FlagCollider = tmpHit.collider;
				FlagStartPos = FlagCollider.transform.position;
				HasFlag = true;
			}
		}
		
		// Set flag position.
		if(HasFlag && FlagCollider != null){	
			FlagCollider.transform.position = new Vector3(Position.x,FlagCollider.transform.position.y, Position.z);
			
		}
		
		// Set player model position.
		transform.position = Position;
		
		// Check enemy territory.
		if(Team == 0 && Position.x > -5 || Team == 1 && Position.x < 5){
			IsInEnemyTerritory = true;
		}
		else {
			IsInEnemyTerritory = false;			
		}
		
		
		
		if(!LightOn && Input.GetAxis("Light_" + Team) > 0.5f)
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

	IEnumerator CollisionResponse()
	{
		#if UNITY_STANDALONE_WIN
		XInputDotNetPure.PlayerIndex tmpIndex = PlayerIndex.Two;
		if(Team == 1)
			tmpIndex = PlayerIndex.One;
		
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
	}
	
	IEnumerator SwitchOnLight()
	{
		LightOn = true;
		yield return new WaitForSeconds(LightTimeSeconds);
		LightOn = false;
		yield return new WaitForSeconds(2);
	}
}
