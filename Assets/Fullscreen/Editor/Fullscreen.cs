/// Fullscreen Editor
/// Version 1.1.2
/// Samuel Schultze
/// samuelschultze@gmail.com

using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// Class for making ContainerWindows and EditorWindows fullscreen.
/// </summary>
[InitializeOnLoad]
public class FullscreenEditor {

    #region Constants
    //Menu items paths.
    //See https://docs.unity3d.com/ScriptReference/MenuItem.html for explanation of how to change the shortcuts below.
    //DO NOT LEAVE THE MENU ITEMS WITHOUT A SHORTCUT, YOU MAY GET STUCK IN FULLSCREEN AND LOSE UNSAVED WORK IF YOU DO SO (Learned that the hard way)
    private const string TOOLBAR_PATH = "Fullscreen/Show toolbar";
    private const string FULLSCREEN_ON_PLAY_PATH = "Fullscreen/Fullscreen on play";
    private const string CURRENT_VIEW_PATH = "Fullscreen/Focused View %F9";
    private const string GAME_VIEW_PATH = "Fullscreen/Game View %F10";
    private const string SCENE_VIEW_PATH = "Fullscreen/Scene View %F11";
    private const string MAIN_VIEW_PATH = "Fullscreen/Main View %F12";

    //Do not change this, this is the core of the plugin.
    private const string CLOSE_METHOD = "Close";
    private const string SHOW_METHOD = "ShowPopup";
    private const string RECT_PROPERTY = "position";

    //Keys for the preferences.
    private const string PREFS_TOOLBAR = "FullscreenToolbar";
    private const string PREFS_FULLSCREEN_ON_PLAY = "FullscreenOnPlay";

    //Misc
    private const string TEMP_LAYOUT_PATH = "Temp/TempLayout.dwlt";
    private const BindingFlags FULL_BINDING = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    #endregion

    #region Fields
    private static readonly Assembly editorAssembly = typeof(Editor).Assembly;

    //These are internal classes that are only available through reflection.
    private static readonly Type containerWindowType;
    private static readonly Type gameViewType;

    private static readonly FullscreenEditor mainView;
    private static readonly FullscreenEditor gameView;
    private static readonly FullscreenEditor sceneView;
    private static readonly FullscreenEditor currentView;

    private Object currentOpen;
    private static Vector2 mousePosition;
    #endregion

    #region Properties
    /// <summary>
    /// The name of this fullscreen instance. 
    /// It will be the window type followed by "_Fullscreen" and will be used to find the window across assembly reloads.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Is this window currently in fullscreen mode?
    /// </summary>
    public bool IsOpen { get { return CurrentOpenObject; } }

    /// <summary>
    /// The window type related to this instance.
    /// </summary>
    public Type Type { get; private set; }

    /// <summary>
    /// The rectangle area this window is occupying in the screen.
    /// </summary>
    public Rect Rect {
        get { return IsOpen ? (Rect)Type.GetProperty(RECT_PROPERTY, FULL_BINDING).GetValue(CurrentOpenObject, null) : new Rect(); }
        set { Type.GetProperty(RECT_PROPERTY, FULL_BINDING).SetValue(CurrentOpenObject, value, null); }
    }

    /// <summary>
    /// The object in fullscreen, can be either an EditorWindow or a ContainerWindow, returns null if there is none in fullscreen.
    /// </summary>
    public Object CurrentOpenObject {
        get {
            if(currentOpen)
                return currentOpen;

            var all = Resources.FindObjectsOfTypeAll(Type);

            for(var i = 0; i < all.Length; i++)
                if(all[i] && all[i].name == Name)
                    return currentOpen = all[i];

            return null;
        }
        set {
            currentOpen = value;
        }
    }

    /// <summary>
    /// The current open object as an EditorWindow, returns null if the object is a ContainerWindow or if not in fullscreen.
    /// </summary>
    public EditorWindow CurrentOpenWindow {
        get { return CurrentOpenObject as EditorWindow; }
    }

    private static bool ShowToolbar {
        get { return EditorPrefs.GetBool(PREFS_TOOLBAR, false); }
        set { EditorPrefs.SetBool(PREFS_TOOLBAR, value); }
    }
    private static bool FullscreenOnPlay {
        get { return EditorPrefs.GetBool(PREFS_FULLSCREEN_ON_PLAY, false); }
        set { EditorPrefs.SetBool(PREFS_FULLSCREEN_ON_PLAY, value); }
    }
    #endregion

    #region Constructors
    static FullscreenEditor() {
        containerWindowType = editorAssembly.GetType("UnityEditor.ContainerWindow");
        gameViewType = editorAssembly.GetType("UnityEditor.GameView");

        mainView = new FullscreenEditor(containerWindowType);
        gameView = new FullscreenEditor(gameViewType);
        sceneView = new FullscreenEditor(typeof(SceneView));
        currentView = new FullscreenEditor(typeof(EditorWindow));

        //Workaround for getting the mouse position while not in play mode.
        SceneView.onSceneGUIDelegate += (view) => mousePosition = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
        SceneView.RepaintAll();

        //Open the game view if the game starts running and close if it stops or gets paused, pretty simple.
        EditorApplication.playmodeStateChanged += () => {
            if(FullscreenOnPlay)
                if(EditorApplication.isPlaying && !EditorApplication.isPaused && !gameView)
                    OpenCloseGameView();
                else if(EditorApplication.isPaused || !EditorApplication.isPlaying)
                    gameView.Close();
        };
    }

    /// <summary>
    /// Class for making ContainerWindows or EditorWindows fullscreen 
    /// </summary>
    /// <param name="type">The type of the window that will be in fullscreen</param>
    public FullscreenEditor(Type type) {
        if(type == null)
            throw new ArgumentNullException("You must specify a window type to make it fullscreen");

        if(!IsValidTypeForFullscreen(type))
            throw new ArgumentException("The type must be inherited from \"UnityEditor.EditorWindow\" or \"UnityEditor.ContainerWindow\"");

        Type = type;
        Name = string.Format("{0}_Fullscreen", type);
    }
    #endregion

    #region Instance Methods
    /// <summary>
    /// Show the window in fullscreen
    /// </summary>
    public void Open() {
        //This is just for making sure the fullscreen window settings match the non-fullscreen ones.
        //Like your position in scene view or your custom aspect ratio in game view.
        if(Type == gameViewType)
            Open(GetMainGameView());
        else if(Type == typeof(SceneView))
            Open(SceneView.lastActiveSceneView);
        else
            Open(null);
    }

    /// <summary>
    /// Show the window in fullscreen
    /// </summary>
    /// <param name="reference">The window that will be cloned, must be the inherited from the type passed in the constructor</param>
    public void Open(Object reference) {
        if(IsOpen)
            return;

        CloseAll();

        if(Type == containerWindowType || Type == gameViewType)
            SaveLayout();

        if(reference)
            if(reference.GetType() == Type || reference.GetType().IsSubclassOf(Type))
                CurrentOpenObject = Object.Instantiate(reference);
            else
                Debug.LogWarning("Reference window is not inherited from " + Type + ", it will be ignored");

        if(!CurrentOpenObject)
            CurrentOpenObject = ScriptableObject.CreateInstance(Type);

        CurrentOpenObject.name = Name;
        Type.GetMethod(SHOW_METHOD, FULL_BINDING).Invoke(CurrentOpenObject, null);

        var rect = GetFullscreenRect();

        if(!ShowToolbar && (CurrentOpenObject is SceneView || CurrentOpenObject.GetType() == gameViewType)) {
            //Move the top border of the window 17 pixels up, so the toolbar will be hidden (you can still click on it if you move your mouse too fast).
            rect.yMin -= 17f;
            CurrentOpenWindow.maximized = true;
            //Set minimum and maximum size to the same, so the user can't resize the window dragging the borders.
            CurrentOpenWindow.maxSize = rect.size;
            CurrentOpenWindow.minSize = rect.size;
        }

        Rect = rect;

        if(CurrentOpenWindow)
            CurrentOpenWindow.Focus();

        InternalEditorUtility.RepaintAllViews(); //Prevents the editor from being completely black when switching fullscreen mode of main window.
        InternalEditorUtility.RepaintAllViews(); //But it won't work every time, so we call it again and hope it works.
    }

    /// <summary>
    /// Close the fullscreen window
    /// </summary>
    public void Close() {
        if(IsOpen) {
            Type.GetMethod(CLOSE_METHOD, FULL_BINDING).Invoke(CurrentOpenObject, null);
            if(Type == containerWindowType || Type == gameViewType)
                LoadLayout();
        }
    }
    #endregion

    #region Static Methods
    private static void SaveLayout() {
        try {
            var type = editorAssembly.GetType("UnityEditor.WindowLayout");
            var method = type.GetMethod("SaveWindowLayout", FULL_BINDING);

            method.Invoke(null, new object[] { TEMP_LAYOUT_PATH });
        }
        catch(Exception e) {
            Debug.LogError("Failed to save current layout: " + e);
        }
    }

    private static void LoadLayout() {
        try {
            var type = editorAssembly.GetType("UnityEditor.WindowLayout");
            var method = type.GetMethod("LoadWindowLayout", FULL_BINDING);

            method.Invoke(null, new object[] { TEMP_LAYOUT_PATH, false });
        }
        catch(Exception e) {
            InternalEditorUtility.LoadDefaultLayout();
            Debug.LogException(e);
            Debug.LogError("Error while loading the previous layout, the default layout was loaded instead");
        }
    }

    private static void CloseAll() {
        //Close all default fullscreeens if they're open, this prevents one overlapping another.
        if(gameView)
            gameView.Close();
        if(sceneView)
            sceneView.Close();
        if(currentView)
            currentView.Close();
        if(mainView)
            mainView.Close();
    }

    private static void ShowFullscreenNotification(EditorWindow window, string message, string menuItemPath) {
        var index = menuItemPath.LastIndexOf(" ");

        if(index++ == -1 || !window)
            return;

        var notification = menuItemPath.Substring(index).Replace("_", "");
        var evt = Event.KeyboardEvent(notification);

        notification = InternalEditorUtility.TextifyEvent(evt);

        if(string.Equals(notification, "None", StringComparison.CurrentCultureIgnoreCase))
            return;

        notification = string.Format(message, notification);
        window.ShowNotification(new GUIContent(notification));
        window.Repaint();

        if(EditorWindow.mouseOverWindow)
            EditorWindow.mouseOverWindow.Repaint();
    }

    private static bool IsValidTypeForFullscreen(Type type) {
        if(type == null)
            return false;
        if(type == containerWindowType || type == typeof(EditorWindow))
            return true;
        if(type.IsSubclassOf(containerWindowType) || type.IsSubclassOf(typeof(EditorWindow)))
            return true;
        return false;
    }

    private static Rect GetFullscreenRect() {
        switch(Application.platform) {
            case RuntimePlatform.WindowsEditor: //Gets the rect of the screen where the mouse cursor is, useful if you're using multiple screens.
                SceneView.RepaintAll();
                return InternalEditorUtility.GetBoundsOfDesktopAtPoint(mousePosition);

            default: //GetBoundsOfDesktopAtPoint throws a "NotImplementedException" on OSX, so we won't use it.
                return new Rect(0f, 0f, Screen.currentResolution.width, Screen.currentResolution.height);
        }
    }

    private static Object GetMainView() {
        var containers = Resources.FindObjectsOfTypeAll(containerWindowType);
        var showMode = containerWindowType.GetProperty("showMode", FULL_BINDING);

        for(int i = 0; i < containers.Length; i++)
            if(containers[i] && (int)showMode.GetValue(containers[i], null) == 4)
                return containers[i];

        throw new NullReferenceException("Couldn't find main view");
    }

    private static EditorWindow GetMainGameView() {
        var type = editorAssembly.GetType("UnityEditor.GameView");
        var method = type.GetMethod("GetMainGameView", FULL_BINDING);
        var result = method.Invoke(null, null);

        return (EditorWindow)result;
    }

    private static EditorWindow[] GetGameViews() {
        var type = editorAssembly.GetType("UnityEditor.GameView");
        var field = type.GetField("s_GameViews", FULL_BINDING);
        var result = field.GetValue(null);

        return ((IList)result).Cast<EditorWindow>().ToArray();
    }
    #endregion

    #region MenuItems
    [MenuItem(TOOLBAR_PATH, true)]
    [MenuItem(FULLSCREEN_ON_PLAY_PATH, true)]
    [MenuItem(CURRENT_VIEW_PATH, true)]
    [MenuItem(GAME_VIEW_PATH, true)]
    [MenuItem(SCENE_VIEW_PATH, true)]
    [MenuItem(MAIN_VIEW_PATH, true)]
    private static bool CheckMenuItems() {
        Menu.SetChecked(TOOLBAR_PATH, ShowToolbar);
        Menu.SetChecked(FULLSCREEN_ON_PLAY_PATH, FullscreenOnPlay);
        Menu.SetChecked(CURRENT_VIEW_PATH, currentView);
        Menu.SetChecked(GAME_VIEW_PATH, gameView);
        Menu.SetChecked(SCENE_VIEW_PATH, sceneView);
        Menu.SetChecked(MAIN_VIEW_PATH, mainView);
        return true;
    }

    [MenuItem(TOOLBAR_PATH, false, 0)]
    private static void ToggleToolbar() {
        ShowToolbar = !ShowToolbar;
    }

    [MenuItem(FULLSCREEN_ON_PLAY_PATH, false, 0)]
    private static void ToggleFullscreenOnPlay() {
        FullscreenOnPlay = !FullscreenOnPlay;
    }

    [MenuItem(CURRENT_VIEW_PATH, false, 100)]
    private static void OpenCloseCurrentView() {
        if(!EditorWindow.mouseOverWindow && !currentView)
            return;

        if(EditorWindow.mouseOverWindow == gameView.CurrentOpenWindow) {
            gameView.Close();
            return;
        }

        if(EditorWindow.mouseOverWindow.GetType() == gameViewType) {
            ShowFullscreenNotification(EditorWindow.mouseOverWindow, "Opening Game View using {0} can lead to unexpected behaviour", CURRENT_VIEW_PATH);
            return;
        }

        if(!currentView) {
            currentView.Open(EditorWindow.mouseOverWindow);
            ShowFullscreenNotification(currentView.CurrentOpenObject as EditorWindow, "Press {0} to exit fullscreen", CURRENT_VIEW_PATH);
        }
        else
            currentView.Close();
    }

    [MenuItem(GAME_VIEW_PATH, false, 100)]
    private static void OpenCloseGameView() {
        if(!gameView) {
            var windows = GetGameViews();

            gameView.Open();
            ShowFullscreenNotification(gameView.CurrentOpenObject as EditorWindow, "Press {0} to exit fullscreen", GAME_VIEW_PATH);

            //We need to close all the other game view, Unity has a bug with the Input class if there are two Game Views in different Container Windows.
            for(var i = 0; i < windows.Length; i++)
                windows[i].Close();

            InternalEditorUtility.OnGameViewFocus(true); //UI may not work properly if this is not called.
        }
        else
            gameView.Close();
    }

    [MenuItem(SCENE_VIEW_PATH, false, 100)]
    private static void OpenCloseSceneView() {
        if(!sceneView) {
            sceneView.Open();
            ShowFullscreenNotification(sceneView.CurrentOpenObject as EditorWindow, "Press {0} to exit fullscreen", SCENE_VIEW_PATH);
        }
        else
            sceneView.Close();
    }

    [MenuItem(MAIN_VIEW_PATH, false, 100)]
    private static void OpenCloseMainView() {
        if(!mainView)
            mainView.Open(GetMainView());
        else
            mainView.Close();
    }
    #endregion

    /// <summary>
    /// Quick shortcut for checking if the window is not null and if it's open.
    /// </summary>
    public static implicit operator bool(FullscreenEditor window) {
        if(window == null)
            return false;
        return window.IsOpen;
    }

}