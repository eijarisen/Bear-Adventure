using BearAdventure.Domain.Gameplay;
using Godot;
using BearAdventure.Rendering.PixelArt;

namespace BearAdventure.Player;

public partial class BearController : CharacterBody2D
{
    private const float MoveSpeed = 360.0f;
    private const float GroundAcceleration = 2800.0f;
    private const float AirAcceleration = 1450.0f;
    private const float Gravity = 1650.0f;
    private const float JumpVelocity = -575.0f;
    private const float ClimbSpeed = 245.0f;
    private const float ClimbAcceleration = 1800.0f;
    private const double CoyoteTimeSeconds = 0.10;
    private const double JumpBufferSeconds = 0.12;

    private bool _jumpWasDown;
    private double _coyoteRemaining;
    private double _jumpBufferRemaining;
    private float _walkCycle;
    private int _facing = 1;

    private float _landLeftX;
    private float _landRightX;
    private Vector2 _respawnPosition;

    public WorldInputState? InputState { get; set; }
    public bool MovementLocked { get; set; }
    public bool FishingActive { get; set; }
    public Func<Vector2>? SafeRespawn { get; set; }
    public void CancelMotion()
    {
        Velocity=Vector2.Zero; MovementLocked=false; FishingActive=false; _coyoteRemaining=0; _jumpBufferRemaining=0; _jumpWasDown=true;
    }
    public void PlaceAt(Vector2 position)
    {
        Position=position; CancelMotion();
        GetNodeOrNull<Camera2D>("Camera")?.ResetSmoothing();
    }

    public bool ClimbEnabled { get; set; }

    public override void _Ready()
    {
        var shape = new RectangleShape2D
        {
            Size = new Vector2(34.0f, 58.0f),
        };

        AddChild(new CollisionShape2D
        {
            Shape = shape,
        });

        ZIndex = 20;
        QueueRedraw();
    }

    public void ConfigureIsland(
        float landLeftX,
        float landRightX,
        Vector2 respawnPosition)
    {
        _landLeftX = landLeftX;
        _landRightX = landRightX;
        _respawnPosition = respawnPosition;
        Respawn();
    }

    public void ConfigureCamera(
        float worldLeft,
        float worldRight,
        float worldTop,
        float worldBottom)
    {
        Camera2D camera =
            GetNodeOrNull<Camera2D>("Camera")
            ?? new Camera2D { Name = "Camera" };

        if (camera.GetParent() is null)
        {
            AddChild(camera);
        }

        camera.PositionSmoothingEnabled = true;
        camera.PositionSmoothingSpeed = 7.5f;
        camera.LimitLeft = Mathf.FloorToInt(worldLeft);
        camera.LimitRight = Mathf.CeilToInt(worldRight);
        camera.LimitTop = Mathf.FloorToInt(worldTop);
        camera.LimitBottom = Mathf.CeilToInt(worldBottom);
        camera.Position = new Vector2(0.0f, -70.0f);
        camera.Enabled = true;
    }

    public override void _PhysicsProcess(double delta)
    {
        bool left = InputState?.Held(GameAction.Left) ?? false;
        bool right = InputState?.Held(GameAction.Right) ?? false;
        bool up = InputState?.Held(GameAction.Up) ?? false;
        bool down = InputState?.Held(GameAction.Down) ?? false;
        bool space = InputState?.Held(GameAction.Jump) ?? false;

        bool jumpDown =
            space
            || (!ClimbEnabled && up);

        float axis = MovementLocked
            ? 0.0f
            : (right ? 1.0f : 0.0f)
                - (left ? 1.0f : 0.0f);

        if (axis != 0.0f)
        {
            _facing =
                axis > 0.0f
                    ? 1
                    : -1;
        }

        Vector2 velocity =
            Velocity;

        float acceleration =
            IsOnFloor()
                ? GroundAcceleration
                : AirAcceleration;

        velocity.X =
            Mathf.MoveToward(
                velocity.X,
                axis * MoveSpeed,
                acceleration * (float)delta);

        bool climbing =
            ClimbEnabled;

        if (climbing)
        {
            float verticalAxis =
                MovementLocked
                    ? 0.0f
                    : (down ? 1.0f : 0.0f)
                        - (up ? 1.0f : 0.0f);

            velocity.Y =
                MovementLocked
                    ? 0.0f
                    : Mathf.MoveToward(
                        velocity.Y,
                        verticalAxis * ClimbSpeed,
                        ClimbAcceleration
                            * (float)delta);

            _coyoteRemaining = 0.0;
            _jumpBufferRemaining = 0.0;
        }
        else
        {
            if (IsOnFloor())
            {
                _coyoteRemaining =
                    CoyoteTimeSeconds;
            }
            else
            {
                _coyoteRemaining =
                    Math.Max(
                        0.0,
                        _coyoteRemaining - delta);

                velocity.Y +=
                    Gravity
                    * (float)delta;
            }

            if (!MovementLocked
                && jumpDown
                && !_jumpWasDown)
            {
                _jumpBufferRemaining =
                    JumpBufferSeconds;
            }
            else
            {
                _jumpBufferRemaining =
                    Math.Max(
                        0.0,
                        _jumpBufferRemaining - delta);
            }

            if (!MovementLocked
                && _jumpBufferRemaining > 0.0
                && _coyoteRemaining > 0.0)
            {
                velocity.Y =
                    JumpVelocity;

                _jumpBufferRemaining = 0.0;
                _coyoteRemaining = 0.0;
            }

            if (!jumpDown
                && velocity.Y < -170.0f)
            {
                velocity.Y =
                    Mathf.MoveToward(
                        velocity.Y,
                        -170.0f,
                        1450.0f
                            * (float)delta);
            }
        }

        _jumpWasDown =
            jumpDown;

        Velocity =
            velocity;

        MoveAndSlide();

        if ((Position.X < _landLeftX
                || Position.X > _landRightX)
            && Position.Y > 80.0f)
        {
            Respawn();
        }
        if (Position.Y > 4896.0f) Respawn();

        if (Math.Abs(Velocity.X) > 8.0f
            && IsOnFloor())
        {
            _walkCycle +=
                (float)delta * 9.0f;
        }

        QueueRedraw();
    }

    public override void _Draw()
    {
        // Read-only presentation. The collider and all movement constants remain unchanged.
        double clock = Time.GetTicksMsec() / 1000.0;
        string pose;
        int frame;
        if (MovementLocked)
        {
            pose = "work";
            frame = (int)(clock * 7.0) % 4;
        }
        else if (ClimbEnabled && !IsOnFloor())
        {
            pose = "climb";
            frame = Math.Abs(Velocity.Y) > 8.0f ? (int)(clock * 7.0) % 4 : 0;
        }
        else if (!IsOnFloor())
        {
            pose = "jump";
            frame = Velocity.Y < 0 ? 0 : 1;
        }
        else if (Math.Abs(Velocity.X) > 8.0f)
        {
            pose = "walk";
            frame = (int)(_walkCycle * 1.8f) % 8;
        }
        else
        {
            pose = "idle";
            frame = clock % 7.0 > 6.82 ? 3 : (int)(clock * 1.5) % 3;
        }
        PixelAtlas.DrawBottom(this, $"bear/{pose}/{frame}", new Vector2(0, 31),
            flip: _facing < 0);
        if(FishingActive)
        {
            float side=24*_facing;
            PixelAtlas.DrawBottom(this,"icon/FishingRod",new Vector2(side,15),0.9f);
            DrawLine(new Vector2(side+8*_facing,-2),new Vector2(side+34*_facing,32),PixelAtlas.ToColor(PxColor.Panel1),2);
        }
    }

    private void Respawn()
    {
        Position = SafeRespawn?.Invoke() ?? _respawnPosition;
        Velocity = Vector2.Zero;
        _coyoteRemaining = 0.0;
        _jumpBufferRemaining = 0.0;
        MovementLocked = false;
    }
}
