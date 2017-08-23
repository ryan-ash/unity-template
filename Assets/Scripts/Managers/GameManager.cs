using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { MainMenu, CharacterSelect, GameInProgress, Scoreboard }

public class GameStateNode {
	public GameState name;
	public ViewController view;
	public GameObject[] objects;
}

public class GameManager : MonoBehaviour {

	public static GameState currentState = GameState.MainMenu;
	public static GameManager instance;

    public static Accessor Accessor;

	public Transform screens, overlays;
	public GameStateNode[] gameStates;

	private static Dictionary<GameState, GameStateNode> gameStatesDict;

	public static void ChangeGameStateTo(GameState state) {
		GameStateNode lastNode = gameStatesDict[currentState];
		if (lastNode.objects != null && lastNode.objects.Length > 0)
			MassTurnOnOff(lastNode.objects, false);

		GameStateNode newNode = gameStatesDict[state];
		if (newNode.objects != null && newNode.objects.Length > 0)
			MassTurnOnOff(newNode.objects, true);

	}

	private static void MassTurnOnOff(GameObject[] objects, bool sign) {
		foreach (GameObject obj in objects) {
			obj.SetActive(sign);
		}
	}

	private void PlaySound(string triggerLine) {
		AudioManager.instance.PlaySound(triggerLine);
	}

	void Start () {
		instance = this;

		Accessor = new Accessor();

        Accessor.Data = new DataAccessor();
        Accessor.Settings = GetComponent<SettingsManager>();

        Accessor.ScreenViewManager = new ViewManager<ScreenBaseController>(Accessor, screens);
        Accessor.OverlayViewManager = new ViewManager<OverlayBaseController>(Accessor, overlays);
	}
	
	void Update () {

	}
}
