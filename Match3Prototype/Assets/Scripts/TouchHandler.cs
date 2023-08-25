using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchHandler : Singleton<TouchHandler>
{
    Cell firstCell;
    Cell secondCell;
    bool processTouch = true;
    public float swapTime = .15f;

    void Start()
    {
        Input.multiTouchEnabled = false;
    }

    IEnumerator ExecuteTouch(List<Cell> firstMatchList, List<Cell> secondMatchList)
    {
        GoalHandler.Instance.DecreaseMoveCount();

        BlastCells(firstMatchList);
        BlastCells(secondMatchList);

        Dictionary<int, List<FillData>> fillData = new Dictionary<int, List<FillData>>();
        yield return StartCoroutine(GridController.Instance.FallCells(fillData));
        yield return StartCoroutine(GridController.Instance.FillGrid(fillData));

        yield return StartCoroutine(GridController.Instance.CheckAutoMatches());
        yield return StartCoroutine(GridController.Instance.CheckShuffle());

        processTouch = GoalHandler.Instance.ProcessNextTouch();

        if (!processTouch)
        {
            yield return new WaitUntil(() => GameManager.Instance.GetProcessCount() == 0);
            GoalHandler.Instance.CheckGoalsCompleted();
        }

        ResetSelectedCells();
    }

    public void BlastCells(IEnumerable<Cell> matchList, bool autoMatch = false)
    {
        if (matchList == null)
            return;

        foreach (Cell tempCell in matchList)
        {
            tempCell.Blast(autoMatch);
        }
    }

    public List<Cell> FindMatchList(Cell cell)
    {
        List<Cell> matchList = MatchFinder.Instance.FindMatchList(cell);
        return (matchList.Count > 2) ? matchList : null;
    }

    #region TouchDetection

    void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        GetTouchEditor();
#else
		GetTouchMobile();
#endif
    }

    void GetTouchEditor()
    {
        if (Input.GetMouseButton(0))
        {
            CheckInputs();
        }

        if (Input.GetMouseButtonUp(0) && processTouch)
        {
            ResetSelectedCells();
        }
    }

    void GetTouchMobile()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Moved:
                    CheckInputs();
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (processTouch)
                        ResetSelectedCells();
                    break;
            }
        }
    }

    #endregion

    bool IsAbleToSwap()
    {
        return GridController.Instance.GetCellNeighbours(firstCell).Contains(secondCell);
    }

    void CheckInputs()
    {
        if (!processTouch)
            return;

        BoxCollider2D hit = (BoxCollider2D)Physics2D.OverlapPoint(GameManager.Instance.mainCam.ScreenToWorldPoint(Input.mousePosition));

        if (hit == null || !hit.TryGetComponent(out Cell cell) || cell.blasted)
            return;

        if (firstCell == null)
            firstCell = cell;
        else if (firstCell != cell)
        {
            secondCell = cell;
            if (IsAbleToSwap())
            {
                SetProcessTouch(false);
                Swap();
            }
        }
    }

    void Swap()
    {
        GridController.Instance.SwapCells(firstCell, secondCell);

        List<Cell> firstMatchList = FindMatchList(firstCell);
        List<Cell> secondMatchList = FindMatchList(secondCell);
        bool rollback = firstMatchList == null && secondMatchList == null;
        Vector3 temp = firstCell.transform.position;
        AnimateSwap().OnComplete(() =>
        {
            if (rollback)
            {
                GridController.Instance.SwapCells(firstCell, secondCell);
                AnimateSwap().OnComplete(() =>
                {
                    SetProcessTouch(true);
                    ResetSelectedCells();
                });
            }
            else
            {
                StartCoroutine(ExecuteTouch(firstMatchList, secondMatchList));
            }
        });
    }

    Sequence AnimateSwap()
    {
        Vector3 temp = firstCell.transform.position;
        return DOTween.Sequence()
        .Join(firstCell.transform.DOMove(secondCell.transform.position, swapTime))
        .Join(secondCell.transform.DOMove(temp, swapTime));
    }

    void SetProcessTouch(bool value)
    {
        processTouch = value;
    }

    void ResetSelectedCells()
    {
        firstCell = null;
        secondCell = null;
    }
}
