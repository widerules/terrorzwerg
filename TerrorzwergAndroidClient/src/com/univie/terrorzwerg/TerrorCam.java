package com.univie.terrorzwerg;

import java.io.ByteArrayOutputStream;
import com.google.zxing.BinaryBitmap;
import com.google.zxing.LuminanceSource;
import com.google.zxing.common.HybridBinarizer;
import com.google.zxing.qrcode.QRCodeReader;
import android.app.Activity;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.ImageFormat;
import android.graphics.Rect;
import android.graphics.YuvImage;
import android.hardware.Camera;
import android.hardware.Camera.PreviewCallback;
import android.hardware.Camera.Size;
import android.util.Log;
import android.view.SurfaceHolder;
import android.view.SurfaceHolder.Callback;
import android.view.SurfaceView;
import android.widget.Toast;

public class TerrorCam implements Callback, PreviewCallback
{

	private static final String TAG = "TerrorZwerg";
	private SurfaceHolder oPreviewHolder = null;
	private Camera oCamera = null;
	private boolean inPreview = false;
	private boolean cameraConfigured = false;
	private Activity oAct = null;
	private QRCodeReturnListener qrcl = null;


	public void addQRCodeReturnListener(QRCodeReturnListener para_qr)
	{
		qrcl = para_qr;
	}
	
	public interface QRCodeReturnListener
	{
		/**
		 * packet received
		 * @param head - function name
		 * @param params - parameters
		 */
		public void validQRcode(String QRText);
	}
	
	public TerrorCam( SurfaceView para_prev, Activity para_act )
	{
		oPreviewHolder = para_prev.getHolder();

		oAct = para_act;
	}

	private Camera.Size getBestPreviewSize( int width, int height,
			Camera.Parameters parameters )
	{
		Log.d( TAG, "getBestPreviewSize" );
		Camera.Size result = null;

		for ( Camera.Size size : parameters.getSupportedPreviewSizes() )
		{
			if ( size.width <= width && size.height <= height )
			{
				if ( result == null )
				{
					result = size;
				} else
				{
					int resultArea = result.width * result.height;
					int newArea = size.width * size.height;

					if ( newArea > resultArea )
					{
						result = size;
					}
				}
			}
		}

		return ( result );
	}

	private void initPreview( int width, int height )
	{
		Log.d( TAG, "initPreview" );
		if ( oCamera != null && oPreviewHolder.getSurface() != null )
		{
			try
			{
				oCamera.setPreviewDisplay( oPreviewHolder );
			} catch ( Throwable t )
			{
				Log.e( "PreviewDemo-surfaceCallback",
						"Exception in setPreviewDisplay()", t );
				Toast.makeText( oAct, t.getMessage(), Toast.LENGTH_LONG )
						.show();
			}

			if ( !cameraConfigured )
			{
				Camera.Parameters parameters = oCamera.getParameters();
				Camera.Size size = getBestPreviewSize( width, height,
						parameters );

				if ( size != null )
				{
					parameters.setPreviewSize( size.width, size.height );
					oCamera.setParameters( parameters );
					cameraConfigured = true;
				}
			}
		}
	}

	public void start()
	{
		Log.d( TAG, "start" );
		try
		{
			oCamera = Camera.open();
		} catch ( Exception e )
		{
			// TODO Auto-generated catch block
			Log.d(TAG,"Camera not found");
			Toast.makeText( oAct, "No Camera Found", Toast.LENGTH_LONG ).show();
			return;
		}
		oPreviewHolder.addCallback( this );
		oPreviewHolder.setType( SurfaceHolder.SURFACE_TYPE_PUSH_BUFFERS );
		startPreview();
		oCamera.setPreviewCallback( this );
	}

	public void stop()
	{
		Log.d( TAG, "stop" );
		if ( inPreview )
		{
			Log.d( TAG, "stop prev" );
			oCamera.stopPreview();
			oCamera.setPreviewCallback( null );
		}
		oPreviewHolder.removeCallback( this );
		inPreview = false;
		if(oCamera != null)
		{
			oCamera.release();
			oCamera = null;
		}
	}
	private void startPreview()
	{
		if ( cameraConfigured && oCamera != null )
		{
			oCamera.startPreview();
			inPreview = true;
		}
	}

	public void onPreviewFrame( byte[] data, Camera camera )
	{
		try
		{
			// Convert to JPG
			Size previewSize = camera.getParameters().getPreviewSize();
			YuvImage yuvimage = new YuvImage( data, ImageFormat.NV21,
					previewSize.width, previewSize.height, null );
			ByteArrayOutputStream baos = new ByteArrayOutputStream();
			yuvimage.compressToJpeg( new Rect( 0, 0, previewSize.width,
					previewSize.height ), 80, baos );
			byte[] jdata = baos.toByteArray();

			Bitmap oBitmap = BitmapFactory.decodeByteArray( jdata, 0,
					jdata.length );// ,opts);

			LuminanceSource source = new RGBLuminanceSource( oBitmap );
			BinaryBitmap oBbitmap = new BinaryBitmap( new HybridBinarizer(
					source ) );
			//Log.d( TAG, "blubb" );
			try
			{
				QRCodeReader oReader = new QRCodeReader();
				String sDecoded = oReader.decode( oBbitmap ).getText();
				Log.d( TAG, "TEXT: " + sDecoded );
				if(qrcl != null)
				{
					qrcl.validQRcode( sDecoded );
				}
			} 
			catch ( Exception e1 )
			{
				// Log.e(TAG,e1.toString());
			}

		} 
		catch ( Exception e )
		{
			// Log.e(TAG,e.toString());
		}
	}

	@Override
	public void surfaceCreated( SurfaceHolder holder )
	{
		// no-op -- wait until surfaceChanged()
	}

	@Override
	public void surfaceChanged( SurfaceHolder holder, int format, int width,
			int height )
	{
		initPreview( width, height );
		startPreview();

	}

	@Override
	public void surfaceDestroyed( SurfaceHolder holder )
	{
		// no-op
	}

}
