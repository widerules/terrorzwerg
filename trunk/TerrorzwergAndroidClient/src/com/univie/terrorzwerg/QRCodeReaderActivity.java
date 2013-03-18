package com.univie.terrorzwerg;

import java.io.ByteArrayOutputStream;

import com.google.zxing.BinaryBitmap;
import com.google.zxing.LuminanceSource;
import com.google.zxing.Result;
import com.google.zxing.common.HybridBinarizer;
import com.google.zxing.qrcode.QRCodeReader;
import com.univie.terrorzwerg.RGBLuminanceSource;
import com.univie.terrorzwerg.TerrorCam.QRCodeReturnListener;

import android.app.Activity;
import android.content.Intent;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.ImageFormat;
import android.graphics.Point;
import android.graphics.Rect;
import android.graphics.YuvImage;
import android.hardware.Camera;
import android.hardware.Camera.PreviewCallback;
import android.hardware.Camera.Size;
import android.os.Bundle;
import android.util.Log;
import android.view.Display;
import android.view.SurfaceHolder;
import android.view.SurfaceView;
import android.view.ViewGroup.LayoutParams;
import android.widget.ImageView;
import android.widget.RelativeLayout;
import android.widget.Toast;

/**
 * An example full-screen activity that shows and hides the system UI (i.e.
 * status bar and navigation/system bar) with user interaction.
 * 
 * @see SystemUiHider
 */
public class QRCodeReaderActivity extends Activity{

	  private static final String TAG = "TerrorZwerg";
	  private SurfaceView preview = null;
	  private Activity oAct = null;
	  private TerrorCam oTerrorCam = null;
	  
	  @Override
	  public void onCreate(Bundle savedInstanceState) {
	    super.onCreate(savedInstanceState);
	    
	    setContentView(R.layout.activity_qrcode_reader);
	    
	    preview = (SurfaceView)findViewById(R.id.cameraPrev);
	    
	    Display display = getWindowManager().getDefaultDisplay(); 
	    int width = display.getWidth();  // deprecated
	    int height = display.getHeight();  // deprecated
	    
	    LayoutParams params =  preview.getLayoutParams();
	    //new android.widget.RelativeLayout.LayoutParams(width*312/600, height*464/960);
	    Log.d(TAG,"h: "+width*330/960+" w: "+height*464/600);
//	    params.height = width*312/960+100;
	    params.height = width*400/960;
	    params.width = height*470/600;
	    preview.setLayoutParams(params);

	    
	    oTerrorCam = new TerrorCam(preview,this);
//	    previewHolder = preview.getHolder();
//	    previewHolder.addCallback(surfaceCallback);
//	    previewHolder.setType(SurfaceHolder.SURFACE_TYPE_PUSH_BUFFERS);
	    oAct = this;
	    oTerrorCam.addQRCodeReturnListener( oQRCodeListener );
	  }

	  QRCodeReturnListener oQRCodeListener = new QRCodeReturnListener()
	  {

		@Override
		public void validQRcode( String QRText )
		{
			Intent gameIntent = new	Intent(oAct,com.univie.terrorzwerg.GameActivity.class);
			gameIntent.putExtra("TERROR_STRING", QRText);
			oAct.startActivity(gameIntent);
		}

	  };
	  
	  @Override
	  public void onResume() {
	    super.onResume();
	    oTerrorCam.start();
	    
	  }
	    
	  @Override
	  public void onPause() {
		  super.onPause();
		  oTerrorCam.stop();
	  }
	  
	 

}