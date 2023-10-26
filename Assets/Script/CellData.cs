using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum cellState
{
    Empty = 0,
    Tstate,
    Lstate,
    Sstate,
    ReLstate,
    ReSstate,
    Square,
    Bar,
    Invisival,
    Rstate
}

/// <summary>
/// 셀 데이터를 저장하는 클래스
/// </summary>
public class CellData : MonoBehaviour
{
    /// <summary>
    /// 양쪽 끝 체크
    /// </summary>
    public bool EndCell = false;

    /// <summary>
    /// 양쪽 끝 체크
    /// </summary>
    public bool RightEnd = false;

    /// <summary>
    /// 양쪽 끝 체크
    /// </summary>
    public bool LeftEnd = false;

    /// <summary>
    /// 가운데 체크
    /// </summary>
    public bool IsCenter = false;

    public bool IsSet = false;
    public bool HoldCheck = false;

    public Action<bool> RightReached;
    public Action<bool> LeftReached;
    public Action ReachedTheEnd;
    /// <summary>
    /// 작동중인지 체크용
    /// </summary>
    public bool isActivated = false;
    public bool IsActivated
    {
        get
        {
            return isActivated;
        }
        set
        {
            if (isActivated != value)
            {
                isActivated = value;
                if (!isActivated)
                {
                    CellState = cellState.Empty;
                }
                else
                {
                    if (RightEnd)
                    {
                        RightReached?.Invoke(false);
                    }
                    if (LeftEnd)
                    {
                        LeftReached?.Invoke(false);
                    }
                    if (EndCell)
                    {
                        ReachedTheEnd?.Invoke();
                    }
                }
            }
        }
    }

    /// <summary>
    /// 현제 타일의 제조 넘버
    /// </summary>
    public int number;

    /// <summary>
    /// 타일의 상태
    /// </summary>
    public TileBase tile;

    /// <summary>
    /// 쎌의 상태
    /// </summary>
    public cellState cellState;
    public cellState CellState
    {
        get
        {
            return cellState;
        }
        set
        {
            if (cellState != value)
            {
                cellState = value;
                switch (cellState)
                {
                    case cellState.Empty:
                        spriteRenderer.color = EmptyColor;
                        break;
                    case cellState.Tstate:
                        spriteRenderer.color = TstateColor;
                        break;
                    case cellState.Lstate:
                        spriteRenderer.color = LstateColor;
                        break;
                    case cellState.Sstate:
                        spriteRenderer.color = SstateColor;
                        break;
                    case cellState.Rstate:
                        spriteRenderer.color = RstateColor;
                        break;
                    case cellState.ReLstate:
                        spriteRenderer.color = ReLstateColor;
                        break;
                    case cellState.ReSstate:
                        spriteRenderer.color = ReSstateColor;
                        break;
                    case cellState.Square:
                        spriteRenderer.color = quareColor;
                        break;
                    case cellState.Bar:
                        spriteRenderer.color = BarClolr;
                        break;
                    case cellState.Invisival:
                        spriteRenderer.color = InvisivalColor;
                        break;
                    default:
                        break;
                }
            }
        }
    }

    /// <summary>
    /// 각 타입마다 색상
    /// </summary>
    public Color EmptyColor;
    public Color TstateColor;
    public Color LstateColor;
    public Color SstateColor;
    public Color RstateColor;
    public Color ReLstateColor;
    public Color ReSstateColor;
    public Color quareColor;
    public Color BarClolr;
    public Color InvisivalColor;

    SpriteRenderer spriteRenderer;
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
}

// 블록 관련 로직들은 다 이곳에, 블록 지나감, 밑블록 옆블록 인식 , 회전과 이동도 여기서 
