using Godot;

namespace BearAdventure.Shell;

public static class BindingId
{
    public const string Left="left";
    public const string Right="right";
    public const string Up="up";
    public const string Down="down";
    public const string Jump="jump";
    public const string Harvest="harvest";
    public const string Use="use";
    public const string Fish="fish";
    public const string Layer="layer";
    public const string Inventory="inventory";
    public const string Crafting="crafting";
    public const string Map="map";
    public const string Friends="friends";

    public static IReadOnlyList<string> All { get; } =
    [Left,Right,Up,Down,Jump,Harvest,Use,Fish,Layer,Inventory,Crafting,Map,Friends];

    public static string Display(string id) => id switch
    {
        Left=>"Move left",Right=>"Move right",Up=>"Move / climb up",Down=>"Climb down",Jump=>"Jump",
        Harvest=>"Harvest / mine",Use=>"Use / talk",Fish=>"Fish",Layer=>"Build layer",Inventory=>"Inventory",
        Crafting=>"Crafting",Map=>"Map",Friends=>"Friends",_=>id
    };
}

public sealed class GameSettings
{
    public bool Fullscreen { get; set; }
    public float UiScale { get; set; } = 1.0f;
    public bool SoundEnabled { get; set; } = true;
    public int LastSlot { get; set; } = 1;
    public Dictionary<string,long> Keys { get; set; } = Defaults();

    public static Dictionary<string,long> Defaults() => new()
    {
        [BindingId.Left]=(long)Key.A,
        [BindingId.Right]=(long)Key.D,
        [BindingId.Up]=(long)Key.W,
        [BindingId.Down]=(long)Key.S,
        [BindingId.Jump]=(long)Key.Space,
        [BindingId.Harvest]=(long)Key.F,
        [BindingId.Use]=(long)Key.R,
        [BindingId.Fish]=(long)Key.G,
        [BindingId.Layer]=(long)Key.Q,
        [BindingId.Inventory]=(long)Key.E,
        [BindingId.Crafting]=(long)Key.C,
        [BindingId.Map]=(long)Key.M,
        [BindingId.Friends]=(long)Key.T,
    };

    public Key KeyFor(string id)
    {
        if(Keys is not null && Keys.TryGetValue(id,out long raw) && raw>0) return (Key)raw;
        return (Key)Defaults()[id];
    }

    public void SetKey(string id,Key key)
    {
        if(!BindingId.All.Contains(id)) throw new ArgumentOutOfRangeException(nameof(id));
        Keys ??= Defaults(); long next=(long)key; long previous=Keys.TryGetValue(id,out long old)?old:Defaults()[id];
        string? conflict=Keys.FirstOrDefault(pair=>pair.Key!=id && pair.Value==next).Key;
        if(!string.IsNullOrEmpty(conflict)) Keys[conflict]=previous;
        Keys[id]=next;
    }

    public void Normalize()
    {
        if(!float.IsFinite(UiScale) || UiScale<0.85f || UiScale>1.15f) UiScale=1.0f;
        LastSlot=Math.Clamp(LastSlot,1,3);
        Keys ??= Defaults();
        foreach(string id in BindingId.All) if(!Keys.ContainsKey(id)) Keys[id]=Defaults()[id];
    }
}
