using Godot;
using System;

public partial class MainMenu : Control
{
	[Export]
	private Button _startButton;

	[Export]
    private Button _exitButton;

	[Export]
    private Button _settingsButton;

    [Export]
    private PackedScene _game;

    public override void _Ready()
	{
        // Connect the button signals
        _startButton.Pressed += OnStartButtonPressed;
        _exitButton.Pressed += OnExitButtonPressed;
        _settingsButton.Pressed += OnSettingsButtonPressed;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}

    private void OnStartButtonPressed()
    {
        GetTree().ChangeSceneToPacked(_game);
    }

    private void OnExitButtonPressed()
    {
        GetTree().Quit();
    }

    private void OnSettingsButtonPressed()
    {
        GD.Print("Settings button pressed");
    }
}
