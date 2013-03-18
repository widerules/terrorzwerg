package com.univie.terrorzwerg;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.io.PrintWriter;
import java.net.Socket;
import java.net.UnknownHostException;

import android.os.AsyncTask;
import android.util.Log;

public class GameSocket{

	private static GameSocket oInstance = null;
	private static final String TAG = "TerrorZwerg";
	private Socket gameSock = null;
	private PrintWriter output = null;
	private BufferedReader input = null;
	boolean conn = false;
	private ReceiveListener rcl = null;
	private GameSocket(){};
	private AsyncTask<Void, String, Void> oReceiver = null;
	
	
	public interface ReceiveListener{
		/**
		 * packet received
		 * @param head - function name
		 * @param params - parameters
		 */
		public void packetReceived(String head,String[] params);
	}
	
	public void addReceiveListener(ReceiveListener p_rcl)
	{
		rcl = p_rcl;
	}
	
	public static GameSocket getInstance() {
        if (oInstance == null) {
            oInstance = new GameSocket();
        }
        return oInstance;
    }
	
	/**
	 * Connects to gamehost
	 * @param ip 
	 * @param port
	 * @param params Like Team number ... or something else - seperated with ;
	 */
	public void  connectTo(String ip,int port,String params){
		gameSock = null;
		conn = false;
		Receive slhReceiver = new Receive();
		slhReceiver.ip = ip;
		slhReceiver.port = port;
		slhReceiver.para = params;
		oReceiver = slhReceiver.execute();
	}
	
	public void write(String txt){
		output.write(txt+"\n");
		output.flush();
//		return true;
	}
	
	 // receive packets - Task
    
    public class Receive extends AsyncTask<Void, String, Void> {

    	public String ip="";
    	public int port = 0;
    	public String para; 
    	
        @Override
        protected void onPreExecute() {
            Log.d(TAG, "onPreExecute");
        }

        @Override
        protected Void doInBackground(Void... params) { //This runs on a different thread
        	while(gameSock == null){
    			try {
    				
    				Log.d(TAG, "trying to open socket "+ip+":"+String.valueOf(port));
    				gameSock = new Socket(ip,port);
    				Log.d(TAG,"opend socket");
    				
    			} catch (UnknownHostException e) {
    				// TODO Auto-generated catch block
    				Log.e(TAG, e.toString());
    			} catch (IOException e) {
    				// TODO Auto-generated catch block
    				Log.e(TAG,e.toString());
    			}
    		}
    		try {
    			OutputStream out = gameSock.getOutputStream();
    			output = new PrintWriter(out);
    			Log.d(TAG,"Sending hello");
    			write("Hello;"+para);
    			input = new BufferedReader(new InputStreamReader(gameSock.getInputStream()));
    			Log.d(TAG,"receiving conn");
    			String tst = recv();
    			if(tst.contains("Hey"))
    			{ 
    				Log.d(TAG,"conn established");
    				conn=true;
    			}
    		} 
    		catch (IOException e) {
    			// TODO Auto-generated catch block
    			Log.e( TAG,e.toString() );
    		}
    		// start receiver
    		if(conn == true)
    		{
    			if(oReceiver != null)
    			{
    				Log.d( TAG,"cancel old receiver" );
    				oReceiver.cancel(true);
    			}   			
    		}
        	while(true){
        		//Log.d(TAG,"receive packets ... waiting");
        		String ret = recv();
        		if(ret != "-1")
        		{
        			publishProgress(ret);
        		}
        	}
        }
        
        protected void onProgressUpdate(String... text) {
        	
        	// header and body seperated by ; - params by ,
        	Log.d(TAG, "Text: "+text[0]);
        	String[] retSpl = text[0].split(";");
        	String head = retSpl[0];
        	String[] params = retSpl[1].split( "," );

        	if(rcl != null)
        	{
        		Log.d(TAG,"got packet: Head "+head+"  params "+params[0]);
        		rcl.packetReceived(head, params);
        	}
        }
    }
	
	private String recv(){
		char[] buff = new char[400];
		String ret="";
		
		try {
			// this blocks
			input.read(buff);
			
		} catch (IOException e) {
			
			// TODO Auto-generated catch block
			Log.e(TAG, e.toString());

			return "-1";	
		}
		
		ret = String.valueOf(buff);
		ret = ret.replaceAll("\0", "");
		if(ret == ""){
			return "-1";
		}
		Log.d(TAG, "return In rev: "+ret);

		return ret;
		
	}
	
}
