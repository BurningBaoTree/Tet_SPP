using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TettrisManager : MonoBehaviour
{
    PlayerInput playerinput;

    /// <summary>
    /// �̹� �� üũ��
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
    /// ��������
    /// </summary>
    int StartPoint;

    /// <summary>
    /// ��� ����� ���� ����
    /// </summary>
    int NowStatePoint;

    /// <summary>
    /// ������Ʈ ����� ��������Ʈ
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
        //�������� ��� ����*���� - ������ ��������� int�� ȯ���� ��(�� ���� ���� ��� ��ȣ�� ��)
        StartPoint = wk.HorizonCell * wk.VerticalCell - (int)(wk.HorizonCell * 0.5f);
        NowStatePoint = StartPoint;
        updater += fallDownBlock;
    }
    private void Update()
    {
        updater();
    }


    /// <summary>
    /// ��� �����ϴ� �Լ� 
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
    /// ��ŸƮ ����Ʈ���� ����� �������� �ϴ� �Լ�
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
//�� �ڵ忡�� ���� : ����, �ٴڿ� ��Ҵٴ� ��ȣ �޾ƿ���, �� ����, �ð� ����,
