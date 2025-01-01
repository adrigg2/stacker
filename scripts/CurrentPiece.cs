using Godot;
using Godot.Collections;
using Stacker.Scripts.Autoloads;
using Stacker.Scripts.CustomResources;
using System;
using System.Collections.Generic;

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

	[Export]
	private Color _color;

    private Array<Node2D> _parts;

    private double _timeSinceLastInput;
    private double _timeInRow;
    private double _maxTimeInRow;
    private int _level;
    private int _remainingResets;

    private bool _canFall;

    public PieceShape Shape { get => _shape; }

	public override void _Ready()
	{
        _timeSinceLastInput = 0.2;
        _timeInRow = 0;
        _level = 1;
        _remainingResets = MaxResets;
        _maxTimeInRow = CalculateGravity();
        _parts = new Array<Node2D>();
        _canFall = true;
    }

    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if (@event.IsActionPressed("rotate_counterclock"))
        {
            _shape.RotateClockwise();
            DrawPiece();
        }
        else if (@event.IsActionPressed("rotate_clock"))
        {
            _shape.RotateCounterClockwise();
            DrawPiece();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_timeSinceLastInput >= TimeBetweenInputs)
        {
            if (Input.IsActionPressed("move_left"))
            {
                if (Position.X - GlobalVariables.PiecePartSize >= 0)
                {
                    Position -= new Vector2(GlobalVariables.PiecePartSize, 0);
                }
            }
            else if (Input.IsActionPressed("move_right"))
            {
                if (Position.X + GlobalVariables.PiecePartSize <= GlobalVariables.BoardWidth * GlobalVariables.PiecePartSize - (_shape.Shape.Count - 1) * GlobalVariables.PiecePartSize)
                {
                    Position += new Vector2(GlobalVariables.PiecePartSize, 0);
                }
            }
            else if (Input.IsActionPressed("soft_drop"))
            {
                if (Position.Y < GlobalVariables.BoardHeigth * GlobalVariables.PiecePartSize - _shape.Shape[0].Count * GlobalVariables.PiecePartSize)
                {
                    GD.Print("Soft drop");
                    Position += new Vector2(0, GlobalVariables.PiecePartSize);
                }
            }
            _timeSinceLastInput = 0;
        }

        _timeSinceLastInput += delta;
        _timeInRow += delta;
        if (_timeInRow >= _maxTimeInRow && _canFall)
        {
            Position += new Vector2(0, GlobalVariables.PiecePartSize);
            _timeInRow = 0;
        }

        if (_canFall && Position.Y >= GlobalVariables.BoardHeigth * GlobalVariables.PiecePartSize - _shape.Shape[0].Count * GlobalVariables.PiecePartSize)
        {
            _canFall = false;
            GetTree().CreateTimer(LockDelay).Timeout += LockPiece;
        }
    }

    public void GeneratePiece(PieceShape shape, Color color)
    {
        _parts.Clear();
        _shape = shape;
        _color = color;
        GenerateParts();
        DrawPiece();
    }

    private void LockPiece()
    {
        EmitSignal(SignalName.PieceLocked, _parts);
        _canFall = true;
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
                    piecePart.Modulate = _color;
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
}
