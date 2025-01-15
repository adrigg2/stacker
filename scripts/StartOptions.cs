using Godot;
using System;

public partial class StartOptions : Panel
{
	[ExportGroup("Buttons")]
	[Export]
	private Button _normalGame;

	[Export] 
	private Button _40Lines;

	[Export]
	private Button _blitz;

	[Export]
    private Button _customGame;

	[Export]
	private Button _mp;

	[Export]
	private Button _menu;

	[ExportGroup("Panels")]
	[Export]
	private Panel _mainMenu;

	[Export]
	private Panel _startPanel;

    public override void _Ready()
	{
        _normalGame.Pressed += () => GetTree().ChangeSceneToFile("res://scenes/game.tscn");
        _40Lines.Pressed += () => GD.Print("40 lines button pressed");
        _blitz.Pressed += () => GD.Print("Blitz button pressed");
        _customGame.Pressed += () => GD.Print("Custom game button pressed");
        _mp.Pressed += () => GD.Print("Multiplayer button pressed");
        _menu.Pressed += () =>
        {
            _mainMenu.Visible = true;
            _startPanel.Visible = false;
        };
    }
}
