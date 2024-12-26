using Godot;
using Stacker.Scripts.Autoloads;
using Stacker.Scripts.CustomResources;
using System;
using System.Collections.Generic;

namespace Stacker.Scripts;
public partial class CurrentPiece : Node2D
{
	[Export]
	private PieceShape _shape;

	[Export]
	private PackedScene _piecePart;

	[Export]
	private Color _color;

    private List<Node2D> _parts;

	public override void _Ready()
	{
        _parts = new List<Node2D>();
        GenerateParts();

        DrawPiece();
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
                    piecePart.Position = new Vector2(i * GlobalVariables.piecePartSize, j * GlobalVariables.piecePartSize);
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
