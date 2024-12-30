using Godot;
using Stacker.Scripts.Autoloads;
using Stacker.Scripts.CustomResources;
using System;
using System.Collections.Generic;

namespace Stacker.Scripts;
public partial class CurrentPiece : Node2D
{
    private const double TimeBetweenInputs = 0.05;

	[Export]
	private PieceShape _shape;

	[Export]
	private PackedScene _piecePart;

	[Export]
	private Color _color;

    private List<Node2D> _parts;

    private double _timeSinceLastInput;

    public PieceShape Shape { get => _shape; }

	public override void _Ready()
	{
        _timeSinceLastInput = 0.2;
        _parts = new List<Node2D>();
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
            _timeSinceLastInput = 0;
        }

        _timeSinceLastInput += delta;
    }

    public void GeneratePiece(PieceShape shape, Color color)
    {
        _shape = shape;
        _color = color;
        GenerateParts();
        DrawPiece();
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
}
