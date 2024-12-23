using Godot;
using Godot.Collections;
using System;

namespace Stacker.Scripts.CustomResources;
[GlobalClass]
public partial class PieceShape : Resource
{
    private Array<Array<bool>> _shape;

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

    public PieceShape(Array<Array<bool>> shape)
    {
        Shape = shape;
    }

    public PieceShape() : this(new Array<Array<bool>>())
    {
    }

    private bool CheckShape(Array<Array<bool>> shape)
    {
        if (shape.Count > 4)
        {
            return false;
        }

        foreach (var item in shape)
        {
            if (item.Count > 4)
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
