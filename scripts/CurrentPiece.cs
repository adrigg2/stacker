using Godot;
using Stacker.Scripts.Autoloads;
using Stacker.Scripts.CustomResources;
using System;

namespace Stacker.Scripts;
public partial class CurrentPiece : StaticBody2D
{
	[Export]
	private PieceShape _shape;

	[Export]
	private PackedScene _piecePart;

	[Export]
	private Color _color;

	public override void _Ready()
	{
		for (int i = 0; i < _shape.Shape.Count; i++)
		{
			for (int j = 0; j < _shape.Shape[i].Count; j++)
			{
				if (_shape.Shape[i][j])
				{
					Node2D piecePart = (Node2D)_piecePart.Instantiate();
					piecePart.Position = new Vector2(i * GlobalVariables.piecePartSize, j * GlobalVariables.piecePartSize);
					piecePart.Modulate = _color;
					AddChild(piecePart);
				}
			}
		}
	}

	public override void _Process(double delta)
	{
	}
}
