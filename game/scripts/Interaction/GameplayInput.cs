using BearAdventure.Domain.Gameplay;
using BearAdventure.Shell;
using Godot;

namespace BearAdventure.Interaction;

public enum MenuCommand { Inventory, Crafting, Map, Friends, Escape }

/// <summary>Only this node owns device input and SceneTree pause. Releases are observed before GUI dispatch.</summary>
public partial class GameplayInput : Node
{
    public WorldInputState State {get;} = new();
    public event Action<MenuCommand>? MenuRequested;
    public event Action? Cancelled;
    public event Action? FocusLost;
    public event Action<string,Key>? BindingChanged;
    private bool _modal, _unfocused, _transition, _loading;
    private string? _pendingBinding;
    private readonly Dictionary<Key,GameAction> _keyActions=new();
    private readonly Dictionary<Key,MenuCommand> _menuKeys=new();
    private GameSettings _settings=new();
    public bool BlocksWorld => _modal || _unfocused || _transition || _loading;
    public override void _Ready() { ProcessMode=ProcessModeEnum.Always; ApplyBindings(_settings); State.Cancel(); }
    public void ApplyBindings(GameSettings settings)
    {
        _settings=settings ?? throw new ArgumentNullException(nameof(settings));
        _settings.Normalize();
        _keyActions.Clear(); _menuKeys.Clear();
        BindAction(BindingId.Left,GameAction.Left); BindAction(BindingId.Right,GameAction.Right);
        BindAction(BindingId.Up,GameAction.Up); BindAction(BindingId.Down,GameAction.Down);
        BindAction(BindingId.Jump,GameAction.Jump); BindAction(BindingId.Harvest,GameAction.Harvest);
        BindAction(BindingId.Use,GameAction.Use); BindAction(BindingId.Fish,GameAction.Fish); BindAction(BindingId.Layer,GameAction.Layer);
        BindMenu(BindingId.Inventory,MenuCommand.Inventory); BindMenu(BindingId.Crafting,MenuCommand.Crafting);
        BindMenu(BindingId.Map,MenuCommand.Map); BindMenu(BindingId.Friends,MenuCommand.Friends);
    }
    private void BindAction(string id,GameAction action) => _keyActions[_settings.KeyFor(id)]=action;
    private void BindMenu(string id,MenuCommand command) => _menuKeys[_settings.KeyFor(id)]=command;
    public void BeginRebind(string bindingId)
    {
        if(!BindingId.All.Contains(bindingId)) return;
        _pendingBinding=bindingId; CancelActions();
    }
    public void SetModal(bool value) { _modal=value; Apply(); }
    public void SetTransition(bool value) { _transition=value; Apply(); }
    public void SetLoading(bool value) { _loading=value; Apply(); }
    public void CancelActions() { State.Cancel(); Cancelled?.Invoke(); }
    private void Apply()
    {
        State.SetBlocked(BlocksWorld); Cancelled?.Invoke();
        if(IsInsideTree()) GetTree().Paused=BlocksWorld;
    }
    public override void _Notification(int what)
    {
        if(what==NotificationApplicationFocusOut)
        { _unfocused=true; Apply(); FocusLost?.Invoke(); }
        else if(what==NotificationApplicationFocusIn)
        { _unfocused=false; Apply(); }
    }
    public override void _Process(double delta)
    {
        _=delta;
        bool neutral=!Input.IsMouseButtonPressed(MouseButton.Left) && !Input.IsMouseButtonPressed(MouseButton.Right)
            && !_keyActions.Keys.Any(key => Input.IsPhysicalKeyPressed(key));
        State.ObserveNeutral(neutral);
    }
    public override void _Input(InputEvent e)
    {
        if(e is not InputEventKey key) 
        {
            if(e is InputEventMouseButton mouse && !mouse.Pressed)
            {
                if(mouse.ButtonIndex==MouseButton.Left) State.Release(GameAction.Build);
                if(mouse.ButtonIndex==MouseButton.Right) State.Release(GameAction.Remove);
            }
            return;
        }

        Key code=key.PhysicalKeycode==Key.None?key.Keycode:key.PhysicalKeycode;
        if(_pendingBinding is not null && key.Pressed && !key.Echo)
        {
            if(code!=Key.Escape)
            {
                _settings.SetKey(_pendingBinding,code);
                string changed=_pendingBinding;
                _pendingBinding=null;
                ApplyBindings(_settings);
                BindingChanged?.Invoke(changed,code);
            }
            else _pendingBinding=null;
            GetViewport().SetInputAsHandled(); return;
        }

        if(_loading) return;
        if(!key.Pressed && _keyActions.TryGetValue(code,out var a)) State.Release(a);
        if(!key.Pressed || key.Echo) return;
        if(code==Key.Escape) { MenuRequested?.Invoke(MenuCommand.Escape); GetViewport().SetInputAsHandled(); return; }
        if(_menuKeys.TryGetValue(code,out var menu)) { MenuRequested?.Invoke(menu); GetViewport().SetInputAsHandled(); }
    }
    public override void _UnhandledInput(InputEvent e)
    {
        if(BlocksWorld) return;
        if(e is InputEventKey key && key.Pressed && !key.Echo)
        {
            Key code=key.PhysicalKeycode==Key.None?key.Keycode:key.PhysicalKeycode;
            if(_keyActions.TryGetValue(code,out var action)) { State.Press(action); GetViewport().SetInputAsHandled(); }
        }
        else if(e is InputEventMouseButton mouse && mouse.Pressed)
        {
            if(mouse.ButtonIndex==MouseButton.Left) { State.Press(GameAction.Build); GetViewport().SetInputAsHandled(); }
            else if(mouse.ButtonIndex==MouseButton.Right) { State.Press(GameAction.Remove); GetViewport().SetInputAsHandled(); }
        }
    }
}
