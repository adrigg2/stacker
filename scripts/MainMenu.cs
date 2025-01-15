using Godot;
using System;

namespace Stacker.Scripts;
public partial class MainMenu : Panel
{
    [ExportGroup("Buttons")]
	[Export]
	private Button _startButton;

	[Export]
    private Button _exitButton;

	[Export]
    private Button _settingsButton;

    [Export]
    private Button _customizeButton;

    [ExportGroup("Panels")]
    [Export]
    private Panel _mainPanel;

    [Export]
    private Panel _startOptions;

    [Export]
    private Panel _optionsPanel;

    public override void _Ready()
	{
        _startButton.Pressed += () =>
        {
            _startOptions.Visible = true;
            _mainPanel.Visible = false;
        };
        _exitButton.Pressed += () => GetTree().Quit();
        _settingsButton.Pressed += () => GD.Print("Settings button pressed");
        _customizeButton.Pressed += () => GD.Print("Customize button pressed");
    }
}
