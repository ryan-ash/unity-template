using UnityEngine;
using System;
using System.Collections.Generic;

public class ViewManager<T> where T: ViewController {

    private const int VIEW_QUEUE_MAX_COUNT = 10;

    public delegate void ViewChangeHandler(ViewManager<T> self);
    public event ViewChangeHandler viewChangedEvent;

    public List<T> ViewList { get; set; }
    private int currentId = -1;
    private List<int> viewQueue;

    public ViewManager( Accessor accessor, Transform screensRoot ) 
    {
        viewQueue = new List<int>();
        ViewList = new List<T>();
        foreach( Transform t in screensRoot ) {
            T sbc = t.GetComponent<T>();
            if( sbc != null ) {
                ViewList.Add( sbc );
                sbc.Accessor = accessor;
                sbc.Deactivate( false );
            }
        }
    }
        
    public void Show<T0>() 
    {
        int i = GetID<T0>();
        Show (i);
    }

    public void Hide<T0>() 
    {
        int i = GetID<T0>();
        Hide (i);
    }

    public void HideCurrent() 
    {
        Hide (currentId);
    }

    public void Back() 
    {
        UpdateQueue( false );
        Show( viewQueue[viewQueue.Count - 1] );
        UpdateQueue( false );
    }

    public void ResetQueue()
    {
        viewQueue.Clear();
        UpdateQueue(true, GetID<MainMenuScreenController>());
    }

    public T0 GetController<T0>() 
    {   
        int id = GetID<T0>();
        if( id > -1 && id < ViewList.Count ) {
            Component c = ViewList[id].GetComponent( typeof(T0) );
            if( c != null ) return (T0)Convert.ChangeType(c, typeof(T0));
        }
        return default(T0);
    }

    public T CurrentController 
    {
        get { return currentId==-1? null: ViewList[currentId].GetComponent<T>(); }
    }
    

    private int GetID<T0>() 
    {
        for( int i=0; i<ViewList.Count; i++ ) {
            Component c = ViewList[i].GetComponent( typeof(T0) );
            if( c != null ) return i;
        }
        return -1;
    }

    // HACK for QuickAccessor Editor
    public void Show(int i) 
    {
        if (i == currentId && !ViewList[i].isActive)
        {
            currentId = -1;
        }

        if (!ViewList[i].isActive)
        {
            ViewList[i].Activate();

            if (currentId > -1)
            {
                ViewList[currentId].Deactivate();
            }

            currentId = i;
            UpdateQueue(true, currentId);

            if (viewChangedEvent != null)
            {
                viewChangedEvent(this);
            }
        }
        else
        {
            ViewList[i].SendMessage("OnEnable", SendMessageOptions.DontRequireReceiver);
        }
    }

    private void Hide( int i ) 
    {
        if( currentId != -1 && ViewList[i].isActive ) {
            ViewList[i].Deactivate();
            currentId = -1;
            UpdateQueue( false );
            if ( viewChangedEvent != null ) viewChangedEvent( this );
        }
    }

    private void UpdateQueue( bool forward, int id=-1 ) 
    {
        if( forward ) {
            viewQueue.Add( id );
            if( viewQueue.Count > VIEW_QUEUE_MAX_COUNT ) {
                viewQueue.RemoveAt(0);
            }
        } else {
            viewQueue.RemoveAt( viewQueue.Count - 1 );
        }

    }
}
