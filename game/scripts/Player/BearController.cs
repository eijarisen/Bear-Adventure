using Godot;

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

    public bool MovementLocked { get; set; }

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
        bool left =
            Input.IsPhysicalKeyPressed(Key.A)
            || Input.IsPhysicalKeyPressed(Key.Left);

        bool right =
            Input.IsPhysicalKeyPressed(Key.D)
            || Input.IsPhysicalKeyPressed(Key.Right);

        bool up =
            Input.IsPhysicalKeyPressed(Key.W)
            || Input.IsPhysicalKeyPressed(Key.Up);

        bool down =
            Input.IsPhysicalKeyPressed(Key.S)
            || Input.IsPhysicalKeyPressed(Key.Down);

        bool space =
            Input.IsPhysicalKeyPressed(Key.Space);

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
        float bob =
            IsOnFloor() && Math.Abs(Velocity.X) > 8.0f
                ? Mathf.Sin(_walkCycle * 2.0f) * 1.4f
                : 0.0f;

        float step = Mathf.Sin(_walkCycle) * 5.0f;
        Color fur = new(0.48f, 0.28f, 0.13f);
        Color darkFur = new(0.30f, 0.16f, 0.07f);
        Color muzzle = new(0.72f, 0.51f, 0.31f);
        Color black = new(0.05f, 0.05f, 0.05f);

        DrawLine(
            new Vector2(-9.0f, 19.0f + bob),
            new Vector2(-10.0f + step, 29.0f),
            darkFur,
            8.0f,
            true);
        DrawLine(
            new Vector2(9.0f, 19.0f + bob),
            new Vector2(10.0f - step, 29.0f),
            darkFur,
            8.0f,
            true);

        DrawCircle(
            new Vector2(0.0f, 5.0f + bob),
            21.0f,
            fur);
        DrawCircle(
            new Vector2(-11.0f, -20.0f + bob),
            8.0f,
            darkFur);
        DrawCircle(
            new Vector2(11.0f, -20.0f + bob),
            8.0f,
            darkFur);
        DrawCircle(
            new Vector2(0.0f, -12.0f + bob),
            18.0f,
            fur);
        DrawCircle(
            new Vector2(4.0f * _facing, -7.0f + bob),
            8.0f,
            muzzle);
        DrawCircle(
            new Vector2(8.0f * _facing, -10.0f + bob),
            2.3f,
            black);
        DrawCircle(
            new Vector2(5.0f * _facing, -17.0f + bob),
            2.0f,
            black);
        DrawCircle(
            new Vector2(-4.5f * _facing, -17.5f + bob),
            1.7f,
            black);

        float armSwing =
            Math.Abs(Velocity.X) > 8.0f
                ? Mathf.Sin(_walkCycle) * 6.0f
                : 0.0f;

        DrawLine(
            new Vector2(-17.0f, 1.0f + bob),
            new Vector2(-22.0f, 13.0f + armSwing + bob),
            fur,
            7.0f,
            true);
        DrawLine(
            new Vector2(17.0f, 1.0f + bob),
            new Vector2(22.0f, 13.0f - armSwing + bob),
            fur,
            7.0f,
            true);
    }

    private void Respawn()
    {
        Position = _respawnPosition;
        Velocity = Vector2.Zero;
        _coyoteRemaining = 0.0;
        _jumpBufferRemaining = 0.0;
        MovementLocked = false;
    }
}
