namespace BearAdventure.Domain.Gameplay;

public enum GameAction { Left, Right, Up, Down, Jump, Harvest, Use, Fish, Build, Remove, Layer }
public sealed class WorldInputState
{
    private readonly HashSet<GameAction> _held = new();
    private readonly HashSet<GameAction> _pressed = new();
    public bool Blocked { get; private set; }
    public bool AwaitingRelease { get; private set; }
    public bool Held(GameAction action) => !Blocked && !AwaitingRelease && _held.Contains(action);
    public bool ConsumePress(GameAction action) => !Blocked && !AwaitingRelease && _pressed.Remove(action);
    public void Press(GameAction action)
    {
        if (Blocked || AwaitingRelease) return;
        if (_held.Add(action)) _pressed.Add(action);
    }
    public void Release(GameAction action) { _held.Remove(action); _pressed.Remove(action); }
    public void Cancel() { _held.Clear(); _pressed.Clear(); AwaitingRelease = true; }
    public void SetBlocked(bool blocked) { Blocked = blocked; Cancel(); }
    public void ObserveNeutral(bool noDeviceActionsHeld)
    { if (noDeviceActionsHeld) AwaitingRelease = false; }
}
