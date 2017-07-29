public class Accessor  {

    public DataAccessor Data { get; set; }

    public SettingsManager Settings { get; set; }

    public ViewManager<ScreenBaseController> ScreenViewManager { get; set; }
    public ViewManager<OverlayBaseController> OverlayViewManager { get; set; }

    public static Accessor instance;

    public Accessor()
    {
        instance = this;
    }
}
