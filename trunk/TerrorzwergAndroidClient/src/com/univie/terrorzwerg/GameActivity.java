package com.univie.terrorzwerg;

import com.univie.terrorzwerg.GameSocket.ReceiveListener;

import android.os.Bundle;
import android.app.Activity;
import android.app.ProgressDialog;
import android.util.Log;
import android.view.Menu;
import android.view.View;
import android.view.View.OnClickListener;
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
	
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_game);
		gImageView = (ImageView) findViewById(R.id.game_image);
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
	
	ReceiveListener oReceiveListener = new ReceiveListener()
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
			// send we are ready !
			oGameSocket.write( "Ready" );
		}
		
	};
	
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
