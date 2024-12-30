using Godot;
using Stacker.Scripts;
using Stacker.Scripts.Autoloads;
using Stacker.Scripts.CustomResources;
using System;
using System.Linq;

namespace Stacker.Scripts;
public partial class Game : Node2D
{
    [Export]
    private TileMapLayer _board;

    // TODO: Find better solution to this
    [Export]
    private Color[] _colors;

    [Export]
    private PieceShape[] _pieceShapes;

    [Export]
    private CurrentPiece _currentPiece;

    private int[] _currentPool;
    private int[] _nextPool;
    private int _currentPoolIndex;

    public override void _Ready()
    {
        for (int i = 0; i < GlobalVariables.BoardWidth; i++)
        {
            for (int j = 0; j < GlobalVariables.BoardHeigth; j++)
            {
                if ((i % 2 == 0 && j % 2 == 0) || (i % 2 != 0 && j % 2 != 0))
                {
                    _board.SetCell(new Vector2I(i, j), sourceId: 0, atlasCoords: new Vector2I(0, 0));
                }
                else
                {
                    _board.SetCell(new Vector2I(i, j), sourceId: 0, atlasCoords: new Vector2I(0, 1));
                }
            }
        }

        GenerateNextPool();
        _currentPool = _nextPool;
        GenerateNextPool();
        _currentPoolIndex = 0;

        PlaceNextPiece();
    }

    private void PlaceNextPiece()
    {
        if (_currentPoolIndex == _currentPool.Length)
        {
            _currentPool = _nextPool;
            _currentPoolIndex = 0;
            GenerateNextPool();
        }

        _currentPiece.GeneratePiece(_pieceShapes[_currentPool[_currentPoolIndex]], _colors[_currentPool[_currentPoolIndex]]);
        int positionX = (GlobalVariables.BoardWidth - _currentPiece.Shape.Shape.Count) / 2;
        int positionY = -_currentPiece.Shape.Shape[0].Count;

        Vector2 startingPosition = _board.MapToLocal(new Vector2I(positionX, positionY));
        _currentPiece.Position = startingPosition;
    }

    private void GenerateNextPool()
    {
        Random random = new();

        _nextPool = new int[GlobalVariables.PoolSize];
        int generatedPool = 0;
        while (generatedPool < _nextPool.Length - 1)
        {
            int possibleValue = random.Next(0, 7);
            if (!_nextPool.Contains(possibleValue))
            {
                _nextPool[generatedPool++] = possibleValue;
            }
        }
    }
}
