using Godot;
using Stacker.Scripts.CustomResources;
using System;

namespace Stacker.Scripts;
public partial class CurrentPiece : StaticBody2D
{
	[Export]
	private PieceShape _shape;

	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}
}
