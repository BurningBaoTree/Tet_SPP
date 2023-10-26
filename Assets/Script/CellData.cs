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
/// �� �����͸� �����ϴ� Ŭ����
/// </summary>
public class CellData : MonoBehaviour
{
    /// <summary>
    /// ���� �� üũ
    /// </summary>
    public bool EndCell = false;

    /// <summary>
    /// ���� �� üũ
    /// </summary>
    public bool RightEnd = false;

    /// <summary>
    /// ���� �� üũ
    /// </summary>
    public bool LeftEnd = false;

    /// <summary>
    /// ��� üũ
    /// </summary>
    public bool IsCenter = false;

    public bool IsSet = false;
    public bool HoldCheck = false;

    public Action<bool> RightReached;
    public Action<bool> LeftReached;
    public Action ReachedTheEnd;
    /// <summary>
    /// �۵������� üũ��
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
    /// ���� Ÿ���� ���� �ѹ�
    /// </summary>
    public int number;

    /// <summary>
    /// Ÿ���� ����
    /// </summary>
    public TileBase tile;

    /// <summary>
    /// ���� ����
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
    /// �� Ÿ�Ը��� ����
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

// ��� ���� �������� �� �̰���, ��� ������, �غ�� ����� �ν� , ȸ���� �̵��� ���⼭ 
