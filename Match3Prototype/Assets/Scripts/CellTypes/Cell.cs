using UnityEngine;

public abstract class Cell : MonoBehaviour
{
    public CellType cellType;
    public Vector2Int gridPosition;
    public bool blasted;


    void Start()
    {
        GridController.Instance.SetGridElementOnStart(this);
    }

    public virtual void InitCell(CellType type, Vector2Int gridPos, Vector3 position, int id = -1, Sprite sprite = null)
    {
        cellType = type;
        gridPosition = gridPos;
        blasted = false;

        transform.localScale = Vector3.one;
        transform.position = position;
        gameObject.SetActive(true);
    }

    public virtual int GetCellId()
    {
        return -1;
    }

    public abstract void Blast(bool autoMatch);

    public virtual bool CheckGoal(int id, bool autoMatch)
    {
        if ((autoMatch && GoalHandler.Instance.countGoalWhenAutoMatch) || !autoMatch)
            return GoalHandler.Instance.IsInGoals(id);

        return false;
    }
}
