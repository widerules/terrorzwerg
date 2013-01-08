using UnityEngine;
using System.Collections;
using com.google.zxing.qrcode;

public class Game_Menu : MonoBehaviour, ITrackerEventHandler
{

    string tempText;
    bool isFrameFormatSet;
    Image cameraFeed;
    string qrText;
    public Texture TexMainMenu;
    public Texture TexMainMenuRed;
    public Texture TexMainMenuBlue;
    public Texture2D UnityCamTex;

    // Use this for initialization
    void Start()
    {
        QCARBehaviour qcarBehaviour = GetComponent<QCARBehaviour>();
        QCARRenderer.Instance.SetVideoBackgroundConfig(new QCARRenderer.VideoBGCfgData(){ enabled = 0, position = new QCARRenderer.Vec2I(20,20), size = new QCARRenderer.Vec2I(100,100), synchronous = 0}); 
        //QCARRenderer.Instance.
        if (qcarBehaviour)
        {
            qcarBehaviour.RegisterTrackerEventHandler(this);
        }

        isFrameFormatSet = CameraDevice.Instance.SetFrameFormat(Image.PIXEL_FORMAT.GRAYSCALE, true);
        UnityCamTex = new Texture2D(CameraDevice.Instance.GetVideoMode(CameraDevice.CameraDeviceMode.MODE_DEFAULT).width / 4, CameraDevice.Instance.GetVideoMode(CameraDevice.CameraDeviceMode.MODE_DEFAULT).height / 4);

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
        Rect tmpFull = new Rect(0,0, Screen.width,Screen.height);
        float tmpAspect = (float)UnityCamTex.width / (float)UnityCamTex.height;
        float tmpCamWidth = Screen.width * 0.45f;
        float tmpCamHeight = tmpCamWidth / tmpAspect;
        Rect tmpCam = new Rect(Screen.width/2-tmpCamWidth*0.5f, Screen.height/2-tmpCamHeight*0.25f, tmpCamWidth, tmpCamHeight);
        //GUI.Label(new Rect(10, 10, 400, 20), "Hover over QR code: " + GameData.instance.ipAdress + ":" + GameData.instance.port + " " + GameData.instance.playerId);
        if (GameData.instance.winningTeam != -1)
        {
            //if (GameData.instance.winningTeam == GameData.instance.playerId)
            //{
            //    GUI.DrawTexture(tmpFull, TexWon);
            //}
            //else
            //{
            //    GUI.DrawTexture(tmpFull, TexLost);
            //}
            //GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 50,400,20), teamWon);
        }
        else
        {
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
            GUI.DrawTexture(tmpCam, UnityCamTex);
        }

    }

    void UpdateCamTex(Image iImage)
    {
        Color32[] tmpColor = new Color32[UnityCamTex.width * UnityCamTex.height];

        for (int i = 0; i < UnityCamTex.width; i++)
        {
            for (int j = 0; j < UnityCamTex.height; j++)
            {
                int tmpUnityPos = j * UnityCamTex.width + i;
                int tmpPos = (j * 4) * iImage.Width + (iImage.Width - i * 4);
                byte tmpCol = iImage.Pixels[tmpPos];
                tmpColor[tmpUnityPos].r = tmpColor[tmpUnityPos].g = tmpColor[tmpUnityPos].b = tmpCol;
                tmpColor[tmpUnityPos].a = 255;
            }
        }
        UnityCamTex.SetPixels32(tmpColor);
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
                string AddressPart = qrText.Split(';')[0];
                GameData.instance.ipAdress = AddressPart.Split(':')[0];
                GameData.instance.port = int.Parse(AddressPart.Split(':')[1]);
                GameData.instance.playerId = int.Parse(qrText.Split(';')[1]);
            }
        }
    }
}
