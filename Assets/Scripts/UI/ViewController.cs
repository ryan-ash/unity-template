using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent (typeof (CanvasGroup))]
public class ViewController : MonoBehaviour {

    public Accessor Accessor { get; set; }

    public bool isActive { get; set; }

    private bool isScreen = false;
    private ScreenBaseController screen;
    
	private CanvasGroup _cgCache;
	public CanvasGroup cgCache {
		get { 
			if (_cgCache == null) {
				_cgCache = GetComponent<CanvasGroup>();
			}
			return _cgCache;
		}
	}
	
    public void PlaySound(string triggerName) 
    {
        SendMessageUpwards( "FireTrigger", triggerName );
    }

    /**
     * TODO: Make sure that ViewManager gets updated on OnEnable/OnDisable and SetActive
     */
    public void Activate(bool animate = true) 
    {
        isActive = true;
        gameObject.SetActive( true );
        
        if(!screen)
            screen = this.GetComponent<ScreenBaseController>();
        if(screen){
            isScreen = true;
            screen.ScreenEnabledTime = Time.time;
        }
        
        GameObject scrollView = getChildGameObject(gameObject, "ScrollView");
        ScrollRect scrollRect;
        if (scrollView != null) {
            scrollRect = scrollView.GetComponent<ScrollRect>();
            scrollRect.verticalNormalizedPosition = 1;
        }
        if( animate ) {
            cgCache.alpha = 0f;
			LeanTween.value(gameObject, (float v) => { cgCache.alpha = v; }, 0, 1, 0.35f );
        }
    }
    
    public GameObject getChildGameObject(GameObject fromGameObject, string withName) {
        Transform[] ts = fromGameObject.transform.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in ts) if (t.gameObject.name == withName && t.gameObject.activeInHierarchy) return t.gameObject;
        return null;
    }

    /**
     * TODO: Make sure that ViewManger gets updated on OnEnable/OnDisable and SetActive
     */
    public void Deactivate( bool animate=true ) 
    {
        isActive = false;
        if( animate ) {
            LeanTween.value( gameObject, (float v)=>{ cgCache.alpha = v; }, 1, 0, 0.175f ).setOnComplete( () => gameObject.SetActive(false));
        } else {
            gameObject.SetActive( false );
        }
    }

    public virtual void OnBackgrounded()
    {
        // Debug.Log("App backgrounded event not handled");
    }

    public virtual void OnForegrounded(int? backgroundedSecs)
    {
        // Debug.Log("App foregrounded event not handled");
    }
}
