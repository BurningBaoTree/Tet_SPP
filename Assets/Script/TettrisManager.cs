using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TettrisManager : MonoBehaviour
{
    PlayerInput playerinput;

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
            }
        }
    }

    WorkArea wk;
    CoolTimeSys cool;

    cellState cellClass;

    /// <summary>
    /// 시작지점
    /// </summary>
    int StartPoint;

    /// <summary>
    /// 가운데 블록의 현재 지점
    /// </summary>
    int NowStatePoint;

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
        playerinput.Enable();
        playerinput.Player.MoveBlock.performed += MoveBlock;
    }

    private void Start()
    {
        //시작지점 계산 가로*세로 - 가로의 가운데지점을 int로 환산한 값(즉 제일 위에 가운데 번호의 셀)
        StartPoint = wk.HorizonCell * wk.VerticalCell - (int)(wk.HorizonCell * 0.5f);
        NowStatePoint = StartPoint;
        updater += fallDownBlock;
    }
    private void Update()
    {
        updater();
    }


    /// <summary>
    /// 블록 스폰하는 함수 
    /// </summary>
    void SpawnBlock(cellState cell)
    {
        switch (cell)
        {
            case cellState.Empty:

                break;
            case cellState.Tstate:

                break;
            case cellState.Lstate:

                break;
            case cellState.Sstate:

                break;
            case cellState.Rstate:

                break;
            case cellState.ReLstate:

                break;
            case cellState.ReSstate:

                break;
            case cellState.Square:

                break;
            case cellState.Bar:

                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 스타트 포인트에서 블록이 떨어지게 하는 함수
    /// </summary>
    void fallDownBlock()
    {
        if (cool.coolclocks[0].coolEnd && NowStatePoint > -1)
        {
            wk.cells[NowStatePoint].CellState = cellState.ReLstate;
            wk.cells[NowStatePoint].IsCenter = true;
            cool.CoolTimeStart(0, 1f);
            NowStatePoint -= wk.HorizonCell;
        }
        else
        {
            updater -= fallDownBlock;
        }
    }

    private void MoveBlock(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {

    }
}
//이 코드에서 할일 : 조작, 바닥에 닿았다는 신호 받아오기, 턴 관리, 시간 관리,
