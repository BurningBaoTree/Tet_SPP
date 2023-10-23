using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TettrisManager : MonoBehaviour
{
    bool startActive;
    public bool StartActive
    {
        get
        {
            return startActive;
        }
        set
        {
            if (startActive != value)
            {
                startActive = value;
            }
        }
    }

    WorkArea wk;
    CoolTimeSys cool;

    int StartPoint;
    int copyStartpoint;

    Action updater;

    private void Awake()
    {
        wk = GetComponent<WorkArea>();
    }
    private void OnEnable()
    {
        cool = GetComponent<CoolTimeSys>();
    }
    private void Start()
    {
        StartPoint = wk.HorizonCell * wk.VerticalCell - (int)(wk.HorizonCell * 0.5f);
        copyStartpoint = StartPoint;
        updater += fallDownBlock;
    }
    void fallDownBlock()
    {
        if (cool.coolclocks[0].coolEnd)
        {
            wk.cells[copyStartpoint].CellState = cellState.ReLstate;
            wk.cells[copyStartpoint].IsCenter = true;
            cool.CoolTimeStart(0, 1f);
            copyStartpoint -= wk.HorizonCell;
        }
    }

    private void Update()
    {
        updater();
    }

}
