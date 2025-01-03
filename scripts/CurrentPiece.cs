using Godot;
using Godot.Collections;
using Stacker.Scripts.Autoloads;
using Stacker.Scripts.CustomResources;
using System;
using System.Collections.Generic;
using static Godot.TextServer;

namespace Stacker.Scripts;
public partial class CurrentPiece : Node2D
{
    [Signal]
    public delegate void PieceLockedEventHandler(Array<Node2D> parts);

    private const double TimeBetweenInputs = 0.05;
    private const double LockDelay = 0.5;
    private const int MaxResets = 15;

    [Export]
    private PieceShape _shape;

    [Export]
    private PackedScene _piecePart;

    private Array<Node2D> _parts;

    private double _maxTimeInRow;
    private int _level;
    private int _remainingResets;
    private float _maxX;
    private float _maxY;

    private bool _canFall;
    private bool _input;
    private bool[,] _boardSquares;

    private Timer _lockTimer;
    private Timer _dropTimer;

    private TileMapLayer _board;

    public PieceShape Shape { get => _shape; }

    public bool[,] BoardSquares { get => _boardSquares; set => _boardSquares = value; }

    public TileMapLayer Board { get => _board; set => _board = value; }

    public override void _Ready()
    {
        _input = true;
        _level = 1;
        _remainingResets = MaxResets;
        _maxTimeInRow = CalculateGravity();
        _parts = new Array<Node2D>();
        _canFall = true;

        _lockTimer = new Timer();
        AddChild(_lockTimer);
        _lockTimer.OneShot = true;
        _lockTimer.WaitTime = LockDelay;
        _lockTimer.Timeout += LockPiece;
        _lockTimer.Timeout += () => GD.Print("Timeout");

        _dropTimer = new Timer();
        AddChild(_dropTimer);
        _dropTimer.OneShot = true;
        _dropTimer.Timeout += Drop;
    }

    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if (@event.IsActionPressed("rotate_counterclock"))
        {
            _shape.RotateClockwise();

            if (_lockTimer.TimeLeft > 0 && _remainingResets > 0)
            {
                _lockTimer.Start();
                _remainingResets--;
            }

            DrawPiece();
            UpdateBounds();
            CorrectPosition();
        }
        else if (@event.IsActionPressed("rotate_clock"))
        {
            _shape.RotateCounterClockwise();

            if (_lockTimer.TimeLeft > 0 && _remainingResets > 0)
            {
                _lockTimer.Start();
                _remainingResets--;
            }

            DrawPiece();
            UpdateBounds();
            CorrectPosition();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_input)
        {
            if (Input.IsActionPressed("move_left") && CheckMovementX(1))
            {
                Position -= new Vector2(GlobalVariables.PiecePartSize, 0);
                if (_lockTimer.TimeLeft > 0 && _remainingResets > 0)
                {
                    _lockTimer.Start();
                    _remainingResets--;
                }
            }
            else if (Input.IsActionPressed("move_right") && CheckMovementX(-1))
            {
                Position += new Vector2(GlobalVariables.PiecePartSize, 0);
                if (_lockTimer.TimeLeft > 0 && _remainingResets > 0)
                {
                    _lockTimer.Start();
                    _remainingResets--;
                }
            }
            else if (Input.IsActionPressed("soft_drop") && _canFall)
            {
                if (Position.Y < _maxY)
                {
                    Position += new Vector2(0, GlobalVariables.PiecePartSize);
                }
            }
            GetTree().CreateTimer(TimeBetweenInputs).Timeout += () => _input = true;
            _input = false;
        }

        _canFall = CheckMovementY();
    }

    public void GeneratePiece(PieceShape shape)
    {
        _canFall = true;
        _parts.Clear();
        _shape = shape;
        GenerateParts();
        DrawPiece();
        UpdateBounds();
    }

    private void Drop()
    {
        if (_canFall)
        {
            Position += new Vector2(0, GlobalVariables.PiecePartSize);
            _dropTimer.Start(_maxTimeInRow);
        }
    }

    private void LockPiece()
    {
        EmitSignal(SignalName.PieceLocked, _parts);
        _canFall = true;
        _remainingResets = MaxResets;
    }

    private void DrawPiece()
    {
        int part = 0;
        for (int i = 0; i < _shape.Shape.Count; i++)
        {
            for (int j = 0; j < _shape.Shape[i].Count; j++)
            {
                if (_shape.Shape[i][j])
                {
                    Node2D piecePart = _parts[part];
                    GD.Print(piecePart.Name);
                    piecePart.Position = new Vector2(i * GlobalVariables.PiecePartSize, j * GlobalVariables.PiecePartSize);
                    part++;
                }
            }
        }
    }

    private void GenerateParts()
    {
        for (int i = 0; i < _shape.Shape.Count; i++)
        {
            for (int j = 0; j < _shape.Shape[i].Count; j++)
            {
                if (_shape.Shape[i][j])
                {
                    Node2D piecePart = (Node2D)_piecePart.Instantiate();
                    piecePart.Modulate = _shape.Color;
                    AddChild(piecePart);
                    _parts.Add(piecePart);
                }
            }
        }
    }

    private double CalculateGravity()
    {
        return Math.Pow(0.8 - ((_level - 1) * 0.007), _level - 1);
    }

    private bool CheckMovementY()
    {
        if (_canFall && Position.Y >= _maxY)
        {
            if (_lockTimer.TimeLeft == 0)
            {
                _lockTimer.Start();
            }

            _canFall = false;
            return false;
        }

        foreach (var piece in _parts)
        {
            Vector2I mapPosition = _board.LocalToMap(_board.ToLocal(piece.GlobalPosition));

            if (mapPosition.Y + 1 < 0)
            {
                continue;
            }

            if (mapPosition.Y >= _maxY)
            {
                return false;
            }

            if (_boardSquares[mapPosition.X, mapPosition.Y + 1])
            {
                if (_lockTimer.TimeLeft == 0)
                {
                    _lockTimer.Start();
                }

                return false;
            }
        }

        if (_dropTimer.TimeLeft == 0)
        {
            _dropTimer.Start(_maxTimeInRow);
        }

        if (_lockTimer.TimeLeft > 0)
        {
            _lockTimer.Stop();
        }

        return true;
    }

    private bool CheckMovementX(int direction)
    {
        if ((Position.X - GlobalVariables.PiecePartSize < 0 && direction == 1) || (Position.X + GlobalVariables.PiecePartSize > _maxX && direction == -1))
        {
            return false;
        }

        foreach (var piece in _parts)
        {
            Vector2I mapPosition = _board.LocalToMap(_board.ToLocal(piece.GlobalPosition));

            if (mapPosition.X + direction < 0 || mapPosition.X + direction >= GlobalVariables.BoardWidth || mapPosition.Y < 0)
            {
                continue;
            }

            if (_boardSquares[mapPosition.X + direction, mapPosition.Y])
            {
                return false;
            }
        }

        return true;
    }

    private void CorrectPosition()
    {
        if (Position.X < 0)
        {
            Position = new Vector2(0, Position.Y);
            GD.Print($"Corrected position to ${Position}");
        }

        if (Position.X > _maxX)
        {
            Position = new Vector2(_maxX, Position.Y);
            GD.Print($"Corrected position to ${Position}");
        }

        if (Position.Y > _maxY)
        {
            Position = new Vector2(Position.X, _maxY);
            GD.Print($"Corrected position to ${Position}");
        }
    }

    private void UpdateBounds()
    {
        Vector2 maxPosition = _board.MapToLocal(new Vector2I(GlobalVariables.BoardWidth - _shape.Shape.Count, GlobalVariables.BoardHeigth - _shape.Shape[0].Count));

        _maxX = maxPosition.X;
        _maxY = maxPosition.Y;
    }
}
