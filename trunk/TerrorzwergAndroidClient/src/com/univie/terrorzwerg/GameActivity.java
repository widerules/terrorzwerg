package com.univie.terrorzwerg;

import com.univie.terrorzwerg.GameSocket.ReceiveListener;

import android.os.Bundle;
import android.app.Activity;
import android.app.ProgressDialog;
import android.content.Intent;
import android.hardware.Sensor;
import android.hardware.SensorEvent;
import android.hardware.SensorEventListener;
import android.hardware.SensorManager;
import android.util.Log;
import android.view.DragEvent;
import android.view.Menu;
import android.view.MotionEvent;
import android.view.View;
import android.view.View.OnClickListener;
import android.view.View.OnDragListener;
import android.view.View.OnTouchListener;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.TextView;

public class GameActivity extends Activity {

	private static final String TAG = "TerrorZwerg";
	private ImageView gImageView = null;
	private String sIp;
	private int iPort;
	private String sTeam;
	private GameSocket oGameSocket = null;
	private ProgressDialog oProgressDialog = null;
	private Button oReadyButton = null;
	private boolean bGameStarted = false;
	private Activity oAct = null;
	private boolean bIsWet = false;
	private TerrorSound oTerrorSound = null;
	private SensorManager oSensorManager = null;
	private Sensor oRotationVector = null;
	
	
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_game);
		gImageView = (ImageView) findViewById(R.id.game_image);
		gImageView.setOnTouchListener( strikeListener );
		
		oAct = this;
		Bundle extras = getIntent().getExtras(); 
		String sTerrorData="";

		if (extras != null) {
			sTerrorData = extras.getString("TERROR_STRING");
			Log.d(TAG,sTerrorData);
		}
		oTerrorSound = new TerrorSound(this);
		oReadyButton = (Button) findViewById(R.id.rdyButton);
		oReadyButton.setVisibility( Button.INVISIBLE );
		oReadyButton.setOnClickListener( oReadyListener );
		
		 oSensorManager = (SensorManager)getSystemService(SENSOR_SERVICE);
         oRotationVector = oSensorManager.getDefaultSensor(Sensor.TYPE_ROTATION_VECTOR);
		
		// if no terror data were delivered
		if(sTerrorData == "")
		{
			finish();
		}
		else
		{

			// open tcp connection	
			String[] oTerrorDataSplit = sTerrorData.split(";");
			if(oTerrorDataSplit.length >=3)
			{
				sIp = oTerrorDataSplit[0];
				iPort = Integer.parseInt(oTerrorDataSplit[1]);
				sTeam = oTerrorDataSplit[2];
				oProgressDialog = ProgressDialog.show( this, "Connecting", "to: "+sIp+":"+iPort );
				setTeamBG();
				// connect
				oGameSocket = GameSocket.getInstance();
				oGameSocket.connectTo(sIp, iPort, sTeam);
				oGameSocket.addReceiveListener( oReceiveListener );
				
			}
		}
	
	}
	
	
	
	@Override
	protected void onDestroy()
	{
		oGameSocket.disconnect();
		super.onDestroy();
	}

	@Override
	protected void onPause()
	{
		// TODO Auto-generated method stub
		oSensorManager.unregisterListener( oTerrorSensor );
		super.onPause();
	}

	@Override
	protected void onResume()
	{
		oSensorManager.registerListener( oTerrorSensor, oRotationVector, SensorManager.SENSOR_DELAY_NORMAL );
		super.onResume();
	}



	private SensorEventListener oTerrorSensor = new SensorEventListener()
	{
		
		@Override
		public void onSensorChanged( SensorEvent event )
		{
			int iOldX = 0;
			int iOldY = 0;
			int iOldZ = 0;
			if( bGameStarted && event.sensor.getType() == Sensor.TYPE_ROTATION_VECTOR )
			{
				
				int iX = (int) (event.values[0]*10);
				int iY = (int) (event.values[1]*10);
				int iZ = (int) (event.values[2]*10);
				
				Log.d(TAG,"x: "+iX+", "+iY+", "+iZ);
				
				if( iX-iOldX > 1 || iX-iOldX < -1 || iY-iOldY > 1 || iY-iOldY < -1 || iZ-iOldZ > 1 || iZ-iOldZ < -1 )
				{
					oGameSocket.write( "walk;"+iX+","+iY+","+iZ );
					oTerrorSound.playSound( "walk" );
					iOldX = iX;
					iOldY = iY;
					iOldZ = iZ;
				}
				
			}
			
		}
		
		@Override
		public void onAccuracyChanged( Sensor sensor, int accuracy )
		{
			// TODO Auto-generated method stub
			
		}
	};
	
	private OnTouchListener strikeListener = new OnTouchListener()
	{
		private float fStartX = 0;
		@Override
		public boolean onTouch( View v, MotionEvent event )
		{
			//Log.d(TAG,"TOUCH");
			if(bGameStarted)
			{
				Log.d( TAG,"TOUCH: "+event.getAction()+" "+event.getX() );
				if( event.getAction() == MotionEvent.ACTION_DOWN )
				{
					fStartX = event.getX();
				}
				else if( event.getAction() == MotionEvent.ACTION_UP  )
				{

					int test = (int ) (fStartX-event.getX());
					Log.d(TAG,"TEST: "+test);
					if(test<0)
					{
						test*=-1;
					}
					if(test > 20)
					{
						oGameSocket.write( "Strike" );
						oTerrorSound.playSound( "strike" );
					}
				}
			}
			return true;
		}
		

	};
	
	private ReceiveListener oReceiveListener = new ReceiveListener()
	{

		@Override
		public void packetReceived( String head, String[] params )
		{
			Log.d(TAG,"in callback:"+head+":"+params[0]);
			if( head.equals( "connected" ) )
			{
				Log.d(TAG,"in connected");
				if( oProgressDialog != null )
				{
					oProgressDialog.dismiss();
				}
				oReadyButton.setVisibility( Button.VISIBLE );
				
			}
			else if( head.equals( "start" ) )
			{
				Log.d(TAG,"in start");
				bGameStarted = true;
				
				gImageView.setImageDrawable( oAct.getResources().getDrawable( R.drawable.striking_surface ) );
				oReadyButton.setVisibility( Button.INVISIBLE );
			}
			else if( head.equals( "fail" ))
			{
		        oProgressDialog.dismiss();
		        Intent setIntent = new Intent(oAct,QRCodeReaderActivity.class);
		        oGameSocket.disconnect();
		        finish();
		        startActivity(setIntent); 
			}
			// Ingame
			if( bGameStarted)
			{
				if( head.equals( "wet" ) )
				{
					if( params[0].equals("0") )
					{
						bIsWet = true;
						// paint wet overlay
					}
					else
					{
						bIsWet = false;
						// get rid of wet overlay
					}	
				}
				else if( head.equals( "sound" ) )
				{
					Log.d(TAG,"Sound"+params[0]);
					oTerrorSound.playSound( params[0] );
				}
				else if( head.equals( "end" ) )
				{
					if( params[0].equals( "0" ) )
					{
						gImageView.setImageDrawable( oAct.getResources().getDrawable( R.drawable.won ) );
					}
					else
					{
						//lost screen
					}
				}

			}
		}
		
	};

	OnClickListener oReadyListener = new OnClickListener()
	{

		@Override
		public void onClick( View v )
		{
			Log.d(TAG,"IAM READY!");
			// send we are ready !
			oGameSocket.write( "Ready" );
		}
		
	};
	
    public void onBackPressed() {
        Log.d(TAG, "onBackPressed Called");
        oProgressDialog.dismiss();
        Intent setIntent = new Intent(this,QRCodeReaderActivity.class);
        finish();
        startActivity(setIntent); 
    }  
	
	private void setTeamBG()
	{
		if(sTeam.equals("0"))
		{
			gImageView.setImageDrawable(getResources().getDrawable(R.drawable.prepare_red));
		}
		else if (sTeam.equals("1"))
		{
			gImageView.setImageDrawable(getResources().getDrawable(R.drawable.prepare_blue));
		}
		else
		{
			// something wrong ;)
			Log.e(TAG,"no valid team");
		}
	}

}
