using Godot;
using Godot.Collections;
using Stacker.Scripts.Autoloads;
using Stacker.Scripts.CustomResources;
using System;

namespace Stacker.Scripts;
public partial class CurrentPiece : Node2D
{
    [Signal]
    public delegate void PieceLockedEventHandler(Array<Node2D> parts);

    private const double TimeBetweenInputs = 0.05;
    private const double LockDelay = 0.5;
    private const int MaxResets = 15;

    [Export]
    private PieceShape _defaultShape;
    private PieceShape _currentShape;

    [Export]
    private PackedScene _piecePart;

    [Export]
    private PieceGuide _pieceGuide;

    private Array<Node2D> _parts;

    private double _maxTimeInRow;
    private int _remainingResets;
    private float _maxX;
    private float _maxY;

    private bool _canFall;
    private bool _input;
    private System.Collections.Generic.Dictionary<Vector2I, Node2D> _boardSquares;

    private Timer _lockTimer;
    private Timer _dropTimer;

    private TileMapLayer _board;

    public PieceShape Shape { get => _defaultShape; }

    public System.Collections.Generic.Dictionary<Vector2I, Node2D> BoardSquares 
    { 
        get => _boardSquares;
        set
        {
            _boardSquares = value;
            _pieceGuide.BoardSquares = value;
        }
    }

    public TileMapLayer Board 
    { 
        get => _board; 
        set {
            _board = value;
            _pieceGuide.Board = value;
        }
    }

    public override void _Ready()
    {
        _input = true;
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
            _currentShape.RotateClockwise();

            if (_lockTimer.TimeLeft > 0 && _remainingResets > 0)
            {
                _lockTimer.Start();
                _remainingResets--;
            }

            DrawPiece();
            UpdateBounds();
            CorrectPosition();

            UpdatePieceGuide();
        }
        else if (@event.IsActionPressed("rotate_clock"))
        {
            _currentShape.RotateCounterClockwise();

            if (_lockTimer.TimeLeft > 0 && _remainingResets > 0)
            {
                _lockTimer.Start();
                _remainingResets--;
            }

            DrawPiece();
            UpdateBounds();
            CorrectPosition();

            UpdatePieceGuide();
        }
        else if (@event.IsActionPressed("rotate_180"))
        {
            _currentShape.RotateClockwise();
            _currentShape.RotateClockwise();

            if (_lockTimer.TimeLeft > 0 && _remainingResets > 0)
            {
                _lockTimer.Start();
                _remainingResets--;
            }

            DrawPiece();
            UpdateBounds();
            CorrectPosition();

            UpdatePieceGuide();
        }
        else if (@event.IsActionPressed("hard_drop"))
        {
            int initialCell = _currentShape.Shape[0].Count + (int)(Position.Y / GlobalVariables.PiecePartSize);
            int finalCell = _currentShape.Shape[0].Count + (int)(_pieceGuide.Position.Y / GlobalVariables.PiecePartSize);

            GlobalVariables.Points += (finalCell - initialCell) * 2;

            Position = _pieceGuide.Position;
            LockPiece();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_input)
        {
            if (Input.IsActionPressed("move_left") && CheckMovementX(-1))
            {
                Position -= new Vector2(GlobalVariables.PiecePartSize, 0);
                if (_lockTimer.TimeLeft > 0 && _remainingResets > 0)
                {
                    _lockTimer.Start();
                    _remainingResets--;
                }

                UpdatePieceGuide();
            }
            else if (Input.IsActionPressed("move_right") && CheckMovementX(1))
            {
                Position += new Vector2(GlobalVariables.PiecePartSize, 0);
                if (_lockTimer.TimeLeft > 0 && _remainingResets > 0)
                {
                    _lockTimer.Start();
                    _remainingResets--;
                }

                UpdatePieceGuide();
            }
            else if (Input.IsActionPressed("soft_drop") && _canFall)
            {
                if (Position.Y < _maxY)
                {
                    Position += new Vector2(0, GlobalVariables.PiecePartSize);
                    GlobalVariables.Points++;
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
        _defaultShape = shape;
        _currentShape = (PieceShape)shape.Clone();
        GenerateParts();
        DrawPiece();
        UpdateBounds();
    }

    public void Hold()
    {
        foreach (var piece in _parts)
        {
            piece.QueueFree();
        }
        _parts.Clear();
    }

    public void UpdatePieceGuide()
    {
        GD.Print("update guide");
        _pieceGuide.Position = Position;
        _pieceGuide.Shape = _currentShape;
        _pieceGuide.DrawPiece();
        _pieceGuide.Fall();
    }

    public void LevelUp()
    {
        _maxTimeInRow = CalculateGravity();
        GD.PrintRich("[color=green]Level up[/color]");
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
        for (int i = 0; i < _currentShape.Shape.Count; i++)
        {
            for (int j = 0; j < _currentShape.Shape[i].Count; j++)
            {
                if (_currentShape.Shape[i][j])
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
        for (int i = 0; i < _currentShape.Shape.Count; i++)
        {
            for (int j = 0; j < _currentShape.Shape[i].Count; j++)
            {
                if (_currentShape.Shape[i][j])
                {
                    Node2D piecePart = (Node2D)_piecePart.Instantiate();
                    piecePart.Modulate = _currentShape.Color;
                    AddChild(piecePart);
                    _parts.Add(piecePart);
                }
            }
        }
    }

    private static double CalculateGravity()
    {
        return Math.Pow(0.8 - ((GlobalVariables.Level - 1) * 0.007), GlobalVariables.Level - 1);
    }

    private bool CheckMovementY()
    {
        if (_canFall && Position.Y >= _maxY)
        {
            if (_lockTimer.TimeLeft == 0)
            {
                _lockTimer.Start();
            }

            return false;
        }

        foreach (var piece in _parts)
        {
            Vector2I mapPosition = _board.LocalToMap(_board.ToLocal(piece.GlobalPosition));

            if (mapPosition.Y + 1 < 0 || mapPosition.Y + 1 >= _maxY)
            {
                GD.PrintRich("[color=red] skipping check [/color]");
                continue;
            }

            if (Position.Y >= _maxY)
            {
                GD.PrintRich("[color=yellow] found bottom [/color]");

                if (_lockTimer.TimeLeft == 0)
                {
                    _lockTimer.Start();
                }

                return false;
            }

            if (_boardSquares.ContainsKey(mapPosition + new Vector2I(0, 1)))
            {
                GD.PrintRich("[color=yellow] found another piece [/color]");

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

        GD.PrintRich("[color=green] can fall [/color]");
        return true;
    }

    private bool CheckMovementX(int direction)
    {
        if ((Position.X - GlobalVariables.PiecePartSize < 0 && direction == -1) || (Position.X + GlobalVariables.PiecePartSize > _maxX && direction == 1))
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

            if (_boardSquares.ContainsKey(mapPosition + new Vector2I(direction, 0)))
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
            GD.Print($"Corrected position to {Position}");
        }

        if (Position.X > _maxX)
        {
            Position = new Vector2(_maxX, Position.Y);
            GD.Print($"Corrected position to {Position}");
        }

        if (Position.Y > _maxY)
        {
            Position = new Vector2(Position.X, _maxY);
            GD.Print($"Corrected position to {Position}");
        }

        Position += CalculateShortestMovement();
    }

    private Vector2 CalculateShortestMovement()
    {
        Vector2[] movements = new Vector2[4];

        Vector2[] directions = new Vector2[] { new(GlobalVariables.PiecePartSize, 0), new(-GlobalVariables.PiecePartSize, 0), new(0, -GlobalVariables.PiecePartSize), new(0, GlobalVariables.PiecePartSize) };

        for (int i = 0; i < 3; i++)
        {
            Vector2 position = Position;
            bool overlapping = true;

            while (overlapping)
            {
                int overlaps = 0;

                foreach (var piece in _parts)
                {
                    Vector2I mapPosition = _board.LocalToMap(_board.ToLocal(piece.GlobalPosition));

                    if (mapPosition.Y < 0)
                    {
                        continue;
                    }

                    if (_boardSquares.ContainsKey(mapPosition))
                    {
                        overlaps++;
                    }
                }

                if (overlaps > 0)
                {
                    Position += directions[i];
                    movements[i] += directions[i];

                    if (Position.X < 0 || Position.X > _maxX)
                    {
                        overlaps = 0;
                        movements[i] = new Vector2(int.MaxValue, int.MaxValue);
                    }
                }

                overlapping = overlaps > 0;
            }

            Position = position;
        }

        if (movements[0].Length() < movements[1].Length() && movements[0].Length() < movements[2].Length())
        {
            return movements[0];
        }
        else if (movements[1].Length() < movements[0].Length() && movements[1].Length() < movements[2].Length())
        {
            return movements[1];
        }
        else
        {
            return movements[2];
        }
    }

    private void UpdateBounds()
    {
        Vector2 maxPosition = _board.MapToLocal(new Vector2I(GlobalVariables.BoardWidth - _currentShape.Shape.Count, GlobalVariables.BoardHeigth - _currentShape.Shape[0].Count));

        _maxX = maxPosition.X;
        _maxY = maxPosition.Y;
    }
}
