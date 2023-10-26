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
    public bool CantMove = false;

    List<CellData> blockGroup = new List<CellData>();

    /// <summary>
    /// 이번 턴 체크용 프로퍼티
    /// </summary>
    public bool trunActive = false;
    public bool TurnActive
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
                    updater += parking;
                }
                else
                {
                    CallNectTurn();
                }
            }
        }
    }

    /// <summary>
    /// 게임 화면 컴포넌트
    /// </summary>
    WorkArea wk;

    /// <summary>
    /// 쿨타임 시스템
    /// </summary>
    CoolTimeSys cool;

    /// <summary>
    /// 블록 이동 관련 Vector2 변수
    /// </summary>
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

    /// <summary>
    /// 현재 조종중인 블록 패턴
    /// </summary>
    public cellState cellClass = cellState.Tstate;

    /// <summary>
    /// 로테이션 조종용 int 변수
    /// </summary>
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
    int previousRotate = 0;

    /// <summary>
    /// 시작지점
    /// </summary>
    int StartPoint;

    /// <summary>
    /// 가운데 블록의 현재 지점
    /// </summary>
    int NowStatePoint;

    /// <summary>
    /// 이전 지점
    /// </summary>
    int previousPoint;

    /// <summary>
    /// 선행 지점
    /// </summary>
    int setPoint;

    /// <summary>
    /// 업데이트 실행용 델리게이트
    /// </summary>
    Action updater;


    //일부 컴포넌트 가져오기
    private void Awake()
    {
        playerinput = new PlayerInput();
        wk = GetComponent<WorkArea>();
        cool = GetComponent<CoolTimeSys>();
    }

    //모든 셀에 함수 구독 및 input시스템
    private void OnEnable()
    {
        foreach (CellData cell in wk.cells)
        {
            cell.RightReached += RightCheck;
            cell.LeftReached += LeftCheck;
            cell.ReachedTheEnd += TurnEndDelay;
        }
        playerinput.Enable();
        playerinput.Player.MoveBlock.performed += MoveBlock;
        playerinput.Player.RotateBlock.performed += RotateBlock;
        playerinput.Player.Space.performed += DownBlockImmidiately;
    }

    //시작지점 지정, 턴 상태 시작
    private void Start()
    {
        //시작지점 계산 가로*세로 - 가로의 가운데지점을 int로 환산한 값(즉 제일 위에 가운데 번호의 셀)
        StartPoint = wk.HorizonCell * wk.VerticalCell - (int)(wk.HorizonCell * 0.5f);
        TurnActive = true;
    }
    private void Update()
    {
        updater();
    }

    /// <summary>
    /// 스타트 포인트에서 블록이 떨어지게 하는 함수
    /// </summary>
    void fallDownBlock()
    {
        if (cool.coolclocks[0].coolEnd && TurnActive && !CantMove)
        {
            SpawnBlock(NowStatePoint, cellClass, RotateCheck, true);
            previousPoint = NowStatePoint;
            setPoint = NowStatePoint - wk.HorizonCell;
            cool.CoolTimeStart(0, 1f);
            NowStatePoint -= wk.HorizonCell;
            DownMoveCheck = true;
        }
        else if (CantMove)
        {
            TurnEndDelay();
        }
    }

    /// <summary>
    /// 블록 도착 실행 함수
    /// </summary>
    void parking()
    {
        if (cool.coolclocks[1].coolEnd)
        {
            conformBlock(previousPoint, cellClass, rotateCheck);
            previousPoint = StartPoint;
            LinePathch();
            updater -= parking;
            TurnActive = true;
        }
    }

    /// <summary>
    /// 턴이 끝났음을 알리는 함수(IsEnd델리게이트랑 연결되어있음)
    /// </summary>
    void TurnEndDelay()
    {
        cool.CoolTimeStart(0, 1.5f);
        updater -= fallDownBlock;
        DownMoveCheck = false;
        cool.CoolTimeStart(1, 1f);
        TurnActive = false;
    }

    /// <summary>
    /// 오른쪽으로 이동할수 있는지 체크용 함수
    /// </summary>
    /// <param name="moveable"></param>
    void RightCheck(bool moveable)
    {
        RigthmoveCheck = moveable;
    }

    /// <summary>
    /// 왼쪽으로 이동할수 있는지 체크용 함수
    /// </summary>
    /// <param name="moveable"></param>
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
            DownMove();
        }
    }

    /// <summary>
    /// 인풋액션 블록을 맨 아래로 보내버리는 함수
    /// </summary>
    /// <param name="context"></param>
    private void DownBlockImmidiately(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        updater += DownMove;
    }

    /// <summary>
    /// 인풋액션 블록을 회전시키는 함수
    /// </summary>
    /// <param name="context"></param>
    private void RotateBlock(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        float con = context.ReadValue<float>();
        if (con > 0)
        {
            RightRotateMove();
        }
        else if (con < 0)
        {
            LeftRotateMove();
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
            SpawnBlock(NowStatePoint, cellClass, RotateCheck, true);
            previousPoint = NowStatePoint;
            setPoint = NowStatePoint - wk.HorizonCell;
            RigthmoveCheck = true;
        }
    }

    /// <summary>
    /// 오른쪽 이동 함수
    /// </summary>
    void RightMove()
    {
        if (RigthmoveCheck)
        {
            NowStatePoint--;
            SpawnBlock(NowStatePoint, cellClass, RotateCheck, true);
            previousPoint = NowStatePoint;
            setPoint = NowStatePoint - wk.HorizonCell;
            LeftmoveCheck = true;
        }
    }

    /// <summary>
    /// 아랫쪽 이동 함수
    /// </summary>
    void DownMove()
    {
        if (DownMoveCheck)
        {
            NowStatePoint -= wk.HorizonCell;
            SpawnBlock(NowStatePoint, cellClass, RotateCheck, true);
            previousPoint = NowStatePoint;
            setPoint = NowStatePoint - wk.HorizonCell;
        }
        else
        {
            updater -= DownMove;
            TurnActive = false;
        }
    }

    /// <summary>
    /// 오른쪽 회전 함수
    /// </summary>
    void RightRotateMove()
    {
        RotateCheck++;
        SpawnBlock(NowStatePoint, cellClass, RotateCheck, true);
        previousPoint = NowStatePoint;
        previousRotate = RotateCheck;
        setPoint = NowStatePoint - wk.HorizonCell;
        LeftCheck(true);
        RightCheck(true);
    }

    /// <summary>
    /// 왼쪽 회전 함수
    /// </summary>
    void LeftRotateMove()
    {
        RotateCheck--;
        SpawnBlock(NowStatePoint, cellClass, RotateCheck, true);
        previousPoint = NowStatePoint;
        previousRotate = RotateCheck;
        setPoint = NowStatePoint - wk.HorizonCell;
        LeftCheck(true);
        RightCheck(true);
    }

    /// <summary>
    /// 턴이 끝나고 같은 라인을 확인하여 한줄이 다 블록으로 차 있으면 사라지고 위에 블록을 가져온다.
    /// </summary>
    void LinePathch()
    {
        for (int i = 0; i < wk.VerticalCell; i++)
        {
            bool LineCheck = true;
            for (int j = 0; j < wk.HorizonCell; j++)
            {
                if (!wk.cells[i * wk.HorizonCell + j].IsSet)
                {
                    LineCheck = false;
                    break;
                }
            }

            if (LineCheck)
            {
                for (int j = 0; j < wk.HorizonCell; j++)
                {
                    blockGroup.Add(wk.cells[i * wk.HorizonCell + j]);
                }
                reFreshBlocks(i);
            }
        }
    }

    void reFreshBlocks(int row)
    {
        //리스트에 있는 줄 다 지우기
        foreach (CellData cell in blockGroup)
        {
            cell.IsSet = false;
            cell.IsActivated = false;
            cell.HoldCheck = false;
            cell.RightEnd = false;
            cell.LeftEnd = false;
        }
        blockGroup.Clear();
        //줄을 한칸씩 땡겨오기
        for (int i = row; i < wk.VerticalCell - 1; i++)
        {
            for (int j = 0; j < wk.HorizonCell; j++)
            {
                wk.cells[i * wk.HorizonCell + j].IsSet = wk.cells[(i + 1) * wk.HorizonCell + j].IsSet;
                wk.cells[i * wk.HorizonCell + j].CellState = wk.cells[(i + 1) * wk.HorizonCell + j].CellState;
                wk.cells[i * wk.HorizonCell + j].RightEnd = wk.cells[(i + 1) * wk.HorizonCell + j].RightEnd;
                wk.cells[i * wk.HorizonCell + j].LeftEnd = wk.cells[(i + 1) * wk.HorizonCell + j].LeftEnd;
                wk.cells[i * wk.HorizonCell + j].HoldCheck = wk.cells[(i + 1) * wk.HorizonCell + j].HoldCheck;
            }
        }
        // 맨 위 줄 지우기
        for (int j = 0; j < wk.HorizonCell; j++)
        {
            wk.cells[(wk.VerticalCell - 1) * wk.HorizonCell + j].IsSet = false;
            wk.cells[(wk.VerticalCell - 1) * wk.HorizonCell + j].CellState = cellState.Empty;
            wk.cells[(wk.VerticalCell - 1) * wk.HorizonCell + j].RightEnd = false;
            wk.cells[(wk.VerticalCell - 1) * wk.HorizonCell + j].LeftEnd = false;
            wk.cells[(wk.VerticalCell - 1) * wk.HorizonCell + j].HoldCheck = false;
        }
    }


    /// <summary>
    /// 다음 턴을 실행하며 일부 값들을 초기화 및 업데이터에 함수 구독
    /// </summary>
    void CallNectTurn()
    {
        DownMoveCheck = true;
        RigthmoveCheck = true;
        LeftmoveCheck = true;
        CantMove = false;
        cellClass = (cellState)UnityEngine.Random.Range(1, 8);
        NowStatePoint = StartPoint;
        setPoint = NowStatePoint - wk.HorizonCell;
        cool.CoolTimeStart(0, 1f);
        updater += fallDownBlock;
    }
    //------------------------------<블록 스폰 관련>--------------------------------------------
    #region 블록 생성 관련
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
            setPoint = previousPoint;
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
        }
    }
    /// <summary>
    /// 패턴 정의용 선택함수
    /// </summary>
    /// <param name="num">cell리스트의 index값</param>
    /// <param name="cell">패턴별 cell 색상 선택용</param>
    void CFcellSellector(int num, cellState cell)
    {
        try
        {
            if (num < wk.cells.Count && num > -1)
            {
                wk.cells[num].CellState = cell;
                wk.cells[num].IsActivated = true;
                wk.cells[num].IsSet = true;
                wk.cells[num + wk.HorizonCell].HoldCheck = true;

                //한 가로줄에서 왼쪽과 오른쪽 끝을 계산해서 제외해야한다.
                if (11 != num % wk.cells.Count)
                {
                    //왼쪽칸에 RightEnd
                    wk.cells[num + 1].RightEnd = true;
                }
                if (0 != num % wk.cells.Count)
                {
                    //오른쪽 칸에 LeftEnd
                    wk.cells[num - 1].LeftEnd = true;
                }
            }
        }
        catch
        {
            Debug.Log("게임오버");
        }
    }
    /// <summary>
    /// 패턴 정의용 선택함수(가운데 정의)
    /// </summary>
    /// <param name="num">cell리스트의 index값</param>
    /// <param name="cell">패턴별 cell 색상 선택용</param>
    void CFcellCenteredmade(int num, cellState cell)
    {
        try
        {
            if (num < wk.cells.Count && num > -1)
            {
                wk.cells[num].CellState = cell;
                wk.cells[num].IsActivated = true;
                wk.cells[num].IsCenter = false;
                wk.cells[num].IsSet = true;
                wk.cells[num + wk.HorizonCell].HoldCheck = true;
                if (wk.cells[num + 1] != null)
                {
                    wk.cells[num + 1].RightEnd = true;
                }
                if (wk.cells[num - 1] != null)
                {
                    wk.cells[num - 1].LeftEnd = true;
                }
            }
        }
        catch
        {
            Debug.Log("게임오버");
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
    /// 패턴 정의용 선택함수
    /// </summary>
    /// <param name="num">cell리스트의 index값</param>
    /// <param name="cell">패턴별 cell 색상 선택용</param>
    void ComcellSellector(int num, cellState cell)
    {
        if (num < wk.cells.Count && num > -1)
        {
            if (wk.cells[num].HoldCheck)
            {
                CantMove = true;
            }
        }
    }
    /// <summary>
    /// 패턴 정의용 선택함수(가운데 정의)
    /// </summary>
    /// <param name="num">cell리스트의 index값</param>
    /// <param name="cell">패턴별 cell 색상 선택용</param>
    void ComcellCenteredmade(int num, cellState cell)
    {
        if (num < wk.cells.Count && num > -1)
        {
            if (wk.cells[num].HoldCheck)
            {
                CantMove = true;
            }
        }
    }
    #endregion
    #region 블록 스폰 관련
    /// <summary>
    /// 블록 이동시 모양을 담당하는 함수
    /// </summary>
    /// <param name="num">인덱스</param>
    /// <param name="cell">셀 패턴</param>
    /// <param name="rotateState">회전값</param>
    /// <param name="DeletPrevious">전에껄 삭제할지 말지 결정</param>
    void SpawnBlock(int num, cellState cell, int rotateState, bool DeletPrevious)
    {
        if (DeletPrevious)
        {
            DespawnBlock(previousPoint, cellClass, previousRotate);
        }
        CompairSpawnBlock(setPoint, cellClass, RotateCheck);
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
                        cellSellector(num - 1, cell);
                        cellCenteredmade(num, cell);
                        cellSellector(num + 1, cell);
                        cellSellector(num + 1 + wk.HorizonCell, cell);
                        break;
                    case 2:
                        cellSellector(num + wk.HorizonCell, cell);
                        cellCenteredmade(num, cell);
                        cellSellector(num - wk.HorizonCell, cell);
                        cellSellector(num - 1 + wk.HorizonCell, cell);
                        break;
                    case 3:
                        cellSellector(num + 1, cell);
                        cellCenteredmade(num, cell);
                        cellSellector(num - 1, cell);
                        cellSellector(num - 1 - wk.HorizonCell, cell);
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
                        cellSellector(num + wk.HorizonCell, cell);
                        cellCenteredmade(num, cell);
                        cellSellector(num + 1, cell);
                        cellSellector(num - wk.HorizonCell + 1, cell);
                        break;
                    case 3:
                        cellSellector(num + 1, cell);
                        cellCenteredmade(num, cell);
                        cellSellector(num - wk.HorizonCell, cell);
                        cellSellector(num - wk.HorizonCell - 1, cell);
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
                        cellSellector(num - 1, cell);
                        cellCenteredmade(num, cell);
                        cellSellector(num + 1, cell);
                        cellSellector(num + 1 - wk.HorizonCell, cell);
                        break;
                    case 2:
                        cellSellector(num + wk.HorizonCell, cell);
                        cellCenteredmade(num, cell);
                        cellSellector(num - wk.HorizonCell, cell);
                        cellSellector(num + 1 + wk.HorizonCell, cell);
                        break;
                    case 3:
                        cellSellector(num + 1, cell);
                        cellCenteredmade(num, cell);
                        cellSellector(num - 1, cell);
                        cellSellector(num - 1 + wk.HorizonCell, cell);
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
                        cellSellector(num + 1, cell);
                        cellCenteredmade(num, cell);
                        cellSellector(num + wk.HorizonCell, cell);
                        cellSellector(num + wk.HorizonCell - 1, cell);
                        break;
                    case 2:
                        cellSellector(num + wk.HorizonCell, cell);
                        cellCenteredmade(num, cell);
                        cellSellector(num - 1, cell);
                        cellSellector(num - wk.HorizonCell - 1, cell);
                        break;
                    case 3:
                        cellSellector(num + 1, cell);
                        cellCenteredmade(num, cell);
                        cellSellector(num + wk.HorizonCell, cell);
                        cellSellector(num + wk.HorizonCell - 1, cell);
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
                        DecellSellect(num - 1);
                        DecellCentered(num);
                        DecellSellect(num + 1);
                        DecellSellect(num + 1 + wk.HorizonCell);
                        break;
                    case 2:
                        DecellSellect(num + wk.HorizonCell);
                        DecellCentered(num);
                        DecellSellect(num - wk.HorizonCell);
                        DecellSellect(num - 1 + wk.HorizonCell);
                        break;
                    case 3:
                        DecellSellect(num + 1);
                        DecellCentered(num);
                        DecellSellect(num - 1);
                        DecellSellect(num - 1 - wk.HorizonCell);
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
                        DecellSellect(num + wk.HorizonCell);
                        DecellCentered(num);
                        DecellSellect(num + 1);
                        DecellSellect(num - wk.HorizonCell + 1);
                        break;
                    case 3:
                        DecellSellect(num + 1);
                        DecellCentered(num);
                        DecellSellect(num - wk.HorizonCell);
                        DecellSellect(num - wk.HorizonCell - 1);
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
                        DecellSellect(num - 1);
                        DecellCentered(num);
                        DecellSellect(num + 1);
                        DecellSellect(num + 1 - wk.HorizonCell);
                        break;
                    case 2:
                        DecellSellect(num + wk.HorizonCell);
                        DecellCentered(num);
                        DecellSellect(num - wk.HorizonCell);
                        DecellSellect(num + 1 + wk.HorizonCell);
                        break;
                    case 3:
                        DecellSellect(num + 1);
                        DecellCentered(num);
                        DecellSellect(num - 1);
                        DecellSellect(num - 1 + wk.HorizonCell);
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
                        DecellSellect(num + 1);
                        DecellCentered(num);
                        DecellSellect(num + wk.HorizonCell);
                        DecellSellect(num + wk.HorizonCell - 1);
                        break;
                    case 2:
                        DecellSellect(num + wk.HorizonCell);
                        DecellCentered(num);
                        DecellSellect(num - 1);
                        DecellSellect(num - wk.HorizonCell - 1);
                        break;
                    case 3:
                        DecellSellect(num + 1);
                        DecellCentered(num);
                        DecellSellect(num + wk.HorizonCell);
                        DecellSellect(num + wk.HorizonCell - 1);
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
                        CFcellSellector(num - 1, cell);
                        CFcellCenteredmade(num, cell);
                        CFcellSellector(num + 1, cell);
                        CFcellSellector(num + 1 + wk.HorizonCell, cell);
                        break;
                    case 2:
                        CFcellSellector(num + wk.HorizonCell, cell);
                        CFcellCenteredmade(num, cell);
                        CFcellSellector(num - wk.HorizonCell, cell);
                        CFcellSellector(num - 1 + wk.HorizonCell, cell);
                        break;
                    case 3:
                        CFcellSellector(num + 1, cell);
                        CFcellCenteredmade(num, cell);
                        CFcellSellector(num - 1, cell);
                        CFcellSellector(num - 1 - wk.HorizonCell, cell);
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
                        CFcellSellector(num + wk.HorizonCell, cell);
                        CFcellCenteredmade(num, cell);
                        CFcellSellector(num + 1, cell);
                        CFcellSellector(num - wk.HorizonCell + 1, cell);
                        break;
                    case 3:
                        CFcellSellector(num + 1, cell);
                        CFcellCenteredmade(num, cell);
                        CFcellSellector(num - wk.HorizonCell, cell);
                        CFcellSellector(num - wk.HorizonCell - 1, cell);
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
                        CFcellSellector(num - 1, cell);
                        CFcellCenteredmade(num, cell);
                        CFcellSellector(num + 1, cell);
                        CFcellSellector(num + 1 - wk.HorizonCell, cell);
                        break;
                    case 2:
                        CFcellSellector(num + wk.HorizonCell, cell);
                        CFcellCenteredmade(num, cell);
                        CFcellSellector(num - wk.HorizonCell, cell);
                        CFcellSellector(num + 1 + wk.HorizonCell, cell);
                        break;
                    case 3:
                        CFcellSellector(num + 1, cell);
                        CFcellCenteredmade(num, cell);
                        CFcellSellector(num - 1, cell);
                        CFcellSellector(num - 1 + wk.HorizonCell, cell);
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
                        CFcellSellector(num + 1, cell);
                        CFcellCenteredmade(num, cell);
                        CFcellSellector(num + wk.HorizonCell, cell);
                        CFcellSellector(num + wk.HorizonCell - 1, cell);
                        break;
                    case 2:
                        CFcellSellector(num + wk.HorizonCell, cell);
                        CFcellCenteredmade(num, cell);
                        CFcellSellector(num - 1, cell);
                        CFcellSellector(num - wk.HorizonCell - 1, cell);
                        break;
                    case 3:
                        CFcellSellector(num + 1, cell);
                        CFcellCenteredmade(num, cell);
                        CFcellSellector(num + wk.HorizonCell, cell);
                        CFcellSellector(num + wk.HorizonCell - 1, cell);
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
        previousPoint = StartPoint;
    }
    void CompairSpawnBlock(int num, cellState cell, int rotateState)
    {
        switch (cell)
        {
            case cellState.Tstate:
                switch (rotateState)
                {
                    case 0:
                        ComcellCenteredmade(num, cell);
                        ComcellSellector(num - wk.HorizonCell, cell);
                        ComcellSellector(num - 1, cell);
                        ComcellSellector(num + 1, cell);
                        break;
                    case 1:
                        ComcellCenteredmade(num, cell);
                        ComcellSellector(num - wk.HorizonCell, cell);
                        ComcellSellector(num - 1, cell);
                        ComcellSellector(num + wk.HorizonCell, cell);
                        break;
                    case 2:
                        ComcellCenteredmade(num, cell);
                        ComcellSellector(num + wk.HorizonCell, cell);
                        ComcellSellector(num - 1, cell);
                        ComcellSellector(num + 1, cell);
                        break;
                    case 3:
                        ComcellCenteredmade(num, cell);
                        ComcellSellector(num - wk.HorizonCell, cell);
                        ComcellSellector(num + wk.HorizonCell, cell);
                        ComcellSellector(num + 1, cell);
                        break;
                }
                break;
            case cellState.Lstate:
                switch (rotateState)
                {
                    case 0:
                        ComcellSellector(num + wk.HorizonCell, cell);
                        ComcellCenteredmade(num, cell);
                        ComcellSellector(num - wk.HorizonCell, cell);
                        ComcellSellector(num - wk.HorizonCell + 1, cell);
                        break;
                    case 1:
                        ComcellSellector(num - 1, cell);
                        ComcellCenteredmade(num, cell);
                        ComcellSellector(num + 1, cell);
                        ComcellSellector(num + 1 + wk.HorizonCell, cell);
                        break;
                    case 2:
                        ComcellSellector(num + wk.HorizonCell, cell);
                        ComcellCenteredmade(num, cell);
                        ComcellSellector(num - wk.HorizonCell, cell);
                        ComcellSellector(num - 1 + wk.HorizonCell, cell);
                        break;
                    case 3:
                        ComcellSellector(num + 1, cell);
                        ComcellCenteredmade(num, cell);
                        ComcellSellector(num - 1, cell);
                        ComcellSellector(num - 1 - wk.HorizonCell, cell);
                        break;
                }
                break;
            case cellState.Sstate:
                switch (rotateState)
                {
                    case 0:
                        ComcellSellector(num + wk.HorizonCell, cell);
                        ComcellCenteredmade(num, cell);
                        ComcellSellector(num + 1, cell);
                        ComcellSellector(num - wk.HorizonCell + 1, cell);
                        break;
                    case 1:
                        ComcellSellector(num + 1, cell);
                        ComcellCenteredmade(num, cell);
                        ComcellSellector(num - wk.HorizonCell, cell);
                        ComcellSellector(num - wk.HorizonCell - 1, cell);
                        break;
                    case 2:
                        ComcellSellector(num + wk.HorizonCell, cell);
                        ComcellCenteredmade(num, cell);
                        ComcellSellector(num + 1, cell);
                        ComcellSellector(num - wk.HorizonCell + 1, cell);
                        break;
                    case 3:
                        ComcellSellector(num + 1, cell);
                        ComcellCenteredmade(num, cell);
                        ComcellSellector(num - wk.HorizonCell, cell);
                        ComcellSellector(num - wk.HorizonCell - 1, cell);
                        break;
                }
                break;
            ///아래 모양 무슨 모양인지 모르겠음
            case cellState.Rstate:
                switch (rotateState)
                {
                    case 0:
                        ComcellCenteredmade(num, cell);
                        ComcellSellector(num - wk.HorizonCell, cell);
                        ComcellSellector(num - 1, cell);
                        ComcellSellector(num + 1, cell);
                        break;
                    case 1:
                        ComcellCenteredmade(num, cell);
                        ComcellSellector(num - wk.HorizonCell, cell);
                        ComcellSellector(num - 1, cell);
                        ComcellSellector(num + 1, cell);
                        break;
                    case 2:
                        ComcellCenteredmade(num, cell);
                        ComcellSellector(num - wk.HorizonCell, cell);
                        ComcellSellector(num - 1, cell);
                        ComcellSellector(num + 1, cell);
                        break;
                    case 3:
                        ComcellCenteredmade(num, cell);
                        ComcellSellector(num - wk.HorizonCell, cell);
                        ComcellSellector(num - 1, cell);
                        ComcellSellector(num + 1, cell);
                        break;
                }
                break;
            case cellState.ReLstate:
                switch (rotateState)
                {
                    case 0:
                        ComcellSellector(num + wk.HorizonCell, cell);
                        ComcellCenteredmade(num, cell);
                        ComcellSellector(num - wk.HorizonCell, cell);
                        ComcellSellector(num - wk.HorizonCell - 1, cell);
                        break;
                    case 1:
                        ComcellSellector(num - 1, cell);
                        ComcellCenteredmade(num, cell);
                        ComcellSellector(num + 1, cell);
                        ComcellSellector(num + 1 - wk.HorizonCell, cell);
                        break;
                    case 2:
                        ComcellSellector(num + wk.HorizonCell, cell);
                        ComcellCenteredmade(num, cell);
                        ComcellSellector(num - wk.HorizonCell, cell);
                        ComcellSellector(num + 1 + wk.HorizonCell, cell);
                        break;
                    case 3:
                        ComcellSellector(num + 1, cell);
                        ComcellCenteredmade(num, cell);
                        ComcellSellector(num - 1, cell);
                        ComcellSellector(num - 1 + wk.HorizonCell, cell);
                        break;
                }
                break;
            case cellState.ReSstate:
                switch (rotateState)
                {
                    case 0:
                        ComcellSellector(num + wk.HorizonCell, cell);
                        ComcellCenteredmade(num, cell);
                        ComcellSellector(num - 1, cell);
                        ComcellSellector(num - wk.HorizonCell - 1, cell);
                        break;
                    case 1:
                        ComcellSellector(num + 1, cell);
                        ComcellCenteredmade(num, cell);
                        ComcellSellector(num + wk.HorizonCell, cell);
                        ComcellSellector(num + wk.HorizonCell - 1, cell);
                        break;
                    case 2:
                        ComcellSellector(num + wk.HorizonCell, cell);
                        ComcellCenteredmade(num, cell);
                        ComcellSellector(num - 1, cell);
                        ComcellSellector(num - wk.HorizonCell - 1, cell);
                        break;
                    case 3:
                        ComcellSellector(num + 1, cell);
                        ComcellCenteredmade(num, cell);
                        ComcellSellector(num + wk.HorizonCell, cell);
                        ComcellSellector(num + wk.HorizonCell - 1, cell);
                        break;
                }
                break;
            case cellState.Square:
                ComcellCenteredmade(num, cell);
                ComcellSellector(num - wk.HorizonCell, cell);
                ComcellSellector(num - 1 - wk.HorizonCell, cell);
                ComcellSellector(num - 1, cell);
                break;
            case cellState.Bar:
                switch (rotateState)
                {
                    case 0:
                        ComcellSellector(num + wk.HorizonCell, cell);
                        ComcellCenteredmade(num, cell);
                        ComcellSellector(num - wk.HorizonCell, cell);
                        ComcellSellector(num - wk.HorizonCell * 2, cell);
                        break;
                    case 1:
                        ComcellSellector(num - 1, cell);
                        ComcellCenteredmade(num, cell);
                        ComcellSellector(num + 1, cell);
                        ComcellSellector(num + 2, cell);
                        break;
                    case 2:
                        ComcellSellector(num - wk.HorizonCell, cell);
                        ComcellCenteredmade(num, cell);
                        ComcellSellector(num + wk.HorizonCell, cell);
                        ComcellSellector(num + wk.HorizonCell * 2, cell);
                        break;
                    case 3:
                        ComcellSellector(num + 1, cell);
                        ComcellCenteredmade(num, cell);
                        ComcellSellector(num - 1, cell);
                        ComcellSellector(num - 2, cell);
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
