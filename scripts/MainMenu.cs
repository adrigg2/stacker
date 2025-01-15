using Godot;
using System;

namespace Stacker.Scripts;
public partial class MainMenu : Control
{
	[Export]
	private Button _startButton;

	[Export]
    private Button _exitButton;

	[Export]
    private Button _settingsButton;

    [Export]
    private Button _customizeButton;

    public override void _Ready()
	{
        _startButton.Pressed += () => GetTree().ChangeSceneToFile("res://scenes/game.tscn");
        _exitButton.Pressed += () => GetTree().Quit();
        _settingsButton.Pressed += () => GD.Print("Settings button pressed");
        _customizeButton.Pressed += () => GD.Print("Customize button pressed");
    }
}
