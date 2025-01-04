using Godot;
using Godot.Collections;
using Stacker.Scripts.Autoloads;
using Stacker.Scripts.CustomResources;

namespace Stacker.Scripts;
public partial class PieceGuide : Node2D
{
    [Export]
    private PackedScene _piecePart;

    private PieceShape _shape;

    private Array<Node2D> _parts;

    private TileMapLayer _board;

    private bool[,] _boardSquares;

    private float _maxY;

    public TileMapLayer Board { get => _board; set => _board = value; }
    public bool[,] BoardSquares { get => _boardSquares; set => _boardSquares = value; }

    public PieceShape Shape 
    { 
        get => _shape; 
        set 
        {
            _shape = value;
            ClearParts();
            GenerateParts();
        } 
    }

    public override void _Ready()
    {
        _parts = new Array<Node2D>();
    }

    public void DrawPiece()
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

        UpdateBounds();
    }

    public void Fall()
    {
        bool canFall = true;
        while (canFall)
        {
            Position += new Vector2(0, GlobalVariables.PiecePartSize);
            canFall = CheckMovementY();
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
                    AddChild(piecePart);
                    _parts.Add(piecePart);
                }
            }
        }
    }

    private void ClearParts()
    {
        foreach (var part in _parts)
        {
            part.QueueFree();
        }

        _parts.Clear();
    }

    private bool CheckMovementY()
    {
        if (Position.Y >= _maxY)
        {
            return false;
        }

        foreach (var piece in _parts)
        {
            Vector2I mapPosition = _board.LocalToMap(_board.ToLocal(piece.GlobalPosition));

            if (mapPosition.Y + 1 < 0 || mapPosition.Y + 1 >= _maxY)
            {
                continue;
            }

            if (Position.Y >= _maxY)
            {
                return false;
            }

            if (_boardSquares[mapPosition.X, mapPosition.Y + 1])
            {
                return false;
            }
        }
        return true;
    }

    private void UpdateBounds()
    {
        Vector2 maxPosition = _board.MapToLocal(new Vector2I(0, GlobalVariables.BoardHeigth - _shape.Shape[0].Count));

        _maxY = maxPosition.Y;
    }
}
