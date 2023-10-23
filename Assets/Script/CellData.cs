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
    Rstate,
    ReLstate,
    ReSstate,
    quare
}

/// <summary>
/// �� �����͸� �����ϴ� Ŭ����
/// </summary>
public class CellData : MonoBehaviour
{
    public bool IsCenter;
    public bool Activated = false;
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
                    case cellState.quare:
                        spriteRenderer.color = quareColor;
                        break;
                    default:
                        break;
                }
            }
        }
    }

    public Color EmptyColor;
    public Color TstateColor;
    public Color LstateColor;
    public Color SstateColor;
    public Color RstateColor;
    public Color ReLstateColor;
    public Color ReSstateColor;
    public Color quareColor;

    SpriteRenderer spriteRenderer;
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
}
