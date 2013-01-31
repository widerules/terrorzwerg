using UnityEngine;
using System.Collections;
using com.google.zxing.qrcode;
using System;
using com.google.zxing;
using com.google.zxing.common;

public class Game_Menu : MonoBehaviour, ITrackerEventHandler
{
    string vDebugText;
    string tempText;
    bool isFrameFormatSet;
    Image cameraFeed;
    string qrText;
    public Texture TexMainMenu;
    public Texture TexMainMenuRed;
    public Texture TexMainMenuBlue;
    public Texture2D UnityCamTex;
    public float UnityTexScale = 0.25f;
    Color32[] vCamColors;
    byte[] vDecodeBytes;

    WebCamTexture vWebcam;

    // Use this for initialization
    void Start()
    {
        QCARManager.Instance.DrawVideoBackground = false;
        QCARBehaviour qcarBehaviour = GetComponent<QCARBehaviour>();
        if (qcarBehaviour)
        {
            qcarBehaviour.RegisterTrackerEventHandler(this);
        }

        isFrameFormatSet = CameraDevice.Instance.SetFrameFormat(Image.PIXEL_FORMAT.GRAYSCALE, true);
        UnityCamTex = new Texture2D((int)(CameraDevice.Instance.GetVideoMode(CameraDevice.CameraDeviceMode.MODE_DEFAULT).width * UnityTexScale), (int)(CameraDevice.Instance.GetVideoMode(CameraDevice.CameraDeviceMode.MODE_DEFAULT).height * UnityTexScale));
        vCamColors = new Color32[UnityCamTex.width * UnityCamTex.height];
        vDecodeBytes = new byte[UnityCamTex.width * UnityCamTex.height];

        InvokeRepeating("Autofocus", 1f, 2f);
		qrText = "";
        GameData.instance.playerId = -1;


       // InitWebcam();

    }

    void InitWebcam()
    {
        bool foundCamera = false;
        string desiredCameraName = "";
        if (WebCamTexture.devices != null)
        {
            for (int i = 0; i < WebCamTexture.devices.Length; i++)
            {
                // We'd like a camera which is not front-facing.
                if (!WebCamTexture.devices[i].isFrontFacing)
                {
                    desiredCameraName = WebCamTexture.devices[i].name;
                    foundCamera = true;
                    break;
                }
            }
        }

        /* If we've got one, we try to get access and then play it. */
        if (foundCamera)
        {
            vWebcam = new WebCamTexture(desiredCameraName);
            //UnityCamTex = vWebcam;

            vWebcam.Play();
            vDebugText = "Found a camera and using it!";
        }
        else
        {
            vDebugText = "No camera found!";
        }
    }
	
    void Autofocus()
    {
        CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_TRIGGERAUTO);
    }
	
    // Update is called once per frame
    void Update()
    {
		/*
        foreach (var tmpTouch in Input.touches)
        {
            if (tmpTouch.phase == TouchPhase.Ended)
            {
                if (!string.IsNullOrEmpty(qrText))
                {
                  //Application.LoadLevel("Client_noMinimap");
                  //qrText = null;
				 // try to connect	
                }
            }
        }
		*/
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    void OnGUI()
    {
        Rect tmpFull = new Rect(0,0, Screen.width,Screen.height);
        float tmpAspect = (float)UnityCamTex.width / (float)UnityCamTex.height;
        float tmpCamWidth = Screen.width * 0.49f;
        float tmpCamHeight = tmpCamWidth / tmpAspect;
        Rect tmpCam = new Rect(Screen.width / 2 - tmpCamWidth * 0.5f + 10, Screen.height / 2 - tmpCamHeight * 0.4f, tmpCamWidth, tmpCamHeight);
        //GUI.Label(new Rect(10, 10, 400, 20), "Hover over QR code: " + GameData.instance.ipAdress + ":" + GameData.instance.port + " " + GameData.instance.playerId);
        GUI.DrawTexture(tmpCam, UnityCamTex);

        if (GameData.instance.playerId == 0)
        {
            GUI.DrawTexture(tmpFull, TexMainMenuBlue);
        }
        else if (GameData.instance.playerId == 1)
        {
            GUI.DrawTexture(tmpFull, TexMainMenuRed);
        }
        else
        {
            GUI.DrawTexture(tmpFull, TexMainMenu);
        }

        GUI.Label(new Rect(20, 20, 1000, 200), vDebugText);

    }

    void UpdateCamTex(Image iImage)
    {
        int tmpScale = (int)(1.0f / UnityTexScale);
        for (int i = 0; i < UnityCamTex.width; i++)
        {
            for (int j = 0; j < UnityCamTex.height; j++)
            {
                int tmpUnityPos = j * UnityCamTex.width + i;
                int tmpPos = (j * tmpScale) * iImage.Width + (iImage.Width - i * tmpScale);
                byte tmpCol = iImage.Pixels[tmpPos];
                vDecodeBytes[tmpUnityPos] = tmpCol;
                vCamColors[tmpUnityPos].r = vCamColors[tmpUnityPos].g = vCamColors[tmpUnityPos].b = tmpCol;
                vCamColors[tmpUnityPos].a = 255;
            }
        }
        UnityCamTex.SetPixels32(vCamColors);
        UnityCamTex.Apply();
    }

    public void OnTrackablesUpdated()
    {

        try
        {
            if (!isFrameFormatSet)
            {
                isFrameFormatSet = CameraDevice.Instance.SetFrameFormat(Image.PIXEL_FORMAT.GRAYSCALE, true);
            }

            cameraFeed = CameraDevice.Instance.GetCameraImage(Image.PIXEL_FORMAT.GRAYSCALE);
            UpdateCamTex(cameraFeed);

            tempText = "";
            tempText = new QRCodeReader().decode(cameraFeed.Pixels, cameraFeed.BufferWidth, cameraFeed.BufferHeight).Text;

			//tempText = new QRCodeReader().decode(vDecodeBytes, UnityCamTex.width, UnityCamTex.height).Text;

        }
        catch(Exception e)
        {
            // Fail to detect QR Code!
            // vDebugText = "Failed: " + e.InnerException.Message ;
        }
        finally
        {
            if (!string.IsNullOrEmpty(tempText))
            {
				//http://www.unet.univie.ac.at/~a0701760/terrorzwerg/TerrorzwergClient.apk?Zwegdata=127.0.0.1:666,ASD-A080a-d080a8d-0ad;0
				if( GameData.instance.ipAdress == ""){
					qrText = tempText.Split(new string[]{"Zwegdata="}, StringSplitOptions.None)[1];
					string AddressPart = qrText.Split(';')[0];
					GameData.instance.ipAdress = AddressPart.Split(':')[0];

					AddressPart = AddressPart.Split(':')[1];
					GameData.instance.port = int.Parse(AddressPart.Split(',')[0]);
					GameData.instance.networkGUID = AddressPart.Split(',')[1];
					GameData.instance.playerId = int.Parse(qrText.Split(';')[1]);
					// connect
					
					Application.LoadLevel("Client_noMinimap");
					qrText = null;
				}

                qrText = null;
            }
			else{
				GameData.instance.ipAdress = "";
			}
        }
    }
}
