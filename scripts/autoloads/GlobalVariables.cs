using Godot;
using System;

namespace Stacker.Scripts.Autoloads;
public static class GlobalVariables
{
	public const int PiecePartSize = 32;
	public const int BoardWidth = 10;
	public const int BoardHeigth = 20;
	public const int PoolSize = 7;

	public static int Level { get; set; } = 1;
	public static int Points { get; set; } = 0;
}
