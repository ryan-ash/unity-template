using Newtonsoft.Json;
using System;
using UnityEngine;

public static class PlayerPrefsWrapper {

    private const string MUSIC_ENABLED = "musicEnabled";
    private const string SOUND_FX_ENABLED = "soundFXEnabled";
    private const string PLAYED_GAMES_COUNTER = "playedGames";
    private const string PLAYER = "playerData";
    private const string PLAYER_IS_NEW_TO_THE_GAME = "playerIsNewToTheGame";

    public delegate void ToggleMusic(bool value);
    public static event ToggleMusic toggleMusicEvent;
    public delegate void ToggleSoundFX(bool value);
    public static event ToggleMusic toggleSoundFXEvent;

    public static bool MusicEnabled 
    {
        get { return PlayerPrefs.GetInt(MUSIC_ENABLED, 1) == 1; }
        set { 
            PlayerPrefs.SetInt(MUSIC_ENABLED, value ? 1 : 0);
            if (toggleMusicEvent != null) toggleMusicEvent(value);
        }
    }

    public static bool SoundFXEnabled 
    {
        get { return PlayerPrefs.GetInt(SOUND_FX_ENABLED, 1) == 1; }
        set { 
            PlayerPrefs.SetInt(SOUND_FX_ENABLED, value ? 1 : 0);
            if (toggleSoundFXEvent != null) toggleSoundFXEvent(value);
        }
    }
    
    public static bool PlayerIsNewToTheGame 
    {
        get { return PlayerPrefs.GetInt(PLAYER_IS_NEW_TO_THE_GAME, 1) == 1; }
        set { PlayerPrefs.SetInt(PLAYER_IS_NEW_TO_THE_GAME, value ? 1 : 0); }
    }

    public static int PlayedMatchesCounter 
    {
        get { return PlayerPrefs.GetInt(PLAYED_GAMES_COUNTER); }
        set { PlayerPrefs.SetInt(PLAYED_GAMES_COUNTER, value); }
    }

    public static void UpdatePlayedGamesCounter() 
    {
        PlayerPrefs.SetInt(PLAYED_GAMES_COUNTER, PlayerPrefs.GetInt(PLAYED_GAMES_COUNTER) + 1);

        if (PlayerIsNewToTheGame)
        {
            PlayerIsNewToTheGame = false;
        }
    }

    /* private static Player _player;
    public static Player CurrentPlayer 
    {
        get { 
            if (_player == null) {
                var jsonString = PlayerPrefs.GetString(PLAYER);
                if (jsonString.Length == 0) {
                    _player = new Player();
                    Debug.Log("No player data found. Creating a new player object...");
                } else {
                    _player = JsonConvert.DeserializeObject<Player>(jsonString);
                    Debug.Log("Player sucessfully recovered. Welcome back.");
                }
            }
            return _player;
        }
        set {
            var jsonString = JsonConvert.SerializeObject(value);
            Debug.Log("Player has been saved. Breath out, your progress won't be lost from this point on.");
            PlayerPrefs.SetString(PLAYER, jsonString);
        }
    } */
		
    public static void Clear() 
    {
        PlayerPrefs.DeleteAll();
        // _player = null;
        Debug.Log("Progress wipe is complete. You can start over.");
    }
}
