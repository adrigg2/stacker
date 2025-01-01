using Godot;
using Godot.Collections;
using System;

namespace Stacker.Scripts.CustomResources;
[GlobalClass]
public partial class PieceShape : Resource
{
    private Array<Array<bool>> _shape;

    [Export]
    private Color _color;

    [Export]
    public Array<Array<bool>> Shape
    {
        get => _shape;

        set
        {
            if (CheckShape(value))
            {
                _shape = value;
            }
            else
            {
                throw new ArgumentException("Shape must be, at most 4 x 4 and contiguous");
            }
        }
    }

    public Color Color => _color;

    public PieceShape(Array<Array<bool>> shape)
    {
        Shape = shape;
    }

    public PieceShape() : this(new Array<Array<bool>>())
    {
    }

    public void RotateClockwise()
    {
        Array<Array<bool>> newShape = CreateArray(Shape[0].Count, Shape.Count);

        for (int i = 0; i < newShape.Count; i++)
        {
            for (int j = 0; j < newShape[i].Count; j++)
            {
                newShape[i][j] = Shape[Shape.Count - j - 1][i];
            }
        }

        Shape = newShape;
    }

    public void RotateCounterClockwise()
    {
        Array<Array<bool>> newShape = CreateArray(Shape[0].Count, Shape.Count);

        for (int i = 0; i < newShape.Count; i++)
        {
            for (int j = 0; j < newShape[i].Count; j++)
            {
                newShape[i][j] = Shape[j][Shape[0].Count - i - 1];
            }            
        }

        Shape = newShape;
    }

    private static Array<Array<bool>> CreateArray(int countX, int countY)
    {
        Array<Array<bool>> array = new();

        for (int i = 0; i < countX; i++)
        {
            Array<bool> row = new();
            row.Resize(countY);
            row.Fill(false);
            array.Add(row);
        }

        return array;
    }

    private static bool CheckShape(Array<Array<bool>> shape)
    {
        if (shape.Count == 0) 
        {
            return true;
        }

        if (shape.Count > 4)
        {
            return false;
        }

        int itemCount = shape[0].Count;
        foreach (var item in shape)
        {
            if (item.Count > 4 || item.Count != itemCount)
            {
                return false;
            }
        }


        int[][] directions = new int[][]
        { 
            new int[] { 1, 0 },
            new int[] { 1, 1 },
            new int[] { 0, 1 },
            new int[] { -1, 1 },
            new int[] { -1, 0 },
            new int[] { -1, -1 },
            new int[] { 0, -1 },
            new int[] { 1, -1 },
        };

        int cellNumber = 0;

        for (int i = 0; i < shape.Count; i++)
        {
            for (int j = 0; j < shape[i].Count; j++)
            {
                if (!shape[i][j])
                {
                    continue;
                }
                
                cellNumber++;

                bool hasNeighbours = false;
                foreach (int[] direction in directions)
                {
                    if (i + direction[0] < 0 ||  j + direction[1] < 0 || i + direction[0] >= shape.Count || j + direction[1] >= shape[i + direction[0]].Count)
                    {
                        continue;
                    }

                    if (shape[i + direction[0]][j + direction[1]])
                    {
                        hasNeighbours = true;
                        break;
                    }
                }
                if (!hasNeighbours && cellNumber > 1)
                {
                    return false;
                }
            }
        }

        return true;
    }
}
