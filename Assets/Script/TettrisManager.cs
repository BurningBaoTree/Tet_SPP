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
    /// �̹� �� üũ�� ������Ƽ
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
    /// ���� ȭ�� ������Ʈ
    /// </summary>
    WorkArea wk;

    /// <summary>
    /// ��Ÿ�� �ý���
    /// </summary>
    CoolTimeSys cool;

    /// <summary>
    /// ��� �̵� ���� Vector2 ����
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
    /// ���� �������� ��� ����
    /// </summary>
    public cellState cellClass = cellState.Tstate;

    /// <summary>
    /// �����̼� ������ int ����
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
    /// ��������
    /// </summary>
    int StartPoint;

    /// <summary>
    /// ��� ����� ���� ����
    /// </summary>
    int NowStatePoint;

    /// <summary>
    /// ���� ����
    /// </summary>
    int previousPoint;

    /// <summary>
    /// ���� ����
    /// </summary>
    int setPoint;

    /// <summary>
    /// ������Ʈ ����� ��������Ʈ
    /// </summary>
    Action updater;


    //�Ϻ� ������Ʈ ��������
    private void Awake()
    {
        playerinput = new PlayerInput();
        wk = GetComponent<WorkArea>();
        cool = GetComponent<CoolTimeSys>();
    }

    //��� ���� �Լ� ���� �� input�ý���
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

    //�������� ����, �� ���� ����
    private void Start()
    {
        //�������� ��� ����*���� - ������ ��������� int�� ȯ���� ��(�� ���� ���� ��� ��ȣ�� ��)
        StartPoint = wk.HorizonCell * wk.VerticalCell - (int)(wk.HorizonCell * 0.5f);
        TurnActive = true;
    }
    private void Update()
    {
        updater();
    }

    /// <summary>
    /// ��ŸƮ ����Ʈ���� ����� �������� �ϴ� �Լ�
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
    /// ��� ���� ���� �Լ�
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
    /// ���� �������� �˸��� �Լ�(IsEnd��������Ʈ�� ����Ǿ�����)
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
    /// ���������� �̵��Ҽ� �ִ��� üũ�� �Լ�
    /// </summary>
    /// <param name="moveable"></param>
    void RightCheck(bool moveable)
    {
        RigthmoveCheck = moveable;
    }

    /// <summary>
    /// �������� �̵��Ҽ� �ִ��� üũ�� �Լ�
    /// </summary>
    /// <param name="moveable"></param>
    void LeftCheck(bool moveable)
    {
        LeftmoveCheck = moveable;
    }

    /// <summary>
    /// ��ǲ �׼� �̵� �Լ�
    /// </summary>
    /// <param name="context"></param>
    private void MoveBlock(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        Inputcontext = context.ReadValue<Vector2>();
        Debug.Log("ȣ��" + Inputcontext);
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
    /// ��ǲ�׼� ����� �� �Ʒ��� ���������� �Լ�
    /// </summary>
    /// <param name="context"></param>
    private void DownBlockImmidiately(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        updater += DownMove;
    }

    /// <summary>
    /// ��ǲ�׼� ����� ȸ����Ű�� �Լ�
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
    /// ���� �̵� �Լ�
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
    /// ������ �̵� �Լ�
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
    /// �Ʒ��� �̵� �Լ�
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
    /// ������ ȸ�� �Լ�
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
    /// ���� ȸ�� �Լ�
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
    /// ���� ������ ���� ������ Ȯ���Ͽ� ������ �� ������� �� ������ ������� ���� ����� �����´�.
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
        //����Ʈ�� �ִ� �� �� �����
        foreach (CellData cell in blockGroup)
        {
            cell.IsSet = false;
            cell.IsActivated = false;
            cell.HoldCheck = false;
            cell.RightEnd = false;
            cell.LeftEnd = false;
        }
        blockGroup.Clear();
        //���� ��ĭ�� ���ܿ���
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
        // �� �� �� �����
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
    /// ���� ���� �����ϸ� �Ϻ� ������ �ʱ�ȭ �� �������Ϳ� �Լ� ����
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
    //------------------------------<��� ���� ����>--------------------------------------------
    #region ��� ���� ����
    /// <summary>
    /// ���� ���ǿ� �����Լ�
    /// </summary>
    /// <param name="num">cell����Ʈ�� index��</param>
    /// <param name="cell">���Ϻ� cell ���� ���ÿ�</param>
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
    /// ���� ���ǿ� �����Լ�(��� ����)
    /// </summary>
    /// <param name="num">cell����Ʈ�� index��</param>
    /// <param name="cell">���Ϻ� cell ���� ���ÿ�</param>
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
    /// ���� ���ǿ� �����Լ�
    /// </summary>
    /// <param name="num">cell����Ʈ�� index��</param>
    /// <param name="cell">���Ϻ� cell ���� ���ÿ�</param>
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

                //�� �����ٿ��� ���ʰ� ������ ���� ����ؼ� �����ؾ��Ѵ�.
                if (11 != num % wk.cells.Count)
                {
                    //����ĭ�� RightEnd
                    wk.cells[num + 1].RightEnd = true;
                }
                if (0 != num % wk.cells.Count)
                {
                    //������ ĭ�� LeftEnd
                    wk.cells[num - 1].LeftEnd = true;
                }
            }
        }
        catch
        {
            Debug.Log("���ӿ���");
        }
    }
    /// <summary>
    /// ���� ���ǿ� �����Լ�(��� ����)
    /// </summary>
    /// <param name="num">cell����Ʈ�� index��</param>
    /// <param name="cell">���Ϻ� cell ���� ���ÿ�</param>
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
            Debug.Log("���ӿ���");
        }
    }

    /// <summary>
    /// ���� ���ǿ� �����Լ�
    /// </summary>
    /// <param name="num">cell����Ʈ�� index��</param>
    /// <param name="cell">���Ϻ� cell ���� ���ÿ�</param>
    void DecellSellect(int num)
    {
        if (num < wk.cells.Count && num > -1)
        {
            wk.cells[num].IsActivated = false;
        }
    }
    /// <summary>
    /// ���� ���ǿ� �����Լ�(��� ����)
    /// </summary>
    /// <param name="num">cell����Ʈ�� index��</param>
    /// <param name="cell">���Ϻ� cell ���� ���ÿ�</param>
    void DecellCentered(int num)
    {
        if (num < wk.cells.Count && num > -1)
        {
            wk.cells[num].IsActivated = false;
            wk.cells[num].IsCenter = false;
        }
    }
    /// <summary>
    /// ���� ���ǿ� �����Լ�
    /// </summary>
    /// <param name="num">cell����Ʈ�� index��</param>
    /// <param name="cell">���Ϻ� cell ���� ���ÿ�</param>
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
    /// ���� ���ǿ� �����Լ�(��� ����)
    /// </summary>
    /// <param name="num">cell����Ʈ�� index��</param>
    /// <param name="cell">���Ϻ� cell ���� ���ÿ�</param>
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
    #region ��� ���� ����
    /// <summary>
    /// ��� �̵��� ����� ����ϴ� �Լ�
    /// </summary>
    /// <param name="num">�ε���</param>
    /// <param name="cell">�� ����</param>
    /// <param name="rotateState">ȸ����</param>
    /// <param name="DeletPrevious">������ �������� ���� ����</param>
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
            ///�Ʒ� ��� ���� ������� �𸣰���
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
            ///�Ʒ� ��� ���� ������� �𸣰���
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
            ///�Ʒ� ��� ���� ������� �𸣰���
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
            ///�Ʒ� ��� ���� ������� �𸣰���
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


//�� �ڵ忡�� ���� : ����, �ٴڿ� ��Ҵٴ� ��ȣ �޾ƿ���, �� ����, �ð� ����,
