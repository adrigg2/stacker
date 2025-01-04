using Godot;
using Godot.Collections;
using Stacker.Scripts.Autoloads;
using Stacker.Scripts.CustomResources;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Stacker.Scripts;
public partial class Game : Node
{
    private const int PoolSize = 5;

    [Export]
    private TileMapLayer _board;

    [Export]
    private PieceShape[] _pieceShapes;

    [Export]
    private CurrentPiece _currentPiece;

    [Export]
    private SubViewport _heldPieceViewport;

    [Export]
    private SubViewport _poolViewport;

    [Export]
    private PackedScene _piecePart;

    private int[] _currentPool;
    private int[] _nextPool;
    private int _currentPoolIndex;

    private System.Collections.Generic.Dictionary<Vector2I, Node2D> _parts;

    private bool[,] _boardSquares;
    private bool _canHold;

    private PieceShape _heldPiece;

    public override void _Ready()
    {
        _canHold = true;

        _parts = new System.Collections.Generic.Dictionary<Vector2I, Node2D>();

        _currentPiece.PieceLocked += OnPieceLocked;

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

        _boardSquares = new bool[GlobalVariables.BoardWidth, GlobalVariables.BoardHeigth];

        _currentPiece.BoardSquares = _boardSquares;
        _currentPiece.Board = _board;

        GenerateNextPool();
        _currentPool = _nextPool;
        GenerateNextPool();
        _currentPoolIndex = 0;

        PlaceNextPiece();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("hold") && _canHold)
        {
            if (_heldPiece == null)
            {
                _heldPiece = _currentPiece.Shape;
                _currentPiece.Hold();
                PlaceNextPiece();
            }
            else
            {
                _currentPiece.Hold();
                PieceShape newHold = _currentPiece.Shape;
                PlaceHeldPiece();
                _heldPiece = newHold;
            }

            foreach (var child in _heldPieceViewport.GetChildren())
            {
                if (child is not Camera2D)
                {
                    child.QueueFree();
                }
            }
            DrawPiece(_heldPiece, _heldPieceViewport, new Vector2(0, 0), new Vector2(1, 1));
            _canHold = false;
        }
    }

    private void PlaceNextPiece()
    {
        if (_currentPoolIndex == _currentPool.Length)
        {
            _currentPool = _nextPool;
            _currentPoolIndex = 0;
            GenerateNextPool();
        }

        _currentPiece.GeneratePiece(_pieceShapes[_currentPool[_currentPoolIndex]]);
        int positionX = (GlobalVariables.BoardWidth - _currentPiece.Shape.Shape.Count) / 2;
        int positionY = -_currentPiece.Shape.Shape[0].Count;

        Vector2 startingPosition = _board.MapToLocal(new Vector2I(positionX, positionY));
        _currentPiece.Position = startingPosition;
        _currentPoolIndex++;
        DrawPool();
        _canHold = true;
    }

    private void PlaceHeldPiece()
    {
        _currentPiece.GeneratePiece(_heldPiece);
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

    private void OnPieceLocked(Array<Node2D> parts)
    {
        foreach (var part in parts)
        {
            Vector2 position = part.GlobalPosition;
            part.GetParent()?.RemoveChild(part);
            AddChild(part);
            part.GlobalPosition = position;

            Vector2I mapPosition = _board.LocalToMap(_board.ToLocal(position));
            _boardSquares[mapPosition.X, mapPosition.Y] = true;
            _parts.Add(mapPosition, part);
        }

        _currentPiece.BoardSquares = _boardSquares;
        CheckClear();
        PlaceNextPiece();
    }

    private void CheckClear()
    {
        List<int> rows = new();

        for (int i = 0; i < _boardSquares.GetLength(1); i++)
        {
            bool cleared = true;
            for (int j = 0; j <  _boardSquares.GetLength(0); j++)
            {
                if (!_boardSquares[j, i])
                {
                    cleared = false; 
                    break;
                }
            }

            if (cleared)
            {
                rows.Add(i);
            }
        }

        if (rows.Count > 0)
        {
            ClearRows(rows);
        }
    }

    private void ClearRows(List<int> rows)
    {
        foreach (int row in rows)
        {
            for (int i = 0; i < GlobalVariables.BoardWidth; i++)
            {
                _parts[new Vector2I(i, row)].QueueFree();
                _parts.Remove(new Vector2I(i, row));
                _boardSquares[i, row] = false;
            }
        }

        List<Node2D> parts = _parts.Values.ToList();
        parts = parts.OrderByDescending(i => i.Position.Y).ToList();
        foreach (var part in parts)
        {
            Vector2I iPos = _board.LocalToMap(_board.ToLocal(part.GlobalPosition));

            int rowsToLower = 0;
            foreach (int row in rows)
            {
                if (row > iPos.Y)
                {
                    rowsToLower++;
                }
            }

            Vector2I fPos = new Vector2I (iPos.X, iPos.Y + rowsToLower);
            if (iPos != fPos)
            {
                MovePart(iPos, fPos);
            }
        }

        _currentPiece.BoardSquares = _boardSquares;
    }

    private void MovePart(Vector2I iPos, Vector2I fPos)
    {
        Node2D part = _parts[iPos];
        _parts.Remove(iPos);
        _boardSquares[iPos.X, iPos.Y] = false;

        part.GlobalPosition = _board.ToGlobal(_board.MapToLocal(fPos));

        _parts.Add(fPos, part);
        _boardSquares[fPos.X, fPos.Y] = true;
    }

    private void DrawPool()
    {
        foreach (var child in _poolViewport.GetChildren())
        {
            if (child is not Camera2D)
            {
                child.QueueFree();
            }
        }

        Vector2 position = new(-50, -350);
        for (int i = _currentPoolIndex; i < PoolSize + _currentPoolIndex; i++)
        {
            if (i >= _currentPool.Length)
            {
                DrawPiece(_pieceShapes[_nextPool[i % _nextPool.Length]], _poolViewport, position, new Vector2(.5f, .5f));
            }
            else
            {
                DrawPiece(_pieceShapes[_currentPool[i]], _poolViewport, position, new Vector2(1, 1));
            }

            position += new Vector2(0, 128);
        }
    }

    private void DrawPiece(PieceShape shape, SubViewport viewport, Vector2 position, Vector2 scale)
    {
        for (int i = 0; i < shape.Shape.Count; i++)
        {
            for (int j = 0; j < shape.Shape[i].Count; j++)
            {
                if (shape.Shape[i][j])
                {
                    Node2D piecePart = (Node2D)_piecePart.Instantiate();
                    piecePart.Position = new Vector2(i * GlobalVariables.PiecePartSize * scale.X, j * GlobalVariables.PiecePartSize * scale.Y) + position;
                    piecePart.Scale = scale;
                    piecePart.Modulate = shape.Color;
                    viewport.AddChild(piecePart);
                }
            }
        }
    }
}
