using UnityEngine;
using System.Collections;
using com.google.zxing.qrcode;

public class Game_Menu : MonoBehaviour, ITrackerEventHandler
{

    string tempText;
    bool isFrameFormatSet;
    Image cameraFeed;
    string qrText;
	string IpAdress="";
	string Player="";
    string bla = "";

    // Use this for initialization
    void Start()
    {
        QCARBehaviour qcarBehaviour = GetComponent<QCARBehaviour>();

        if (qcarBehaviour)
        {
            qcarBehaviour.RegisterTrackerEventHandler(this);
        }

        isFrameFormatSet = CameraDevice.Instance.SetFrameFormat(Image.PIXEL_FORMAT.GRAYSCALE, true);

        InvokeRepeating("Autofocus", 1f, 2f);
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var tmpTouch in Input.touches)
        {
            if (tmpTouch.phase == TouchPhase.Ended)
            {
                if (!string.IsNullOrEmpty(qrText))
                {
                    Application.LoadLevel("Client_noMinimap");
                    qrText = null;
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
        GUI.Label(new Rect(10, 10, 400, 20), "Hover over QR code: " + GameData.instance.ipAdress+" "+ GameData.instance.playerId);
		if(GameData.instance.winningTeam!=-1){
			string teamWon="Your Team lost";
			if(GameData.instance.winningTeam==GameData.instance.playerId){
					teamWon="Your Team Won";
			}
			GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 50,400,20), teamWon);
		}

    }

    public void OnTrackablesUpdated()
    {
        bla = "blub";
        try
        {
            if (!isFrameFormatSet)
            {
                isFrameFormatSet = CameraDevice.Instance.SetFrameFormat(Image.PIXEL_FORMAT.GRAYSCALE, true);
            }

            cameraFeed = CameraDevice.Instance.GetCameraImage(Image.PIXEL_FORMAT.GRAYSCALE);
            tempText = new QRCodeReader().decode(cameraFeed.Pixels, cameraFeed.BufferWidth, cameraFeed.BufferHeight).Text;
        }
        catch
        {
            // Fail to detect QR Code!
        }
        finally
        {
            if (!string.IsNullOrEmpty(tempText))
            {
                qrText = tempText;
				GameData.instance.ipAdress=qrText.Split(';')[0];
				GameData.instance.playerId=int.Parse(qrText.Split(';')[1]);
            }
        }
    }
}
