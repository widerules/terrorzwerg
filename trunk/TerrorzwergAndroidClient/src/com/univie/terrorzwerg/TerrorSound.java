package com.univie.terrorzwerg;

import java.util.HashMap;
import java.util.Map;

import android.app.Activity;
import android.media.MediaPlayer;

public class TerrorSound
{

	Map<String,MediaPlayer> oTerrorPlayer = null;
	
	public TerrorSound(Activity para_act){
		oTerrorPlayer = new HashMap<String,MediaPlayer>(); 
		oTerrorPlayer.put( "walk", MediaPlayer.create( para_act, R.raw.step_0 ) );
		oTerrorPlayer.put( "hurt", MediaPlayer.create( para_act, R.raw.hurt_1 ) );
		oTerrorPlayer.put( "death", MediaPlayer.create( para_act, R.raw.player_killed_1 ) );
		oTerrorPlayer.put( "strike", MediaPlayer.create( para_act, R.raw.strikematch ) );
	}
	
	public void playSound(String para_sound)
	{
		if(oTerrorPlayer.containsKey( para_sound ))
		{
			oTerrorPlayer.get( para_sound ).start();
		}
	}

 
}
