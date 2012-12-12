using UnityEngine;
using System.Collections;
using com.google.zxing.qrcode;
using com.google.zxing.common;

public class Menu : MonoBehaviour {

    Texture2D TextureLogPlayer0;
    Texture2D TextureLogPlayer1;

	// Use this for initialization
	void Start () {
        Game tmpGame = GetComponent<Game>();
        TextureLogPlayer0 = CreateQR("P0;" + tmpGame.IPAddress, 256);
        TextureLogPlayer1 = CreateQR("P1;" + tmpGame.IPAddress, 256);
    }

    void OnGUI()
    {
        Game tmpGame = GetComponent<Game>();

        if (!tmpGame.IsGameRunning)
        {
            GUI.Label(new Rect(Screen.width / 2 - 100, 40, 200, 20), "Hover over QR Code to connect!");
            GUI.DrawTexture(new Rect(Screen.width/2 - 300, 100, 256, 256), TextureLogPlayer0);
            GUI.DrawTexture(new Rect(Screen.width/2 + 44, 100, 256, 256), TextureLogPlayer1);
        }
    }


    Texture2D CreateQR(string iQRString, int iSize)
    {
        Texture2D tmpTex = new Texture2D(iSize, iSize);

        QRCodeWriter tmpWriter = new QRCodeWriter();
        ByteMatrix tmpMatrix = tmpWriter.encode(iQRString, com.google.zxing.BarcodeFormat.QR_CODE, iSize, iSize);

        Color32[] tmpColor = new Color32[iSize * iSize];

        for (int i = 0; i < iSize; i++)
        {
            for (int j = 0; j < iSize; j++)
            {
                int tmpPos = j*iSize+i;
                byte tmpCol = tmpMatrix.Array[i][j] == 0 ? (byte)0 : (byte)255;
                tmpColor[tmpPos].r = tmpColor[tmpPos].g = tmpColor[tmpPos].b = tmpCol;
                tmpColor[tmpPos].a = 255;
            }
        }

        tmpTex.SetPixels32(tmpColor);
		tmpTex.Apply();
		
        return tmpTex;
    }

	// Update is called once per frame
	void Update () {
	    
	}
}
