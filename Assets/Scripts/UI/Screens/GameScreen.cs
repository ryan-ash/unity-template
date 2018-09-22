using UnityEngine;

public class GameScreen : ScreenBaseController {

    private static GameScreen instance;

    void Start()
    {
        instance = this;
    }

    public void OnEnable() 
    {
            
    }
}
