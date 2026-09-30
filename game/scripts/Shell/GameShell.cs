using BearAdventure.Persistence;
using BearAdventure.Rendering.PixelArt;
using Godot;

namespace BearAdventure.Shell;

public partial class GameShell : CanvasLayer
{
    private enum Screen { Hidden, Title, NewGame, Load, Settings, ResetConfirm }
    private Screen _screen;
    private Control _root=null!;
    private ColorRect _veil=null!, _panel=null!;
    private Label _title=null!, _subtitle=null!;
    private Control _body=null!;
    private LineEdit? _seed;
    private Func<IReadOnlyList<SaveSlotSummary>>? _slots;
    private Func<GameSettings>? _settings;
    private bool _settingsFromTitle=true;
    private int _resetSlot;

    public bool VisibleShell => _screen!=Screen.Hidden;
    public event Action<int>? ContinueRequested;
    public event Action<int,string>? NewGameRequested;
    public event Action<int>? LoadSlotRequested;
    public event Action<int>? ResetSlotRequested;
    public event Action<GameSettings>? SettingsChanged;
    public event Action<string>? RebindRequested;
    public event Action? QuitRequested;

    public override void _Ready()
    {
        ProcessMode=ProcessModeEnum.Always; Layer=100;
        _root=new Control{MouseFilter=Control.MouseFilterEnum.Ignore}; AddChild(_root); _root.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        _veil=new ColorRect{Color=new Color(0.025f,0.03f,0.04f,0.97f),MouseFilter=Control.MouseFilterEnum.Stop}; _root.AddChild(_veil); _veil.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        _panel=new ColorRect{Position=new Vector2(260,96),Size=new Vector2(760,540),MouseFilter=Control.MouseFilterEnum.Stop}; _veil.AddChild(_panel); PixelUi.Decorate(_panel);
        _title=Label(_panel,"BEAR ADVENTURE",new Vector2(26,20),new Vector2(708,40),30); _title.HorizontalAlignment=HorizontalAlignment.Center;
        _subtitle=Label(_panel,$"Desktop build {BearAdventure.UI.GameHud.BuildId}",new Vector2(26,64),new Vector2(708,28),15); _subtitle.HorizontalAlignment=HorizontalAlignment.Center;
        _body=new Control{Position=new Vector2(28,108),Size=new Vector2(704,400),MouseFilter=Control.MouseFilterEnum.Ignore}; _panel.AddChild(_body);
        PixelUi.StyleLabels(_root);
    }

    public void Configure(Func<IReadOnlyList<SaveSlotSummary>> slots,Func<GameSettings> settings)
    { _slots=slots; _settings=settings; }

    public void ShowTitle()
    { _screen=Screen.Title; _root.Visible=true; Render(); }
    public void HideShell()
    { _screen=Screen.Hidden; _root.Visible=false; GetViewport().GuiReleaseFocus(); }
    public void ShowSettings(bool fromTitle)
    { _settingsFromTitle=fromTitle; _screen=Screen.Settings; _root.Visible=true; Render(); }
    public void NotifyBindingChanged() { if(_screen==Screen.Settings) Render(); }
    public void ApplyUiScale(float scale)
    { if(_root is null) return; scale=Mathf.Clamp(scale,0.85f,1.15f); _root.Scale=new Vector2(scale,scale); _root.Position=new Vector2(1280f*(1f-scale)/2f,720f*(1f-scale)/2f); }

    private void Render()
    {
        foreach(Node child in _body.GetChildren()){_body.RemoveChild(child);child.QueueFree();}
        switch(_screen)
        {
            case Screen.Title: RenderTitle(); break;
            case Screen.NewGame: RenderNewGame(); break;
            case Screen.Load: RenderLoad(); break;
            case Screen.Settings: RenderSettings(); break;
            case Screen.ResetConfirm: RenderResetConfirm(); break;
        }
        PixelUi.StyleLabels(_body);
    }

    private IReadOnlyList<SaveSlotSummary> Slots() => _slots?.Invoke() ?? Array.Empty<SaveSlotSummary>();
    private GameSettings Settings() => _settings?.Invoke() ?? new GameSettings();

    private void RenderTitle()
    {
        _title.Text="BEAR ADVENTURE"; _subtitle.Text=$"Desktop build {BearAdventure.UI.GameHud.BuildId} · generated pixel edition";
        var slots=Slots(); GameSettings settings=Settings();
        SaveSlotSummary? preferred=slots.FirstOrDefault(s=>s.Slot==settings.LastSlot && s.Loadable) ?? slots.FirstOrDefault(s=>s.Loadable);
        var continueButton=Button(_body,preferred is null?"Continue — no saved world":"Continue — Slot "+preferred.Slot,new Vector2(130,24),new Vector2(444,56),()=>{if(preferred is not null) ContinueRequested?.Invoke(preferred.Slot);});
        continueButton.Disabled=preferred is null;
        Button(_body,"New world",new Vector2(130,96),new Vector2(444,56),()=>{_screen=Screen.NewGame;Render();});
        Button(_body,"Load / reset worlds",new Vector2(130,168),new Vector2(444,56),()=>{_screen=Screen.Load;Render();});
        Button(_body,"Settings",new Vector2(130,240),new Vector2(444,56),()=>ShowSettings(true));
        Button(_body,"Quit",new Vector2(130,312),new Vector2(444,56),()=>QuitRequested?.Invoke());
    }

    private void RenderNewGame()
    {
        _title.Text="NEW WORLD"; _subtitle.Text="Choose a seed and an empty save slot";
        Label(_body,"World seed",new Vector2(20,8),new Vector2(160,28),16);
        _seed=new LineEdit{Position=new Vector2(20,40),Size=new Vector2(664,42),Text=NewSeedSuggestion(),PlaceholderText="Any text becomes a deterministic world seed"}; _body.AddChild(_seed);
        int y=106;
        foreach(var slot in Slots())
        {
            int captured=slot.Slot;
            string status=slot.Exists?"occupied — reset it from Load / reset first":"empty";
            var b=Button(_body,$"Slot {captured} · {status}",new Vector2(62,y),new Vector2(580,48),()=>
            {
                string seed=(_seed?.Text??string.Empty).Trim(); if(seed.Length==0) seed=NewSeedSuggestion(); NewGameRequested?.Invoke(captured,seed);
            });
            b.Disabled=slot.Exists; y+=60;
        }
        Button(_body,"Back",new Vector2(202,326),new Vector2(300,46),()=>ShowTitle());
    }

    private void RenderLoad()
    {
        _title.Text="SAVE SLOTS"; _subtitle.Text="Load an existing world or permanently reset a slot";
        int y=8;
        foreach(var slot in Slots())
        {
            string when=slot.ModifiedUtc.HasValue?slot.ModifiedUtc.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm"):"—";
            string detail=slot.Exists?(slot.Loadable?$"seed {ShortSeed(slot.Seed)} · island {slot.CurrentIslandId} · {when}":"unreadable · "+slot.StatusText):"empty";
            Label(_body,$"Slot {slot.Slot} · {detail}",new Vector2(8,y),new Vector2(688,36),14);
            int captured=slot.Slot;
            var load=Button(_body,"Load",new Vector2(356,y+38),new Vector2(150,38),()=>LoadSlotRequested?.Invoke(captured)); load.Disabled=!slot.Loadable;
            var reset=Button(_body,"Reset",new Vector2(518,y+38),new Vector2(150,38),()=>{_resetSlot=captured;_screen=Screen.ResetConfirm;Render();}); reset.Disabled=!slot.Exists;
            y+=94;
        }
        Button(_body,"Back",new Vector2(202,326),new Vector2(300,46),()=>ShowTitle());
    }

    private void RenderResetConfirm()
    {
        _title.Text="RESET SLOT"; _subtitle.Text=$"Slot {_resetSlot} will be permanently cleared";
        Label(_body,"This deletes the slot's primary save, backup, migration copies and rejected-write evidence. This cannot be undone.",new Vector2(42,52),new Vector2(620,92),18);
        Button(_body,"CONFIRM RESET",new Vector2(132,184),new Vector2(440,58),()=>{ResetSlotRequested?.Invoke(_resetSlot);_screen=Screen.Load;Render();});
        Button(_body,"Cancel",new Vector2(202,270),new Vector2(300,48),()=>{_screen=Screen.Load;Render();});
    }

    private void RenderSettings()
    {
        _title.Text="SETTINGS"; _subtitle.Text="Changes are stored outside world saves";
        GameSettings settings=Settings(); int y=0;
        Button(_body,settings.Fullscreen?"Display: Fullscreen":"Display: Windowed",new Vector2(8,y),new Vector2(330,42),()=>{settings.Fullscreen=!settings.Fullscreen;SettingsChanged?.Invoke(settings);Render();});
        Button(_body,$"UI scale: {settings.UiScale:0.00}×",new Vector2(354,y),new Vector2(330,42),()=>{settings.UiScale=settings.UiScale<0.9f?1.0f:settings.UiScale<1.05f?1.15f:0.85f;SettingsChanged?.Invoke(settings);Render();}); y+=54;
        Button(_body,settings.SoundEnabled?"Feedback sounds: On":"Feedback sounds: Off",new Vector2(8,y),new Vector2(330,42),()=>{settings.SoundEnabled=!settings.SoundEnabled;SettingsChanged?.Invoke(settings);Render();});
        Button(_body,"Restore default controls",new Vector2(354,y),new Vector2(330,42),()=>{settings.Keys=GameSettings.Defaults();SettingsChanged?.Invoke(settings);Render();}); y+=58;
        Label(_body,"Controls — click a binding, then press the new key (Esc cancels)",new Vector2(8,y),new Vector2(680,28),14); y+=30;
        var scroll=new ScrollContainer{Position=new Vector2(8,y),Size=new Vector2(676,220),HorizontalScrollMode=ScrollContainer.ScrollMode.Disabled}; _body.AddChild(scroll);
        var list=new VBoxContainer{CustomMinimumSize=new Vector2(650,0)};scroll.AddChild(list);
        foreach(string id in BindingId.All)
        {
            string captured=id; var b=new Button{Text=$"{BindingId.Display(id)}: {settings.KeyFor(id)}",CustomMinimumSize=new Vector2(630,34),FocusMode=Control.FocusModeEnum.None}; PixelUi.StyleSimpleButton(b); list.AddChild(b);
            b.Pressed+=()=>RebindRequested?.Invoke(captured);
        }
        Button(_body,"Back",new Vector2(202,352),new Vector2(300,44),()=>{if(_settingsFromTitle)ShowTitle();else HideShell();});
    }

    private static string NewSeedSuggestion() => "bear-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
    private static string ShortSeed(string value) => value.Length<=26?value:value[..23]+"…";
    private static Label Label(Node parent,string text,Vector2 pos,Vector2 size,int font)
    { var l=new Label{Text=text,Position=pos,Size=size,AutowrapMode=TextServer.AutowrapMode.WordSmart};l.AddThemeFontSizeOverride("font_size",font);parent.AddChild(l);return l; }
    private static Button Button(Node parent,string text,Vector2 pos,Vector2 size,Action action)
    { var b=new Button{Text=text,Position=pos,Size=size,FocusMode=Control.FocusModeEnum.None};PixelUi.StyleSimpleButton(b);parent.AddChild(b);b.Pressed+=action;return b; }
}
