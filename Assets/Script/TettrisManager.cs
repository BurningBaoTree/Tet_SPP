using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TettrisManager : MonoBehaviour
{
    PlayerInput playerinput;

    public bool RigthmoveCheck = true;
    public bool LeftmoveCheck = true;
    public bool DownMoveCheck = true;

    bool reachend = false;

    /// <summary>
    /// 이번 턴 체크용
    /// </summary>
    bool trunActive = false;
    public bool TrunActive
    {
        get
        {
            return trunActive;
        }
        set
        {
            if (trunActive != value)
            {
                trunActive = value;
                if (!trunActive)
                {

                }
            }
        }
    }

    WorkArea wk;
    CoolTimeSys cool;

    Vector2 inputcontext;
    Vector2 Inputcontext
    {
        get
        {
            return inputcontext;
        }
        set
        {
            if (inputcontext != value)
            {
                inputcontext = value;

            }
        }
    }

    public cellState cellClass = cellState.Tstate;
    int rotateCheck = 0;
    int RotateCheck
    {
        get
        {
            return rotateCheck;
        }
        set
        {
            if (rotateCheck != value)
            {
                rotateCheck = value;
                if (rotateCheck > 3)
                {
                    rotateCheck = 0;
                }
                else if (rotateCheck < 0)
                {
                    rotateCheck = 3;
                }
            }
        }
    }


    /// <summary>
    /// 시작지점
    /// </summary>
    int StartPoint;

    /// <summary>
    /// 가운데 블록의 현재 지점
    /// </summary>
    int NowStatePoint;

    int previousPoint;

    /// <summary>
    /// 업데이트 실행용 델리게이트
    /// </summary>
    Action updater;

    List<CellData> blockGroup;

    private void Awake()
    {
        playerinput = new PlayerInput();
        wk = GetComponent<WorkArea>();
        cool = GetComponent<CoolTimeSys>();
    }
    private void OnEnable()
    {
        foreach (CellData cell in wk.cells)
        {
            cell.RightReached += RightCheck;
            cell.LeftReached += LeftCheck;
            cell.ReachedTheEnd += TurnEnd;
        }

        playerinput.Enable();
        playerinput.Player.MoveBlock.performed += MoveBlock;
    }

    private void Start()
    {
        TrunActive = true;
        //시작지점 계산 가로*세로 - 가로의 가운데지점을 int로 환산한 값(즉 제일 위에 가운데 번호의 셀)
        StartPoint = wk.HorizonCell * wk.VerticalCell - (int)(wk.HorizonCell * 0.5f);
        NowStatePoint = StartPoint;
        previousPoint = NowStatePoint;
        updater += fallDownBlock;
    }
    private void Update()
    {
        updater();
    }


    /// <summary>
    /// 패턴 정의용 선택함수
    /// </summary>
    /// <param name="num">cell리스트의 index값</param>
    /// <param name="cell">패턴별 cell 색상 선택용</param>
    void cellSellector(int num, cellState cell)
    {
        if (num < wk.cells.Count && num > -1)
        {
            wk.cells[num].CellState = cell;
            wk.cells[num].IsActivated = true;
            /*            blockGroup.Add(wk.cells[num]);*/
        }
    }
    /// <summary>
    /// 패턴 정의용 선택함수(가운데 정의)
    /// </summary>
    /// <param name="num">cell리스트의 index값</param>
    /// <param name="cell">패턴별 cell 색상 선택용</param>
    void cellCenteredmade(int num, cellState cell)
    {
        if (num < wk.cells.Count && num > -1)
        {
            wk.cells[num].CellState = cell;
            wk.cells[num].IsActivated = true;
            wk.cells[num].IsCenter = true;
            /*            blockGroup.Add(wk.cells[num]);*/
        }
    }
    /// <summary>
    /// 패턴 정의용 선택함수
    /// </summary>
    /// <param name="num">cell리스트의 index값</param>
    /// <param name="cell">패턴별 cell 색상 선택용</param>
    void CFcellSellector(int num, cellState cell)
    {
        if (num < wk.cells.Count && num > -1)
        {
            wk.cells[num].CellState = cell;
            wk.cells[num].IsActivated = false;
            wk.cells[num].IsSet = true;
            /*            blockGroup.Add(wk.cells[num]);*/
        }
    }
    /// <summary>
    /// 패턴 정의용 선택함수(가운데 정의)
    /// </summary>
    /// <param name="num">cell리스트의 index값</param>
    /// <param name="cell">패턴별 cell 색상 선택용</param>
    void CFcellCenteredmade(int num, cellState cell)
    {
        if (num < wk.cells.Count && num > -1)
        {
            wk.cells[num].CellState = cell;
            wk.cells[num].IsActivated = false;
            wk.cells[num].IsCenter = false;
            wk.cells[num].IsSet = true;
            /*            blockGroup.Add(wk.cells[num]);*/
        }
    }

    /// <summary>
    /// 패턴 정의용 선택함수
    /// </summary>
    /// <param name="num">cell리스트의 index값</param>
    /// <param name="cell">패턴별 cell 색상 선택용</param>
    void DecellSellect(int num)
    {
        if (num < wk.cells.Count && num > -1)
        {
            wk.cells[num].IsActivated = false;
        }
    }
    /// <summary>
    /// 패턴 정의용 선택함수(가운데 정의)
    /// </summary>
    /// <param name="num">cell리스트의 index값</param>
    /// <param name="cell">패턴별 cell 색상 선택용</param>
    void DecellCentered(int num)
    {
        if (num < wk.cells.Count && num > -1)
        {
            wk.cells[num].IsActivated = false;
            wk.cells[num].IsCenter = false;
        }
    }

    /// <summary>
    /// 스타트 포인트에서 블록이 떨어지게 하는 함수
    /// </summary>
    void fallDownBlock()
    {
        if (cool.coolclocks[0].coolEnd && NowStatePoint > 0)
        {
            SpawnBlock(NowStatePoint, cellClass, RotateCheck);
            previousPoint = NowStatePoint;
            cool.CoolTimeStart(0, 1f);
            NowStatePoint -= wk.HorizonCell;
        }
        else if (reachend)
        {
            cool.CoolTimeStart(0, 1f);
            TrunActive = false;
            reachend = false;
            conformBlock(previousPoint, cellClass, rotateCheck);
            cellClass = (cellState)UnityEngine.Random.Range(1, 8);
            NowStatePoint = StartPoint;
            TrunActive = true;
        }
    }
    void parking()
    {

    }

    void TurnEnd()
    {
        reachend = true;
        updater -= fallDownBlock;
    }
    void RightCheck(bool moveable)
    {
        RigthmoveCheck = moveable;
    }
    void LeftCheck(bool moveable)
    {
        LeftmoveCheck = moveable;
    }

    /// <summary>
    /// 인풋 액션 이동 함수
    /// </summary>
    /// <param name="context"></param>
    private void MoveBlock(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        Inputcontext = context.ReadValue<Vector2>();
        Debug.Log("호출" + Inputcontext);
        if (Inputcontext.x > 0)
        {
            RightMove();
        }
        else if (Inputcontext.x < 0)
        {
            LeftMove();
        }
        else if (Inputcontext.y < 0)
        {

        }
    }

    /// <summary>
    /// 왼쪽 이동 함수
    /// </summary>
    void LeftMove()
    {
        if (LeftmoveCheck)
        {
            NowStatePoint++;
            SpawnBlock(NowStatePoint, cellClass, RotateCheck);
            previousPoint = NowStatePoint;
            RigthmoveCheck = true;
        }
    }
    void RightMove()
    {
        if (RigthmoveCheck)
        {
            NowStatePoint--;
            SpawnBlock(NowStatePoint, cellClass, RotateCheck);
            previousPoint = NowStatePoint;
            LeftmoveCheck = true;
        }
    }
    void DownMove()
    {
        if (DownMoveCheck)
        {
            NowStatePoint -= wk.HorizonCell;
            SpawnBlock(NowStatePoint, cellClass, RotateCheck);
            previousPoint = NowStatePoint;
        }
        else
        {
            TrunActive = false;
        }
    }
    void RightRotateMove()
    {
        RotateCheck++;
        SpawnBlock(NowStatePoint, cellClass, RotateCheck);
        previousPoint = NowStatePoint;
    }
    void LeftRotateMove()
    {
        RotateCheck--;
        SpawnBlock(NowStatePoint, cellClass, RotateCheck);
        previousPoint = NowStatePoint;
    }


    //------------------------------<블록 스폰 관련>--------------------------------------------

    #region 블록 스폰 관련
    /// <summary>
    /// 블록 모양을 결정짓는 함수
    /// </summary>
    /// <param name="num">시작 인덱스</param>
    /// <param name="cell">셀 패턴</param>
    /// <param name="rotateState">회전 상태</param>
    void SpawnBlock(int num, cellState cell, int rotateState)
    {
        DespawnBlock(previousPoint, cellClass, RotateCheck);
        switch (cell)
        {
            case cellState.Tstate:
                switch (rotateState)
                {
                    case 0:
                        cellCenteredmade(num, cell);
                        cellSellector(num - wk.HorizonCell, cell);
                        cellSellector(num - 1, cell);
                        cellSellector(num + 1, cell);
                        break;
                    case 1:
                        cellCenteredmade(num, cell);
                        cellSellector(num - wk.HorizonCell, cell);
                        cellSellector(num - 1, cell);
                        cellSellector(num + wk.HorizonCell, cell);
                        break;
                    case 2:
                        cellCenteredmade(num, cell);
                        cellSellector(num + wk.HorizonCell, cell);
                        cellSellector(num - 1, cell);
                        cellSellector(num + 1, cell);
                        break;
                    case 3:
                        cellCenteredmade(num, cell);
                        cellSellector(num - wk.HorizonCell, cell);
                        cellSellector(num + wk.HorizonCell, cell);
                        cellSellector(num + 1, cell);
                        break;
                }
                break;
            case cellState.Lstate:
                switch (rotateState)
                {
                    case 0:
                        cellSellector(num + wk.HorizonCell, cell);
                        cellCenteredmade(num, cell);
                        cellSellector(num - wk.HorizonCell, cell);
                        cellSellector(num - wk.HorizonCell + 1, cell);
                        break;
                    case 1:
                        cellSellector(num + 1, cell);
                        cellCenteredmade(num, cell);
                        cellSellector(num - 1, cell);
                        cellSellector(num - 1 - wk.HorizonCell, cell);
                        break;
                    case 2:
                        cellSellector(num + wk.HorizonCell, cell);
                        cellCenteredmade(num, cell);
                        cellSellector(num - wk.HorizonCell, cell);
                        cellSellector(num + 1 + wk.HorizonCell, cell);
                        break;
                    case 3:
                        cellSellector(num - 1, cell);
                        cellCenteredmade(num, cell);
                        cellSellector(num + 1, cell);
                        cellSellector(num + 1 + wk.HorizonCell, cell);
                        break;
                }
                break;
            case cellState.Sstate:
                switch (rotateState)
                {
                    case 0:
                        cellSellector(num + wk.HorizonCell, cell);
                        cellCenteredmade(num, cell);
                        cellSellector(num + 1, cell);
                        cellSellector(num - wk.HorizonCell + 1, cell);
                        break;
                    case 1:
                        cellSellector(num + 1, cell);
                        cellCenteredmade(num, cell);
                        cellSellector(num - wk.HorizonCell, cell);
                        cellSellector(num - wk.HorizonCell - 1, cell);
                        break;
                    case 2:
                        cellSellector(num - 1 + wk.HorizonCell, cell);
                        cellSellector(num - 1, cell);
                        cellCenteredmade(num, cell);
                        cellSellector(num - wk.HorizonCell, cell);
                        break;
                    case 3:
                        cellSellector(num + wk.HorizonCell + 1, cell);
                        cellSellector(num + wk.HorizonCell, cell);
                        cellCenteredmade(num, cell);
                        cellSellector(num - 1, cell);
                        break;
                }
                break;
            ///아래 모양 무슨 모양인지 모르겠음
            case cellState.Rstate:
                switch (rotateState)
                {
                    case 0:
                        cellCenteredmade(num, cell);
                        cellSellector(num - wk.HorizonCell, cell);
                        cellSellector(num - 1, cell);
                        cellSellector(num + 1, cell);
                        break;
                    case 1:
                        cellCenteredmade(num, cell);
                        cellSellector(num - wk.HorizonCell, cell);
                        cellSellector(num - 1, cell);
                        cellSellector(num + 1, cell);
                        break;
                    case 2:
                        cellCenteredmade(num, cell);
                        cellSellector(num - wk.HorizonCell, cell);
                        cellSellector(num - 1, cell);
                        cellSellector(num + 1, cell);
                        break;
                    case 3:
                        cellCenteredmade(num, cell);
                        cellSellector(num - wk.HorizonCell, cell);
                        cellSellector(num - 1, cell);
                        cellSellector(num + 1, cell);
                        break;
                }
                break;
            case cellState.ReLstate:
                switch (rotateState)
                {
                    case 0:
                        cellSellector(num + wk.HorizonCell, cell);
                        cellCenteredmade(num, cell);
                        cellSellector(num - wk.HorizonCell, cell);
                        cellSellector(num - wk.HorizonCell - 1, cell);
                        break;
                    case 1:
                        cellSellector(num + 1, cell);
                        cellCenteredmade(num, cell);
                        cellSellector(num - 1, cell);
                        cellSellector(num - 1 + wk.HorizonCell, cell);
                        break;
                    case 2:
                        cellSellector(num + wk.HorizonCell, cell);
                        cellCenteredmade(num, cell);
                        cellSellector(num - wk.HorizonCell, cell);
                        cellSellector(num - 1 + wk.HorizonCell, cell);
                        break;
                    case 3:
                        cellSellector(num - 1, cell);
                        cellCenteredmade(num, cell);
                        cellSellector(num + 1, cell);
                        cellSellector(num + 1 - wk.HorizonCell, cell);
                        break;
                }
                break;
            case cellState.ReSstate:
                switch (rotateState)
                {
                    case 0:
                        cellSellector(num + wk.HorizonCell, cell);
                        cellCenteredmade(num, cell);
                        cellSellector(num - 1, cell);
                        cellSellector(num - wk.HorizonCell - 1, cell);
                        break;
                    case 1:
                        cellSellector(num - 1, cell);
                        cellCenteredmade(num, cell);
                        cellSellector(num + wk.HorizonCell, cell);
                        cellSellector(num + wk.HorizonCell + 1, cell);
                        break;
                    case 2:
                        cellSellector(num - 1 + wk.HorizonCell, cell);
                        cellSellector(num - 1, cell);
                        cellCenteredmade(num, cell);
                        cellSellector(num - wk.HorizonCell, cell);
                        break;
                    case 3:
                        cellSellector(num - wk.HorizonCell - 1, cell);
                        cellSellector(num - wk.HorizonCell, cell);
                        cellCenteredmade(num, cell);
                        cellSellector(num + 1, cell);
                        break;
                }
                break;
            case cellState.Square:
                cellCenteredmade(num, cell);
                cellSellector(num - wk.HorizonCell, cell);
                cellSellector(num - 1 - wk.HorizonCell, cell);
                cellSellector(num - 1, cell);
                break;
            case cellState.Bar:
                switch (rotateState)
                {
                    case 0:
                        cellSellector(num + wk.HorizonCell, cell);
                        cellCenteredmade(num, cell);
                        cellSellector(num - wk.HorizonCell, cell);
                        cellSellector(num - wk.HorizonCell * 2, cell);
                        break;
                    case 1:
                        cellSellector(num - 1, cell);
                        cellCenteredmade(num, cell);
                        cellSellector(num + 1, cell);
                        cellSellector(num + 2, cell);
                        break;
                    case 2:
                        cellSellector(num - wk.HorizonCell, cell);
                        cellCenteredmade(num, cell);
                        cellSellector(num + wk.HorizonCell, cell);
                        cellSellector(num + wk.HorizonCell * 2, cell);
                        break;
                    case 3:
                        cellSellector(num + 1, cell);
                        cellCenteredmade(num, cell);
                        cellSellector(num - 1, cell);
                        cellSellector(num - 2, cell);
                        break;
                }
                break;
            default:
                break;
        }
    }
    void DespawnBlock(int num, cellState cell, int rotateState)
    {
        switch (cell)
        {
            case cellState.Tstate:
                switch (rotateState)
                {
                    case 0:
                        DecellCentered(num);
                        DecellSellect(num - wk.HorizonCell);
                        DecellSellect(num - 1);
                        DecellSellect(num + 1);
                        break;
                    case 1:
                        DecellCentered(num);
                        DecellSellect(num - wk.HorizonCell);
                        DecellSellect(num - 1);
                        DecellSellect(num + wk.HorizonCell);
                        break;
                    case 2:
                        DecellCentered(num);
                        DecellSellect(num + wk.HorizonCell);
                        DecellSellect(num - 1);
                        DecellSellect(num + 1);
                        break;
                    case 3:
                        DecellCentered(num);
                        DecellSellect(num - wk.HorizonCell);
                        DecellSellect(num + wk.HorizonCell);
                        DecellSellect(num + 1);
                        break;
                }
                break;
            case cellState.Lstate:
                switch (rotateState)
                {
                    case 0:
                        DecellSellect(num + wk.HorizonCell);
                        DecellCentered(num);
                        DecellSellect(num - wk.HorizonCell);
                        DecellSellect(num - wk.HorizonCell + 1);
                        break;
                    case 1:
                        DecellSellect(num + 1);
                        DecellCentered(num);
                        DecellSellect(num - 1);
                        DecellSellect(num - 1 - wk.HorizonCell);
                        break;
                    case 2:
                        DecellSellect(num + wk.HorizonCell);
                        DecellCentered(num);
                        DecellSellect(num - wk.HorizonCell);
                        DecellSellect(num + 1 + wk.HorizonCell);
                        break;
                    case 3:
                        DecellSellect(num - 1);
                        DecellCentered(num);
                        DecellSellect(num + 1);
                        DecellSellect(num + 1 + wk.HorizonCell);
                        break;
                }
                break;
            case cellState.Sstate:
                switch (rotateState)
                {
                    case 0:
                        DecellSellect(num + wk.HorizonCell);
                        DecellCentered(num);
                        DecellSellect(num + 1);
                        DecellSellect(num - wk.HorizonCell + 1);
                        break;
                    case 1:
                        DecellSellect(num + 1);
                        DecellCentered(num);
                        DecellSellect(num - wk.HorizonCell);
                        DecellSellect(num - wk.HorizonCell - 1);
                        break;
                    case 2:
                        DecellSellect(num - 1 + wk.HorizonCell);
                        DecellSellect(num - 1);
                        DecellCentered(num);
                        DecellSellect(num - wk.HorizonCell);
                        break;
                    case 3:
                        DecellSellect(num + wk.HorizonCell + 1);
                        DecellSellect(num + wk.HorizonCell);
                        DecellCentered(num);
                        DecellSellect(num - 1);
                        break;
                }
                break;
            ///아래 모양 무슨 모양인지 모르겠음
            case cellState.Rstate:
                switch (rotateState)
                {
                    case 0:
                        DecellCentered(num);
                        DecellSellect(num - wk.HorizonCell);
                        DecellSellect(num - 1);
                        DecellSellect(num + 1);
                        break;
                    case 1:
                        DecellCentered(num);
                        DecellSellect(num - wk.HorizonCell);
                        DecellSellect(num - 1);
                        DecellSellect(num + 1);
                        break;
                    case 2:
                        DecellCentered(num);
                        DecellSellect(num - wk.HorizonCell);
                        DecellSellect(num - 1);
                        DecellSellect(num + 1);
                        break;
                    case 3:
                        DecellCentered(num);
                        DecellSellect(num - wk.HorizonCell);
                        DecellSellect(num - 1);
                        DecellSellect(num + 1);
                        break;
                }
                break;
            case cellState.ReLstate:
                switch (rotateState)
                {
                    case 0:
                        DecellSellect(num + wk.HorizonCell);
                        DecellCentered(num);
                        DecellSellect(num - wk.HorizonCell);
                        DecellSellect(num - wk.HorizonCell - 1);
                        break;
                    case 1:
                        DecellSellect(num + 1);
                        DecellCentered(num);
                        DecellSellect(num - 1);
                        DecellSellect(num - 1 + wk.HorizonCell);
                        break;
                    case 2:
                        DecellSellect(num + wk.HorizonCell);
                        DecellCentered(num);
                        DecellSellect(num - wk.HorizonCell);
                        DecellSellect(num - 1 + wk.HorizonCell);
                        break;
                    case 3:
                        DecellSellect(num - 1);
                        DecellCentered(num);
                        DecellSellect(num + 1);
                        DecellSellect(num + 1 - wk.HorizonCell);
                        break;
                }
                break;
            case cellState.ReSstate:
                switch (rotateState)
                {
                    case 0:
                        DecellSellect(num + wk.HorizonCell);
                        DecellCentered(num);
                        DecellSellect(num - 1);
                        DecellSellect(num - wk.HorizonCell - 1);
                        break;
                    case 1:
                        DecellSellect(num - 1);
                        DecellCentered(num);
                        DecellSellect(num + wk.HorizonCell);
                        DecellSellect(num + wk.HorizonCell + 1);
                        break;
                    case 2:
                        DecellSellect(num - 1 + wk.HorizonCell);
                        DecellSellect(num - 1);
                        DecellCentered(num);
                        DecellSellect(num - wk.HorizonCell);
                        break;
                    case 3:
                        DecellSellect(num - wk.HorizonCell - 1);
                        DecellSellect(num - wk.HorizonCell);
                        DecellCentered(num);
                        DecellSellect(num + 1);
                        break;
                }
                break;
            case cellState.Square:
                DecellCentered(num);
                DecellSellect(num - wk.HorizonCell);
                DecellSellect(num - 1 - wk.HorizonCell);
                DecellSellect(num - 1);
                break;
            case cellState.Bar:
                switch (rotateState)
                {
                    case 0:
                        DecellSellect(num + wk.HorizonCell);
                        DecellCentered(num);
                        DecellSellect(num - wk.HorizonCell);
                        DecellSellect(num - wk.HorizonCell * 2);
                        break;
                    case 1:
                        DecellSellect(num - 1);
                        DecellCentered(num);
                        DecellSellect(num + 1);
                        DecellSellect(num + 2);
                        break;
                    case 2:
                        DecellSellect(num - wk.HorizonCell);
                        DecellCentered(num);
                        DecellSellect(num + wk.HorizonCell);
                        DecellSellect(num + wk.HorizonCell * 2);
                        break;
                    case 3:
                        DecellSellect(num + 1);
                        DecellCentered(num);
                        DecellSellect(num - 1);
                        DecellSellect(num - 2);
                        break;
                }
                break;
        }
    }
    void conformBlock(int num, cellState cell, int rotateState)
    {
        switch (cell)
        {
            case cellState.Tstate:
                switch (rotateState)
                {
                    case 0:
                        CFcellCenteredmade(num, cell);
                        CFcellSellector(num - wk.HorizonCell, cell);
                        CFcellSellector(num - 1, cell);
                        CFcellSellector(num + 1, cell);
                        break;
                    case 1:
                        CFcellCenteredmade(num, cell);
                        CFcellSellector(num - wk.HorizonCell, cell);
                        CFcellSellector(num - 1, cell);
                        CFcellSellector(num + wk.HorizonCell, cell);
                        break;
                    case 2:
                        CFcellCenteredmade(num, cell);
                        CFcellSellector(num + wk.HorizonCell, cell);
                        CFcellSellector(num - 1, cell);
                        CFcellSellector(num + 1, cell);
                        break;
                    case 3:
                        CFcellCenteredmade(num, cell);
                        CFcellSellector(num - wk.HorizonCell, cell);
                        CFcellSellector(num + wk.HorizonCell, cell);
                        CFcellSellector(num + 1, cell);
                        break;
                }
                break;
            case cellState.Lstate:
                switch (rotateState)
                {
                    case 0:
                        CFcellSellector(num + wk.HorizonCell, cell);
                        CFcellCenteredmade(num, cell);
                        CFcellSellector(num - wk.HorizonCell, cell);
                        CFcellSellector(num - wk.HorizonCell + 1, cell);
                        break;
                    case 1:
                        CFcellSellector(num + 1, cell);
                        CFcellCenteredmade(num, cell);
                        CFcellSellector(num - 1, cell);
                        CFcellSellector(num - 1 - wk.HorizonCell, cell);
                        break;
                    case 2:
                        CFcellSellector(num + wk.HorizonCell, cell);
                        CFcellCenteredmade(num, cell);
                        CFcellSellector(num - wk.HorizonCell, cell);
                        CFcellSellector(num + 1 + wk.HorizonCell, cell);
                        break;
                    case 3:
                        CFcellSellector(num - 1, cell);
                        CFcellCenteredmade(num, cell);
                        CFcellSellector(num + 1, cell);
                        CFcellSellector(num + 1 + wk.HorizonCell, cell);
                        break;
                }
                break;
            case cellState.Sstate:
                switch (rotateState)
                {
                    case 0:
                        CFcellSellector(num + wk.HorizonCell, cell);
                        CFcellCenteredmade(num, cell);
                        CFcellSellector(num + 1, cell);
                        CFcellSellector(num - wk.HorizonCell + 1, cell);
                        break;
                    case 1:
                        CFcellSellector(num + 1, cell);
                        CFcellCenteredmade(num, cell);
                        CFcellSellector(num - wk.HorizonCell, cell);
                        CFcellSellector(num - wk.HorizonCell - 1, cell);
                        break;
                    case 2:
                        CFcellSellector(num - 1 + wk.HorizonCell, cell);
                        CFcellSellector(num - 1, cell);
                        CFcellCenteredmade(num, cell);
                        CFcellSellector(num - wk.HorizonCell, cell);
                        break;
                    case 3:
                        CFcellSellector(num + wk.HorizonCell + 1, cell);
                        CFcellSellector(num + wk.HorizonCell, cell);
                        CFcellCenteredmade(num, cell);
                        CFcellSellector(num - 1, cell);
                        break;
                }
                break;
            ///아래 모양 무슨 모양인지 모르겠음
            case cellState.Rstate:
                switch (rotateState)
                {
                    case 0:
                        CFcellCenteredmade(num, cell);
                        CFcellSellector(num - wk.HorizonCell, cell);
                        CFcellSellector(num - 1, cell);
                        CFcellSellector(num + 1, cell);
                        break;
                    case 1:
                        CFcellCenteredmade(num, cell);
                        CFcellSellector(num - wk.HorizonCell, cell);
                        CFcellSellector(num - 1, cell);
                        CFcellSellector(num + 1, cell);
                        break;
                    case 2:
                        CFcellCenteredmade(num, cell);
                        CFcellSellector(num - wk.HorizonCell, cell);
                        CFcellSellector(num - 1, cell);
                        CFcellSellector(num + 1, cell);
                        break;
                    case 3:
                        CFcellCenteredmade(num, cell);
                        CFcellSellector(num - wk.HorizonCell, cell);
                        CFcellSellector(num - 1, cell);
                        CFcellSellector(num + 1, cell);
                        break;
                }
                break;
            case cellState.ReLstate:
                switch (rotateState)
                {
                    case 0:
                        CFcellSellector(num + wk.HorizonCell, cell);
                        CFcellCenteredmade(num, cell);
                        CFcellSellector(num - wk.HorizonCell, cell);
                        CFcellSellector(num - wk.HorizonCell - 1, cell);
                        break;
                    case 1:
                        CFcellSellector(num + 1, cell);
                        CFcellCenteredmade(num, cell);
                        CFcellSellector(num - 1, cell);
                        CFcellSellector(num - 1 + wk.HorizonCell, cell);
                        break;
                    case 2:
                        CFcellSellector(num + wk.HorizonCell, cell);
                        CFcellCenteredmade(num, cell);
                        CFcellSellector(num - wk.HorizonCell, cell);
                        CFcellSellector(num - 1 + wk.HorizonCell, cell);
                        break;
                    case 3:
                        CFcellSellector(num - 1, cell);
                        CFcellCenteredmade(num, cell);
                        CFcellSellector(num + 1, cell);
                        CFcellSellector(num + 1 - wk.HorizonCell, cell);
                        break;
                }
                break;
            case cellState.ReSstate:
                switch (rotateState)
                {
                    case 0:
                        CFcellSellector(num + wk.HorizonCell, cell);
                        CFcellCenteredmade(num, cell);
                        CFcellSellector(num - 1, cell);
                        CFcellSellector(num - wk.HorizonCell - 1, cell);
                        break;
                    case 1:
                        CFcellSellector(num - 1, cell);
                        CFcellCenteredmade(num, cell);
                        CFcellSellector(num + wk.HorizonCell, cell);
                        CFcellSellector(num + wk.HorizonCell + 1, cell);
                        break;
                    case 2:
                        CFcellSellector(num - 1 + wk.HorizonCell, cell);
                        CFcellSellector(num - 1, cell);
                        CFcellCenteredmade(num, cell);
                        CFcellSellector(num - wk.HorizonCell, cell);
                        break;
                    case 3:
                        CFcellSellector(num - wk.HorizonCell - 1, cell);
                        CFcellSellector(num - wk.HorizonCell, cell);
                        CFcellCenteredmade(num, cell);
                        CFcellSellector(num + 1, cell);
                        break;
                }
                break;
            case cellState.Square:
                CFcellCenteredmade(num, cell);
                CFcellSellector(num - wk.HorizonCell, cell);
                CFcellSellector(num - 1 - wk.HorizonCell, cell);
                CFcellSellector(num - 1, cell);
                break;
            case cellState.Bar:
                switch (rotateState)
                {
                    case 0:
                        CFcellSellector(num + wk.HorizonCell, cell);
                        CFcellCenteredmade(num, cell);
                        CFcellSellector(num - wk.HorizonCell, cell);
                        CFcellSellector(num - wk.HorizonCell * 2, cell);
                        break;
                    case 1:
                        CFcellSellector(num - 1, cell);
                        CFcellCenteredmade(num, cell);
                        CFcellSellector(num + 1, cell);
                        CFcellSellector(num + 2, cell);
                        break;
                    case 2:
                        CFcellSellector(num - wk.HorizonCell, cell);
                        CFcellCenteredmade(num, cell);
                        CFcellSellector(num + wk.HorizonCell, cell);
                        CFcellSellector(num + wk.HorizonCell * 2, cell);
                        break;
                    case 3:
                        CFcellSellector(num + 1, cell);
                        CFcellCenteredmade(num, cell);
                        CFcellSellector(num - 1, cell);
                        CFcellSellector(num - 2, cell);
                        break;
                }
                break;
            default:
                break;
        }
    }
    #endregion
}


//이 코드에서 할일 : 조작, 바닥에 닿았다는 신호 받아오기, 턴 관리, 시간 관리,
