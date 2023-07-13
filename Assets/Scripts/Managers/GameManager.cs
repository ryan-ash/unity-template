using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { Default, MainMenu, InProgress, GameOver }

[System.Serializable]
public class GameStateNode {
	public GameState name;
	public ViewController view;
	public GameObject[] objectsOn;
	public GameObject[] objectsOff;
}

public class GameManager : MonoBehaviour {

	public static GameState currentState = GameState.Default;
	public static GameManager instance;

    public static Accessor Accessor;

	public Transform screens, overlays;
	public GameStateNode[] gameStates;
	public GameState startState = GameState.MainMenu;

	public GameObject secondPlayer;

	private static Dictionary<GameState, GameStateNode> gameStatesDict;

	public static bool doRestart = false;
	public static bool gameStarted = false;

	void Start () {
		instance = this;

		Accessor = new Accessor();

        Accessor.Data = new DataAccessor();
        Accessor.Settings = GetComponent<SettingsManager>();

        Accessor.ScreenViewManager = new ViewManager<ScreenBaseController>(Accessor, screens);
        Accessor.OverlayViewManager = new ViewManager<OverlayBaseController>(Accessor, overlays);

		gameStatesDict = new Dictionary<GameState, GameStateNode>();
        foreach (GameStateNode node in gameStates) {
            gameStatesDict[node.name] = node;
        }

		if (doRestart) {
			doRestart = false;
			startState = GameState.InProgress;
		}

		ChangeGameStateTo(startState);
	}
	
	void Update () {

	}


	public static void ChangeGameStateTo(GameState state) {
		if (currentState != GameState.Default) {
			GameStateNode lastNode = gameStatesDict[currentState];
			if (lastNode.objectsOff != null && lastNode.objectsOff.Length > 0)
				MassTurnOnOff(lastNode.objectsOff, false);
            SetView(lastNode.view, false);
			switch(currentState) {
				case GameState.MainMenu:
					break;
				case GameState.InProgress:
					break;
				case GameState.GameOver:
					break;
			}
		}

		GameStateNode newNode = gameStatesDict[state];
		if (newNode.objectsOn != null && newNode.objectsOn.Length > 0)
			MassTurnOnOff(newNode.objectsOn, true);

        currentState = state;

		switch(state) {
			case GameState.MainMenu:
				break;
			case GameState.InProgress:
				gameStarted = true;
				break;
			case GameState.GameOver:
				gameStarted = false;
				break;
		}

        SetView(newNode.view, true);
	}

	private static void MassTurnOnOff(GameObject[] objects, bool sign) {
		foreach (GameObject obj in objects) {
			obj.SetActive(sign);
		}
	}

	private void PlaySound(string triggerLine) {
		AudioManager.instance.PlaySound(triggerLine);
	}

	private static void SetView(ViewController view, bool active) {
        if (view is MainMenuScreen) {
            SetScreen<MainMenuScreen>(active);
        } else if (view is GameScreen) {
            SetScreen<GameScreen>(active);
        } else if (view is GameOverScreen) {
            SetScreen<GameOverScreen>(active);
        }
    }

    private static void SetScreen<T>(bool active) {
        if (active)
            Accessor.ScreenViewManager.Show<T>();
        else
            Accessor.ScreenViewManager.Hide<T>();
    }

    private static void SetOverlay<T>(bool active) {
        if (active)
            Accessor.OverlayViewManager.Show<T>();
        else
            Accessor.OverlayViewManager.Hide<T>();
    }
}
