using System.Collections.Generic;
using UnityEngine;

public class MatchFinder : Singleton<MatchFinder>
{
    public List<Cell> FindMatchList(Cell cell)
    {
        Vector2Int gridSize = GridController.Instance.gridSize;
        bool[,] visitedCells = new bool[gridSize.x, gridSize.y];
        List<Cell> resultCells = new List<Cell>();
        List<List<Cell>> directionalMatches = new List<List<Cell>>() { new List<Cell>(),new List<Cell>()};
        FindMatches(cell, cell, Direction.right, directionalMatches, visitedCells);

        foreach(List<Cell> matchList in directionalMatches)
        {
            if (matchList.Count > 1)
                resultCells.AddRange(matchList);
        }
        resultCells.Add(cell);

        return resultCells;
    }

    void FindMatches(Cell newCell, Cell selectedCell, Direction dir, List<List<Cell>> directionalMatches, bool[,] visitedCells)
    {
        if (newCell == null)
            return;

        Vector2Int gridPos = newCell.gridPosition;

        if (visitedCells[gridPos.x, gridPos.y])
            return;

        visitedCells[gridPos.x, gridPos.y] = true;

        if (newCell is IMatchable matchable && matchable.CheckMatch(selectedCell.GetCellId()))
        {
            if(newCell == selectedCell)
            {
                for (int i = 0; i < 4; i++)
                {
                    dir = (Direction)i;
                    Cell neighbour = GridController.Instance.GetNeighbourByDirection(newCell, dir);
                    if (neighbour != null)
                        FindMatches(neighbour, selectedCell, dir, directionalMatches, visitedCells);
                }
            }
            else
            {
                if (dir == Direction.left || dir == Direction.right)
                    directionalMatches[0].Add(newCell);
                else
                    directionalMatches[1].Add(newCell);

                Cell neighbour = GridController.Instance.GetNeighbourByDirection(newCell, dir);
                if (neighbour != null)
                    FindMatches(neighbour, selectedCell, dir, directionalMatches, visitedCells);
            }
        }
    }
}

