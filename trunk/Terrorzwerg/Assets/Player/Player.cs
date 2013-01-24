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
    public Light UnityLightLightOnly;
    public Light CollisionLight;
    public GameObject Dwarf;
    public Material BlueSkin;
    public Material RedSkin;
    public ParticleSystem Fire;
	Game gameScript;
	
	public bool LightOn {
		get {
			return vLightOn;
		}
		set {
			vLightOn = value;
			UnityLight.enabled = value;
            UnityLightLightOnly.enabled = value;
		}
	}
	public bool IsInEnemyTerritory = false;
	public bool HasFlag = false;
	public float MaximumRunSpeed = 1;
	public float MaximumRunSpeedWithTreasure = 0.7f;
	public float LightTimeSeconds = 5;
	public float LightReloadTimeSeconds = 2;
    public bool IsInRain = false;

	public NetworkPlayer nPlayer;
	
	public float Health = 100;
	
	public eTeam Team = eTeam.Blue;
	int TeamNumber;

    public bool DebugMode;
	
	public AudioClip SoundCapture;
	
	Collider FlagCollider;
	Vector3 FlagStartPos;
	
	public Coin BaseCoin;
	float CoinDropRateDelayInSeconds = 2;
	
	public float xAxis;
	public float yAxis;
	public float LightButton;
	public bool vibrate;
    public bool IsDead;
    public float ActualMovement;

    float vWalkTime;
	// Use this for initialization
	void Start () {
        SetPositionAndTeam(StartPosition, Team);
		
		gameScript=(Game)FindObjectOfType(typeof(Game));					
	}
	
	// Update is called once per frame
	void Update () {
        if (DebugMode)
        {
            xAxis = Input.GetAxis("Horizontal_" + TeamNumber);
            yAxis = Input.GetAxis("Vertical_" + TeamNumber);
        }
		var tmpHorizontal = xAxis;
		var tmpVertical = yAxis;
		
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

        if (!IsDead)
        {
            float tmpCurrentRunSpeed = Mathf.Clamp01(Mathf.Abs(Mathf.Sqrt(tmpHorizontal * tmpHorizontal + tmpVertical * tmpVertical) * 3f) - 0.3f) * tmpRunSpeed;
            float tmpSpeed = tmpCurrentRunSpeed * Time.deltaTime;

            tmpMovement = tmpDirection * tmpSpeed;

            if (tmpMovement.magnitude > 0.01f)
            {
                //float tmpRotation = Vector2.Angle(-Vector2.up, new Vector2(-tmpMovement.x, tmpMovement.z));
                float tmpRotation = Mathf.Rad2Deg * Mathf.Atan2(tmpMovement.x, tmpMovement.z) + 180;
                Dwarf.transform.localRotation = Quaternion.Euler(0, tmpRotation, 0);
                Dwarf.animation["walk"].speed = tmpCurrentRunSpeed / 10.0f; // 0.5f;

                if (!Dwarf.animation.IsPlaying("walk"))
                {
                    Dwarf.animation.Play("walk");
                }
                rigidbody.drag = 3;
            }
            else
            {
                if (!Dwarf.animation.IsPlaying("idle"))
                {
                    Dwarf.animation.Play("idle");
                }
                rigidbody.drag = 30;
            }

            //// Check obstacle collision.
            RaycastHit tmpHitCenter;
            int layerMask = 1 << 8 | 1 << 10;

            rigidbody.AddForce(tmpMovement, ForceMode.Impulse);
            Position = rigidbody.position;

            ActualMovement = (Position - tmpOldPos).magnitude;
            if (ActualMovement > 0.01)
            {
                vWalkTime += Time.deltaTime * (tmpCurrentRunSpeed / 4.0f);
                if (vWalkTime >= 0.25f)
                {
                    gameScript.Player_PlaySound(nPlayer, "Walk");
                    vWalkTime = 0;
                    // Debug.Log("Step");
                }
            }

            //Check flag collision.
            layerMask = 1 << 9;
            if (!HasFlag && Physics.Raycast(tmpOldPos, Position - tmpOldPos, out tmpHitCenter, (Position - tmpOldPos).magnitude, layerMask))
            {

                if (tmpHitCenter.collider.gameObject.CompareTag("Flag_" + (1 - TeamNumber)))
                {
                    FlagCollider = tmpHitCenter.collider;
                    FlagStartPos = FlagCollider.transform.position;
                    HasFlag = true;

                    // Play capture sound
                    AudioSource.PlayClipAtPoint(SoundCapture, Camera.main.transform.position);


                    // Drop coins.
                    StartCoroutine(DropCoins());
                }
            }

            // Set flag position.
            if (HasFlag && FlagCollider != null)
            {
                FlagCollider.transform.localRotation = Quaternion.Euler(0, Dwarf.transform.localRotation.eulerAngles.y - 90, 0);
                FlagCollider.transform.position = new Vector3(Position.x, Position.y + 0.5f, Position.z);
            }

            // Set player model position.
            transform.position = Position;
        }

		// Check enemy territory.
		if(Team == eTeam.Blue && Position.x > -5 || Team == eTeam.Red && Position.x < 5){
			IsInEnemyTerritory = true;
		}
		else {
			IsInEnemyTerritory = false;			
		}

        CheckRainZones();

        if (DebugMode)
        {
            LightButton = Input.GetAxis("Light_" + TeamNumber);
        }
        if (!LightOn && LightButton > 0.5f && !IsInRain) //  && !HasFlag
		{
			StartCoroutine(SwitchOnLight());
		}
		
		
		
	}

    private void CheckRainZones()
    {
        var tmpRainzones = FindObjectsOfType(typeof(Rainzone));
        IsInRain = false;
        foreach (Rainzone tmpZone in tmpRainzones)
        {
            if ((new Vector2(tmpZone.transform.position.x, tmpZone.transform.position.z) - new Vector2(transform.position.x, transform.position.z)).magnitude < tmpZone.Size)
            {
                if (IsInRain == false)
                {
                    gameScript.Player_PlaySound(nPlayer, "Extinguish");
                    LightOn = false;
                    IsInRain = true;
                }
                break;
            }
        }
    }
	
	public void DoDamage(float iAmount)
	{
		Health -= iAmount;
		gameScript.SendHealth((int)Health,nPlayer);
        if (Health <= 0 && !IsDead)
        {
			StartCoroutine(Die());	
		}
	}

    public void SetPositionAndTeam(Vector3 iPosition, eTeam iTeam)
    {
        StartPosition = iPosition;
        Position = iPosition;
        rigidbody.position = iPosition;
        Team = iTeam;
        if (Team == eTeam.Blue)
        {
            Dwarf.transform.FindChild("head_geo").renderer.material = BlueSkin;
            Dwarf.transform.FindChild("body").renderer.material = BlueSkin;

            UnityLightLightOnly.color = new Color(0.5f, 0.5f, 0.8f, 1.0f);
            UnityLight.color = new Color(0.5f, 0.5f, 0.8f, 1.0f);
            TeamNumber = 0;
        }
        else
        {
            Dwarf.transform.FindChild("head_geo").renderer.material = RedSkin;
            Dwarf.transform.FindChild("body").renderer.material = RedSkin;

            UnityLightLightOnly.color = new Color(0.8f, 0.5f, 0.5f, 1.0f);
            UnityLight.color = new Color(0.8f, 0.5f, 0.5f, 1.0f);
            TeamNumber = 1;
        }
        UnityLightLightOnly.shadows = LightShadows.None;
        UnityLight.shadows = LightShadows.Soft;
        LightOn = false;
    }
	
	IEnumerator Die()
	{
        IsDead = true;
        gameScript.Player_PlaySound(nPlayer, "Death");
		LightOn = false;
		
		if(HasFlag && FlagCollider != null)
		{
            FlagCollider.transform.localRotation = Quaternion.identity;
			FlagCollider.transform.position = FlagStartPos;
			HasFlag = false;
			FlagCollider = null;
		}
        int tmpRand = Random.Range(0, 2);
        Dwarf.animation.Play("die_" + tmpRand);

		yield return new WaitForSeconds(2);
		Position = StartPosition;
        rigidbody.position = StartPosition;
		Health = 100;
		gameScript.SendHealth((int)Health,nPlayer);
		enabled = true;
        IsDead = false;
	}
	
	IEnumerator DropCoins()
	{
		while(HasFlag && FlagCollider != null)
		{
            yield return new WaitForSeconds(CoinDropRateDelayInSeconds);
            gameScript.Player_PlaySound(nPlayer, "Coin");
			
			Instantiate(BaseCoin, Position, Quaternion.identity);
		}
	}

    void OnCollisionEnter(Collision collision)
    {
        StartCoroutine(CollisionResponse());
    }

    void OnCollisionStay(Collision collisionInfo)
    {
        foreach (ContactPoint contact in collisionInfo.contacts)
        {
            //Debug.DrawRay(contact.point, contact.normal * 10, Color.white);
        }
    }

	IEnumerator CollisionResponse()
	{
		
		//vibrate=true;
		
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

        //CollisionLight.intensity = 1;
        //yield return new WaitForEndOfFrame();
        //CollisionLight.intensity = 0.9f;
        //yield return new WaitForEndOfFrame(); 
        //CollisionLight.intensity = 1;
        //yield return new WaitForEndOfFrame();
        //CollisionLight.intensity = 0.8f;
        //yield return new WaitForSeconds(0.2f);
        //CollisionLight.intensity = 0.7f;
        //yield return new WaitForSeconds(0.2f);
        //CollisionLight.intensity = 0.4f;
        //yield return new WaitForSeconds(0.2f);
        //CollisionLight.intensity = 0.2f;
        //yield return new WaitForSeconds(0.2f);
        //CollisionLight.intensity = 0;


		yield return false;
	}
	
	IEnumerator SwitchOnLight()
	{
		LightOn = true;
        Fire.enableEmission = true;
        gameScript.Player_PlaySound(nPlayer, "Striking");
        yield return new WaitForSeconds(LightTimeSeconds);
        Fire.enableEmission = false;
		LightOn = false;
		//yield return new WaitForSeconds(LightReloadTimeSeconds);
	}
}
