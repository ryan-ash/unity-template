using UnityEngine;
using System.Collections;

public class OverlayBaseController : ViewController {

    public delegate void OnCloseHandler();

    public OnCloseHandler OnNextClose;

    public void CloseSelf() 
    {
        Accessor.OverlayViewManager.HideCurrent();

        if (OnNextClose != null) {
            OnNextClose();
            OnNextClose = null;
        }
    }
}
